using System.Data;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Services;
public class FinanceService
{
    private readonly MailService _mail;
    private readonly EduFlowDbContext _db;
    private readonly PaymentGatewayService _gateways;
    public bool SimulationEnabled => _gateways.DemoEnabled;
    public FinanceService(EduFlowDbContext db, PaymentGatewayService gateways, MailService mail)
    {
        _db = db;
        _gateways = gateways;
        _mail = mail;
    }

    // Báo cáo theo ngày tạo đơn; chỉ lấy đơn đã thanh toán và có giao dịch xác nhận.
    public async Task<RevenueViewModel> GetRevenueAsync(long? instructorId, DateTime fromDate, DateTime toDate, bool isTest)
    {
        isTest = SimulationEnabled || isTest;
        var model = new RevenueViewModel { SimulationEnabled = SimulationEnabled, From = fromDate, To = toDate, IsTest = isTest, IsAdmin = instructorId == null };
        var end = toDate.AddDays(1);
        var items = from item in _db.OrderItems.AsNoTracking()
                    join course in _db.Courses on item.CourseId equals course.Id
                    where item.Order!.Status == OrderStatus.Paid
                        && item.Order.CreatedAt >= fromDate && item.Order.CreatedAt < end
                        && _db.Payments.Any(p => p.OrderId == item.OrderId && p.Status == "success"
                            && p.IsTest == isTest && (isTest || p.Gateway != "demo"))
                    select new { item, course };
        if (instructorId.HasValue)
            items = items.Where(x => x.course.InstructorId == instructorId.Value);
        model.Courses = await items.GroupBy(x => new { x.course.Id, x.course.Title, Instructor = x.course.Instructor!.FullName })
            .Select(g => new CourseRevenueRow { CourseId = g.Key.Id, Title = g.Key.Title, Instructor = g.Key.Instructor,
                Sales = g.Count(), Revenue = g.Sum(x => x.item.Price), InstructorIncome = g.Sum(x => x.item.InstructorEarning) })
            .OrderByDescending(x => x.Revenue).ThenBy(x => x.CourseId).ToListAsync();
        // Tiền mô phỏng và tiền thật luôn tính trong hai sổ riêng, không cộng lẫn.
        var payouts = _db.PayoutRequests.AsNoTracking().Where(p => p.IsTest == isTest);
        if (instructorId.HasValue)
            payouts = payouts.Where(p => p.InstructorId == instructorId.Value);
        model.HeldAmount = await payouts.Where(p => p.Status == "pending" || p.Status == "approved").SumAsync(p => (decimal?)p.Amount) ?? 0;
        model.PaidAmount = await payouts.Where(p => p.Status == "paid").SumAsync(p => (decimal?)p.Amount) ?? 0;
        model.WithheldTax = await payouts.Where(p => p.Status == "paid").SumAsync(p => p.TaxAmount) ?? 0;
        model.NetPaid = await payouts.Where(p => p.Status == "paid" && p.TaxAmount != null).SumAsync(p => (decimal?)(p.Amount - p.TaxAmount!.Value)) ?? 0;
        model.RemittedTax = await payouts.Where(p => p.Status == "paid" && p.TaxRemittedOn != null).SumAsync(p => p.TaxAmount) ?? 0;
        if (instructorId.HasValue)
        {
            model.WithdrawableBalance = await GetWithdrawableBalanceAsync(instructorId.Value, isTest);
            model.CurrentInstructorRate = await _db.InstructorProfiles.Where(p => p.UserId == instructorId.Value)
                .Select(p => (decimal?)p.CommissionRate).FirstOrDefaultAsync();
        }
        return model;
    }

    public async Task<decimal> GetWithdrawableBalanceAsync(long instructorId, bool? isTest = null)
    {
        var test = isTest ?? SimulationEnabled;
        var paidOrders = _db.Payments.Where(p => p.IsTest == test && p.Status == "success" && (test || p.Gateway != "demo")).Select(p => p.OrderId);
        var earned = await (from item in _db.OrderItems
                            join course in _db.Courses on item.CourseId equals course.Id
                            where course.InstructorId == instructorId && item.Order!.Status == OrderStatus.Paid
                                && paidOrders.Contains(item.OrderId)
                            select (decimal?)item.InstructorEarning).SumAsync() ?? 0;
        var withdrawnOrHeld = await _db.PayoutRequests.Where(p => p.InstructorId == instructorId
            && p.IsTest == test && (p.Status == "pending" || p.Status == "approved" || p.Status == "paid"))
            .SumAsync(p => (decimal?)p.Amount) ?? 0;
        return Math.Max(0, earned - withdrawnOrHeld);
    }

    public async Task<string?> RequestAsync(long instructorId, decimal amount, bool taxDeclared = false, bool voluntaryTax = false, string? taxIdentifier = null)
    {
        if (amount <= 0 || amount > 9999999999999m || amount != decimal.Truncate(amount))
            return "Nhập số tiền VND nguyên dương.";
        if (!taxDeclared) return "Xác nhận đối tượng nhận thù lao trước khi gửi yêu cầu.";
        var tax = PersonalTax.Calculate(amount, DateTime.Today, voluntaryTax);
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var profile = await _db.InstructorProfiles.FromSqlInterpolated($"SELECT * FROM instructor_profiles WITH (UPDLOCK,HOLDLOCK) WHERE user_id={instructorId}").FirstOrDefaultAsync();
        if (profile == null || profile.KycStatus != KycStatus.Approved || string.IsNullOrWhiteSpace(profile.BankAccountNumber) || string.IsNullOrWhiteSpace(profile.BankName) || string.IsNullOrWhiteSpace(profile.BankAccountName))
            return "Hồ sơ ngân hàng chưa đầy đủ hoặc chưa được duyệt.";
        taxIdentifier = (taxIdentifier ?? profile.TaxId ?? profile.IdentityCard)?.Trim();
        if (taxIdentifier == null || !System.Text.RegularExpressions.Regex.IsMatch(taxIdentifier, @"^(\d{10}(-\d{3})?|\d{12})$"))
            return "Nhập MST hoặc số định danh thuế hợp lệ.";
        if (await _db.PayoutRequests.AnyAsync(p => p.InstructorId == instructorId && p.IsTest == SimulationEnabled && (p.Status == "pending" || p.Status == "approved")))
            return "Bạn đã có yêu cầu đang xử lý. Chờ xử lý xong trước khi gửi yêu cầu mới.";
        var payee = await _db.Users.Where(u => u.Id == instructorId).Select(u => u.FullName).FirstAsync();
        var available = await GetWithdrawableBalanceAsync(instructorId);
        if (amount > available)
            return "Số tiền vượt quá số dư khả dụng.";
        if (!SimulationEnabled) profile.Balance = available - amount; // Giữ tiền ngay khi gửi yêu cầu; từ chối thì trả lại đúng một lần.
        _db.PayoutRequests.Add(new() { IsTest = SimulationEnabled, InstructorId = instructorId, Amount = amount, TaxDeclared = true, TaxVoluntary = voluntaryTax, TaxRule = PersonalTax.Rule, TaxRate = tax.Rate, TaxAmount = tax.Tax, TaxIdSnapshot = taxIdentifier, PayeeNameSnapshot = payee, BankNameSnapshot = profile.BankName, BankNumberSnapshot = profile.BankAccountNumber, BankOwnerSnapshot = profile.BankAccountName });
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return null;
    }

    public async Task<string?> ProcessAsync(long id, string action, string? note, string? receipt, bool taxConfirmed = false, long? adminId = null, DateTime? paidOn = null)
    {
        if (action is not ("approved" or "rejected" or "paid"))
            return "Thao tác không hợp lệ.";
        if (note?.Length > 500)
            return "Ghi chú tối đa 500 ký tự.";
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var request = await _db.PayoutRequests.FromSqlInterpolated($"SELECT * FROM payout_requests WITH (UPDLOCK,HOLDLOCK) WHERE id={id}").FirstOrDefaultAsync();
        if (request == null)
            return "Không tìm thấy yêu cầu.";
        if (request.IsTest != SimulationEnabled) return "Yêu cầu thuộc chế độ khác, không được xử lý chung tiền thật và mô phỏng.";
        if (SimulationEnabled && action == "paid" && !paidOn.HasValue) paidOn = DateTime.Today;
        if (action == "approved" && request.Status != "pending" || action == "paid" && request.Status != "approved" || action == "rejected" && request.Status is not ("pending" or "approved"))
            return "Yêu cầu đã được xử lý hoặc trạng thái không phù hợp.";
        if (action is "approved" or "paid")
        {
            if (action == "paid" && (!paidOn.HasValue || paidOn.Value.Date > DateTime.Today || paidOn.Value.Date < request.TaxReviewedAt?.Date))
                return "Nhập ngày chuyển khoản thực tế từ ngày duyệt đến hôm nay.";
            if (!request.TaxDeclared || string.IsNullOrWhiteSpace(request.TaxIdSnapshot) || !request.TaxAmount.HasValue)
                return "Yêu cầu thiếu hồ sơ thuế. Từ chối để giảng viên gửi lại, không tự sửa dữ liệu cũ.";
            var tax = PersonalTax.Calculate(request.Amount, action == "paid" ? paidOn!.Value : DateTime.Today, request.TaxVoluntary);
            if (action == "approved")
            {
                if (!taxConfirmed || !adminId.HasValue) return "Xác nhận đã kiểm tra đối tượng, hợp đồng và thông tin thuế.";
                request.TaxRate = tax.Rate;
                request.TaxAmount = tax.Tax;
                request.TaxRule = PersonalTax.Rule;
                request.TaxReviewedBy = adminId;
                request.TaxReviewedAt = DateTime.Now;
            }
            else if (!request.TaxReviewedAt.HasValue || request.TaxRule != PersonalTax.Rule || request.TaxAmount != tax.Tax || request.TaxRate != tax.Rate)
                return "Chính sách/hồ sơ thuế đã thay đổi hoặc chưa được duyệt; không xác nhận chi trả.";
        }
        if (action == "paid" && !SimulationEnabled && string.IsNullOrEmpty(receipt))
            return "Tải biên lai chuyển khoản trước khi xác nhận đã trả.";
        if (action == "rejected")
        {
            if (string.IsNullOrWhiteSpace(note))
                return "Nhập lý do từ chối.";
            if (!SimulationEnabled) await _db.InstructorProfiles.Where(profile => profile.UserId == request.InstructorId).ExecuteUpdateAsync(update => update.SetProperty(profile => profile.Balance, profile => profile.Balance + request.Amount));
        }

        request.Status = action;
        request.AdminNote = note;
        request.ProcessedAt = DateTime.Now;
        if (action == "paid")
        {
            request.ReceiptUrl = receipt;
            request.PaidAt = paidOn!.Value.Date;
        }
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        await _mail.SendUserAsync(request.InstructorId,"Kết quả yêu cầu rút tiền"+(request.IsTest?" mô phỏng":""),$"Yêu cầu #{request.Id}: {EduFlow.Web.Helpers.UiText.PayoutStatus(request.Status)}. Trước thuế: {EduFlow.Web.Helpers.UiText.Money(request.Amount)}; thực nhận dự kiến: {EduFlow.Web.Helpers.UiText.Money(request.NetAmount??0)}.\n{note}");
        return null;
    }

    public async Task<string?> MarkTaxRemittedAsync(long id, DateTime remittedOn, string? reference, long adminId)
    {
        if (string.IsNullOrWhiteSpace(reference) || reference.Trim().Length > 200)
            return "Nhập mã chứng từ nộp thuế (tối đa 200 ký tự).";
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var request = await _db.PayoutRequests.FromSqlInterpolated($"SELECT * FROM payout_requests WITH (UPDLOCK,HOLDLOCK) WHERE id={id}").FirstOrDefaultAsync();
        if (request == null || request.IsTest != SimulationEnabled || request.Status != "paid" || request.TaxAmount is not > 0 || !request.PaidAt.HasValue)
            return "Chỉ ghi nhận nộp thuế cho khoản đã chi và có thuế khấu trừ.";
        if (request.TaxRemittedOn.HasValue) return "Khoản thuế này đã được ghi nhận nộp.";
        if (remittedOn.Date < request.PaidAt.Value.Date || remittedOn.Date > DateTime.Today)
            return "Ngày nộp thuế phải từ ngày chi trả đến hôm nay.";
        request.TaxRemittedOn = remittedOn.Date;
        request.TaxRemittanceRef = reference.Trim();
        request.TaxRemittedBy = adminId;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return null;
    }

    public async Task<string?> SaveCouponAsync(Coupon model)
    {
        model.Code = model.Code.Trim().ToUpperInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(model.Code, "^[A-Z0-9_-]{2,50}$"))
            return "Mã gồm 2–50 chữ in hoa, số, dấu _ hoặc -.";
        if (model.DiscountType is not ("percent" or "fixed") || model.DiscountValue <= 0 || model.DiscountValue != decimal.Truncate(model.DiscountValue) || model.DiscountValue > 100000000 || model.DiscountType == "percent" && model.DiscountValue > 100)
            return "Mức giảm giá không hợp lệ.";
        if (model.EndDate <= model.StartDate || model.MaxDiscount is <= 0 || model.TotalLimit is <= 0)
            return "Kiểm tra thời gian, giới hạn lượt dùng và mức giảm tối đa.";
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (await _db.Coupons.AnyAsync(coupon => coupon.Code == model.Code && coupon.Id != model.Id))
            return "Mã đã tồn tại.";
        Coupon coupon;
        if (model.Id == 0)
        {
            coupon = new();
            _db.Coupons.Add(coupon);
        }
        else
        {
            coupon = (await _db.Coupons.FindAsync(model.Id))!;
            if (coupon == null)
                return "Không tìm thấy mã.";
            // Mã đã gắn vào đơn phải giữ điều kiện cũ; quản trị viên chỉ được bật hoặc tắt.
            if (await _db.Orders.AnyAsync(o => o.CouponId == coupon.Id))
                return "Mã đã gắn với đơn hàng. Chỉ bật/tắt mã, tạo mã mới nếu cần đổi điều kiện.";
        }

        coupon.Code = model.Code;
        coupon.DiscountType = model.DiscountType;
        coupon.DiscountValue = model.DiscountValue;
        coupon.MaxDiscount = model.MaxDiscount;
        coupon.TotalLimit = model.TotalLimit;
        coupon.StartDate = model.StartDate;
        coupon.EndDate = model.EndDate;
        coupon.IsActive = model.IsActive;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return null;
    }
}



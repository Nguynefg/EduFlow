using System.Data;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Services;
public class CommerceService
{
    private readonly MailService _mail;
    private readonly EduFlowDbContext _db;
    private readonly PaymentGatewayService _gateways;
    public CommerceService(EduFlowDbContext db, PaymentGatewayService gateways, MailService mail)
    {
        _db = db;
        _gateways = gateways;
        _mail = mail;
    }

    // Thêm vào giỏ: kiểm tra khóa còn bán, chưa mua và không phải khóa của mình.
    public async Task<string?> AddAsync(long userId, long courseId)
    {
        var course = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(course => course.Id == courseId && course.Status == CourseStatus.Published && course.DeletedAt == null && course.Instructor!.IsActive && course.Instructor.DeletedAt == null);
        if (course == null)
            return "Khóa học không còn mở bán.";
        if (course.InstructorId == userId)
            return "Bạn không thể mua khóa học của mình.";
        if (await _db.Enrollments.AnyAsync(enrollment => enrollment.UserId == userId && enrollment.CourseId == courseId))
            return "Bạn đã đăng ký hoặc bị thu hồi quyền học khóa này.";
        if (!await _db.CartItems.AnyAsync(item => item.UserId == userId && item.CourseId == courseId))
        {
            _db.CartItems.Add(new CartItem { UserId = userId, CourseId = courseId });
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _db.ChangeTracker.Clear();
                if (!await _db.CartItems.AnyAsync(item => item.UserId == userId && item.CourseId == courseId))
                    throw;
            }
        }

        return null;
    }

    public async Task RemoveAsync(long userId, long courseId)
    {
        await _db.CartItems.Where(item => item.UserId == userId && item.CourseId == courseId).ExecuteDeleteAsync();
    }

    // Chia tiền giảm cho từng khóa. Phần lẻ được phân bổ để tổng tiền luôn khớp.
    public static decimal[] AllocateDiscount(decimal[] prices, decimal discount)
    {
        var total = prices.Sum();
        if (prices.Any(price => price < 0) || discount < 0 || discount > total)
            throw new ArgumentOutOfRangeException(nameof(discount));
        var values = prices.Select(price => total == 0 ? 0 : decimal.Floor(discount * price / total)).ToArray();
        var remainder = discount - values.Sum();
        for (int i = 0; i < values.Length && remainder > 0; i++)
        {
            var extra = Math.Min(prices[i] - values[i], remainder);
            values[i] += extra;
            remainder -= extra;
        }

        return values;
    }

    private async Task<Coupon?> FindCouponAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;
        var normalizedCode = code.Trim().ToUpperInvariant();
        if (_db.Database.CurrentTransaction != null)
        {
            // Khóa coupon khi tạo đơn để hai người không dùng chung lượt cuối.
            return await _db.Coupons.FromSqlInterpolated($"SELECT * FROM coupons WITH (UPDLOCK,HOLDLOCK) WHERE code={normalizedCode}").FirstOrDefaultAsync();
        }

        return await _db.Coupons.FirstOrDefaultAsync(coupon => coupon.Code == normalizedCode);
    }

    // Tính tiền giảm; mã phần trăm có thể kèm giới hạn số tiền tối đa.
    public static decimal Discount(Coupon coupon, decimal subtotal)
    {
        decimal discount;
        if (coupon.DiscountType == "percent")
        {
            discount = subtotal * coupon.DiscountValue / 100;
            if (coupon.MaxDiscount != null)
                discount = Math.Min(discount, coupon.MaxDiscount.Value);
        }
        else
        {
            discount = coupon.DiscountValue;
        }

        return Math.Clamp(decimal.Floor(discount), 0, subtotal);
    }

    private async Task<string?> CheckCouponAsync(Coupon? coupon)
    {
        if (coupon == null || !coupon.IsActive || coupon.StartDate > DateTime.Now || coupon.EndDate < DateTime.Now)
            return "Mã giảm giá không hợp lệ hoặc đã hết hạn.";
        var cutoff = DateTime.Now.AddMinutes(-30);
        var reserved = await _db.Orders.CountAsync(order => order.CouponId == coupon.Id && order.Status == OrderStatus.Pending && (order.CreatedAt > cutoff || order.Payments.Any(payment => payment.Status == "pending")));
        return coupon.TotalLimit != null && coupon.UsedCount + reserved >= coupon.TotalLimit ? "Mã giảm giá đã hết lượt sử dụng." : null;
    }

    // Lấy giỏ hàng và tính tạm giá; khi tạo đơn sẽ kiểm tra lại lần nữa.
    public async Task<CartViewModel> GetCartAsync(long userId, string? code, IEnumerable<long>? selectedIds = null)
    {
        var model = new CartViewModel
        {
            Items = await _db.CartItems.Include(item => item.Course).ThenInclude(coupon => coupon!.Instructor).Where(item => item.UserId == userId).OrderBy(item => item.AddedAt).ToListAsync(),
            CouponCode = code,
            Gateways = _gateways.GetAvailableGateways()
        };
        var allIds = model.Items.Select(item => item.CourseId).ToHashSet();
        var requestedIds = selectedIds?.ToHashSet() ?? allIds;
        model.SelectedIds = requestedIds.Where(id => allIds.Contains(id)).ToList();
        if (requestedIds.Any(id => !allIds.Contains(id)))
            model.Error = "Có khóa đã bị xóa khỏi giỏ. Vui lòng kiểm tra lại lựa chọn.";
        var courseIds = model.SelectedIds;
        var cutoff = DateTime.Now.AddMinutes(-30);
        // Hiện đúng đơn đang giữ các khóa trong giỏ, để học viên tiếp tục hoặc hủy đơn đó.
        model.PendingOrders = await _db.Orders
            .Include(order => order.Payments)
            .Where(order => order.UserId == userId && order.Status == OrderStatus.Pending
                && (order.CreatedAt > cutoff || order.Payments.Any(payment => payment.Status == "pending"))
                && order.Items.Any(item => courseIds.Contains(item.CourseId)))
            .OrderByDescending(order => order.Id)
            .ToListAsync();
        model.Subtotal = model.SelectedItems.Sum(item => item.Course!.CurrentPrice);
        if (!string.IsNullOrWhiteSpace(code))
        {
            var coupon = await FindCouponAsync(code);
            model.Error ??= await CheckCouponAsync(coupon);
            if (model.Error == null)
                model.Discount = Discount(coupon!, model.Subtotal);
        }

        return model;
    }

    // Tạo đơn trong một transaction: kiểm tra giỏ → giữ coupon → chốt giá từng khóa.
    public async Task<(long? Id, string? Error)> CheckoutAsync(long userId, string? code, IEnumerable<long>? selectedIds = null)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        // Khóa dòng người mua trong giao dịch để hai lần bấm không tạo hai đơn cùng lúc.
        await _db.Users.FromSqlInterpolated($"SELECT * FROM users WITH (UPDLOCK, HOLDLOCK) WHERE id={userId}").LoadAsync();
        var cart = await GetCartAsync(userId, code, selectedIds);
        cart.Items = cart.SelectedItems;
        if (cart.Items.Count == 0)
            return (null, "Chọn ít nhất một khóa học để thanh toán.");
        if (cart.Error != null)
            return (null, cart.Error);
        if (cart.Items.Any(item => item.Course!.DeletedAt != null || item.Course.Status != CourseStatus.Published || item.Course.InstructorId == userId || item.Course.Instructor?.IsActive != true || item.Course.Instructor.DeletedAt != null))
            return (null, "Có khóa học không còn mở bán. Hãy xóa khỏi giỏ hàng.");
        var courseIds = cart.Items.Select(item => item.CourseId).ToList();
        if (await _db.Enrollments.AnyAsync(enrollment => enrollment.UserId == userId && courseIds.Contains(enrollment.CourseId)))
            return (null, "Có khóa học bạn đã đăng ký. Hãy xóa khỏi giỏ hàng.");
        var cutoff = DateTime.Now.AddMinutes(-30);
        if (await _db.Orders.AnyAsync(pendingOrder => pendingOrder.UserId == userId && pendingOrder.Status == OrderStatus.Pending && (pendingOrder.CreatedAt > cutoff || pendingOrder.Payments.Any(payment => payment.Status == "pending")) && pendingOrder.Items.Any(i => courseIds.Contains(i.CourseId))))
            return (null, "Bạn có đơn đang chờ thanh toán cho khóa học trong giỏ. Mở Lịch sử đơn hàng để tiếp tục hoặc hủy đơn cũ.");
        var coupon = await FindCouponAsync(code);
        var order = new Order
        {
            UserId = userId,
            CouponId = coupon?.Id,
            Subtotal = cart.Subtotal,
            DiscountAmount = cart.Discount,
            TotalPrice = cart.Total
        };
        var prices = cart.Items.Select(item => item.Course!.CurrentPrice).ToArray();
        if (prices.Any(item => item < 0 || item != decimal.Truncate(item)))
            return (null, "Học phí phải là số nguyên VND. Vui lòng liên hệ giảng viên.");
        var discounts = AllocateDiscount(prices, cart.Discount);
        for (int i = 0; i < cart.Items.Count; i++)
        {
            var course = cart.Items[i].Course!;
            var profile = await _db.InstructorProfiles.FirstOrDefaultAsync(item => item.UserId == course.InstructorId);
            var net = prices[i] - discounts[i];
            order.Items.Add(new OrderItem { CourseId = course.Id, Price = net, InstructorEarning = Math.Round(net * Math.Clamp(profile?.CommissionRate ?? 70, 0, 100) / 100, 2) });
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        if (order.TotalPrice == 0)
            await FulfillOrderAsync(order);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        if(order.Status==OrderStatus.Paid)await _mail.PaidAsync(order.Id,_gateways.DemoEnabled);
        return (order.Id, null);
    }

    // Chỉ gọi bên trong transaction. Một đơn chỉ cấp quyền học và cộng thu nhập một lần.
    private async Task FulfillOrderAsync(Order order, bool creditRealIncome = false)
    {
        if (order.Status == OrderStatus.Paid)
            return;
        foreach (var item in order.Items)
        {
            var existing = await _db.Enrollments.FirstOrDefaultAsync(enrollment => enrollment.UserId == order.UserId && enrollment.CourseId == item.CourseId);
            if (existing != null)
                throw new InvalidOperationException("Khóa học đã có quyền học. Cần đối soát đơn hàng.");
            _db.Enrollments.Add(new Enrollment { UserId = order.UserId, CourseId = item.CourseId });
            var course = await _db.Courses.FirstAsync(course => course.Id == item.CourseId);
            if (creditRealIncome)
                await _db.InstructorProfiles.Where(profile => profile.UserId == course.InstructorId).ExecuteUpdateAsync(update => update.SetProperty(profile => profile.Balance, profile => profile.Balance + item.InstructorEarning));
        }

        if (order.CouponId != null)
            await _db.Coupons.Where(course => course.Id == order.CouponId).ExecuteUpdateAsync(update => update.SetProperty(course => course.UsedCount, course => course.UsedCount + 1));
        var courseIds = order.Items.Select(item => item.CourseId).ToList();
        await _db.CartItems.Where(course => course.UserId == order.UserId && courseIds.Contains(course.CourseId)).ExecuteDeleteAsync();
        order.Status = OrderStatus.Paid;
    }

    // Học viên chỉ xem được đơn hàng của chính mình.
    public async Task<List<Order>> GetOrdersAsync(long userId) => await _db.Orders.Include(order => order.Items).Where(order => order.UserId == userId).OrderByDescending(order => order.Id).ToListAsync();
    // Chi tiết đơn, khóa học và các giao dịch đã gửi tới cổng thanh toán.
    public async Task<OrderDetailsViewModel?> GetOrderAsync(long userId, long id)
    {
        var order = await _db.Orders.Include(order => order.Items).Include(order => order.Payments).AsSplitQuery().FirstOrDefaultAsync(order => order.Id == id && order.UserId == userId);
        if (order == null)
            return null;
        var courseIds = order.Items.Select(item => item.CourseId).ToList();
        return new()
        {
            Order = order,
            DemoEnabled = _gateways.DemoEnabled,
            Courses = await _db.Courses.Where(course => courseIds.Contains(course.Id)).ToDictionaryAsync(course => course.Id),
            Gateways = _gateways.GetAvailableGateways()
        };
    }

    // Chỉ cho hủy đơn chưa gửi thanh toán.
    public async Task<string?> CancelAsync(long userId, long id)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await _db.Users.FromSqlInterpolated($"SELECT * FROM users WITH (UPDLOCK,HOLDLOCK) WHERE id={userId}").LoadAsync();
        var order = await _db.Orders.FromSqlInterpolated($"SELECT * FROM orders WITH (UPDLOCK,HOLDLOCK) WHERE id={id}").FirstOrDefaultAsync(order => order.UserId == userId);
        if (order == null || order.Status != OrderStatus.Pending)
            return "Chỉ hủy được đơn đang chờ thanh toán.";
        // Không hủy giao dịch đang xử lý vì cổng thanh toán có thể đã nhận tiền.
        if (await _db.Payments.AnyAsync(payment => payment.OrderId == id && payment.Status == "pending"))
            return "Đơn đã gửi tới cổng thanh toán, cần chờ kết quả để tránh giao dịch đang xử lý.";
        order.Status = OrderStatus.Cancelled;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return null;
    }

    // Một đơn chỉ tạo một mã giao dịch; bấm lại không tạo thêm lần thu tiền.
    public async Task<(Payment? Payment, string? Error)> CreatePaymentAsync(long userId, long id, string gateway)
    {
        if (!_gateways.GetAvailableGateways().Contains(gateway))
            return (null, "Cổng thanh toán chưa được cấu hình.");
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await _db.Users.FromSqlInterpolated($"SELECT * FROM users WITH (UPDLOCK,HOLDLOCK) WHERE id={userId}").LoadAsync();
        var order = await _db.Orders.FromSqlInterpolated($"SELECT * FROM orders WITH (UPDLOCK,HOLDLOCK) WHERE id={id}").FirstOrDefaultAsync(order => order.UserId == userId);
        if (order == null || order.Status != OrderStatus.Pending || order.CreatedAt.AddMinutes(30) < DateTime.Now)
            return (null, "Đơn hàng không hợp lệ hoặc đã hết hạn thanh toán.");
        if (await _db.Payments.AnyAsync(payment => payment.OrderId == id))
            return (null, "Đơn đã có giao dịch thanh toán. Vui lòng hoàn tất tại cổng thanh toán hoặc chờ đối soát; không tạo giao dịch thứ hai.");
        var reference = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).ToString("yyMMdd") + "_" + Guid.NewGuid().ToString("N")[..24];
        var payment = new Payment
        {
            OrderId = id,
            IsTest = !_gateways.IsLive,
            Gateway = gateway,
            GatewayTxnId = reference,
            Amount = order.TotalPrice
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return (payment, null);
    }

    // Nhận kết quả đã xác thực từ cổng; kiểm tra số tiền trước khi cấp quyền học.
    public async Task<string?> ConfirmPaymentAsync(VerifiedPayment result, bool reportDuplicate = false)
    {
        var snapshot = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(payment => payment.GatewayTxnId == result.Reference);
        if (snapshot == null)
            return "not_found";
        var buyer = await _db.Orders.AsNoTracking().Where(order => order.Id == snapshot.OrderId).Select(order => order.UserId).FirstAsync();
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await _db.Users.FromSqlInterpolated($"SELECT * FROM users WITH (UPDLOCK,HOLDLOCK) WHERE id={buyer}").LoadAsync();
        var order = await _db.Orders.FromSqlInterpolated($"SELECT * FROM orders WITH (UPDLOCK,HOLDLOCK) WHERE id={snapshot.OrderId}").Include(order => order.Items).FirstAsync();
        var payment = await _db.Payments.FromSqlInterpolated($"SELECT * FROM payments WITH (UPDLOCK,HOLDLOCK) WHERE gateway_txn_id={result.Reference}").FirstAsync();
        if (payment.Gateway != result.Gateway || payment.IsTest == _gateways.IsLive)
            return "environment_mismatch";
        if (payment.Amount != result.Amount) return "invalid_amount";
        if (payment.Status == "success" && order.Status == OrderStatus.Paid)
            return reportDuplicate ? "already_confirmed" : null;
        if (payment.Status == "failed" && order.Status == OrderStatus.Failed && !result.Success && result.IsFinal)
            return reportDuplicate ? "already_confirmed" : null;
        if (!result.Success && !result.IsFinal) return "pending";
        if (!result.Success)
        {
            if (result.IsFinal && payment.Status == "pending" && order.Status == OrderStatus.Pending)
            {
                payment.Status = "failed";
                order.Status = OrderStatus.Failed;
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            return null;
        }
        else
        {
            if (order.Status != OrderStatus.Pending || payment.Status != "pending")
                return "state_conflict";
            // Callback hợp lệ có thể đến muộn. Vẫn cấp quyền học khi cổng đã xác nhận thu tiền.
            await FulfillOrderAsync(order, creditRealIncome: !payment.IsTest);
            payment.Status = "success";
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        await _mail.PaidAsync(order.Id,payment.IsTest);
        return null;
    }

    // Lưu lại liên kết để học viên có thể mở lại cùng giao dịch, không tạo giao dịch thứ hai.
    public async Task SavePaymentUrlAsync(long paymentId, string url)
    {
        await _db.Payments.Where(payment => payment.Id == paymentId && payment.Status == "pending").ExecuteUpdateAsync(update => update.SetProperty(payment => payment.PaymentUrl, url));
    }

    // Thanh toán mô phỏng để demo đồ án; không gọi ngân hàng và không thu tiền thật.
    public async Task<string?> SimulatePaymentAsync(long userId, long orderId)
    {
        if (!_gateways.DemoEnabled) return "Thanh toán mô phỏng đang tắt.";

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await _db.Users.FromSqlInterpolated($"SELECT * FROM users WITH (UPDLOCK,HOLDLOCK) WHERE id={userId}").LoadAsync();
        var order = await _db.Orders
            .FromSqlInterpolated($"SELECT * FROM orders WITH (UPDLOCK,HOLDLOCK) WHERE id={orderId}")
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.UserId == userId);
        if (order == null) return "Không tìm thấy đơn hàng của bạn.";

        // Bấm lại sau khi thành công không cấp quyền hay cộng số dư thêm lần nữa.
        if (order.Status == OrderStatus.Paid && await _db.Payments.AnyAsync(payment => payment.OrderId == orderId && payment.Gateway == "demo" && payment.Status == "success"))
            return null;
        if (order.Status != OrderStatus.Pending || order.CreatedAt.AddMinutes(30) <= DateTime.Now)
            return "Đơn không còn chờ thanh toán hoặc đã hết hạn. Hãy tạo đơn mới từ giỏ hàng.";
        if (await _db.Payments.AnyAsync(payment => payment.OrderId == orderId))
            return "Đơn đã gửi tới cổng thanh toán. Không thể dùng mô phỏng cho giao dịch này.";

        var courseIds = order.Items.Select(item => item.CourseId).ToList();
        if (await _db.Enrollments.AnyAsync(enrollment => enrollment.UserId == userId && courseIds.Contains(enrollment.CourseId)))
            return "Bạn đã đăng ký một khóa học trong đơn này.";

        _db.Payments.Add(new Payment
        {
            OrderId = order.Id,
            Gateway = "demo",
            IsTest = true,
            GatewayTxnId = "DEMO-" + Guid.NewGuid().ToString("N"),
            Amount = order.TotalPrice,
            Status = "success"
        });
        await FulfillOrderAsync(order);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        await _mail.PaidAsync(order.Id,true);
        return null;
    }
}

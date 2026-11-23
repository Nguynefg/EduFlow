using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Controllers;
[Authorize]
public class FinanceController : Controller
{
    private readonly EduFlowDbContext _db;
    private readonly FinanceService _finance;
    private readonly IWebHostEnvironment _env;
    public FinanceController(EduFlowDbContext db, FinanceService finance, IWebHostEnvironment env)
    {
        _db = db;
        _finance = finance;
        _env = env;
    }

    [Authorize(Roles = AppRoles.Instructor)]
    public async Task<IActionResult> Index()
    {
        var id = AuthHelper.GetUserId(User);
        var profile = await _db.InstructorProfiles.FirstOrDefaultAsync(profile => profile.UserId == id);
        if (profile == null)
            return NotFound();
        return View(new PayoutViewModel { SimulationEnabled = _finance.SimulationEnabled, WithdrawableBalance = await _finance.GetWithdrawableBalanceAsync(id), Profile = profile, Requests = await _db.PayoutRequests.Where(request => request.InstructorId == id && request.IsTest == _finance.SimulationEnabled).OrderByDescending(request => request.Id).ToListAsync() });
    }

    [Authorize(Roles = AppRoles.Instructor)]
    public async Task<IActionResult> Revenue(DateTime? from, DateTime? to, bool isTest = false)
    {
        var start = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var end = (to ?? DateTime.Today).Date;
        if (!ModelState.IsValid || start > end || end.Year >= 9999)
        {
            TempData["Error"] = "Khoảng ngày không hợp lệ. Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.";
            return RedirectToAction(nameof(Revenue));
        }
        return View(await _finance.GetRevenueAsync(AuthHelper.GetUserId(User), start, end, isTest));
    }

    [HttpPost, Authorize(Roles = AppRoles.Instructor), ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestPayout(decimal amount, bool taxDeclared, bool voluntaryTax, string? taxIdentifier)
    {
        var error = !ModelState.IsValid ? "Dữ liệu không hợp lệ." : await _finance.RequestAsync(AuthHelper.GetUserId(User), amount, taxDeclared, voluntaryTax, taxIdentifier);
        TempData[error == null ? "Success" : "Error"] = error ?? "Đã gửi yêu cầu. Số tiền được giữ lại trong lúc xử lý.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Receipt(long id)
    {
        var request = await _db.PayoutRequests.FirstOrDefaultAsync(request => request.Id == id && (request.InstructorId == AuthHelper.GetUserId(User) || User.IsInRole(AppRoles.Admin)));
        if (request?.IsTest == true && request.Status == "paid")
        {
            var tax = request.TaxAmount.HasValue ? UiText.Money(request.TaxAmount.Value) : "Chưa xác định";
            var net = request.NetAmount.HasValue ? UiText.Money(request.NetAmount.Value) : "Chưa xác định";
            var text = $"EDUFLOW — BIÊN NHẬN CHI TRẢ MÔ PHỎNG\nMã: SIM-PAYOUT-{request.Id}\nNgười nhận: {request.PayeeNameSnapshot}\nNgân hàng mô phỏng: {request.BankNameSnapshot}\nTài khoản: {request.BankNumberSnapshot}\nThù lao trước thuế: {UiText.Money(request.Amount)}\nThuế TNCN: {tax}\nThực nhận: {net}\nNgày chi mô phỏng: {request.PaidAt:dd/MM/yyyy}\nKHÔNG CHUYỂN TIỀN THẬT. Không phải chứng từ ngân hàng hoặc chứng từ thuế.\n";
            return File(System.Text.Encoding.UTF8.GetBytes(text), "text/plain; charset=utf-8", $"SIM-PAYOUT-{request.Id}.txt");
        }
        if (request?.ReceiptUrl == null)
            return NotFound();
        var name = Path.GetFileName(request.ReceiptUrl);
        var path = Path.Combine(_env.ContentRootPath, "App_Data", "uploads", "receipts", name);
        if (!System.IO.File.Exists(path))
            return NotFound();
        return PhysicalFile(path, "application/octet-stream", name);
    }
}


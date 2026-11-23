using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles = AppRoles.Admin)]
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

    public async Task<IActionResult> Index(string? status)
    {
        ViewBag.Status = status;
        ViewBag.SimulationEnabled = _finance.SimulationEnabled;
        return View(await _db.PayoutRequests.Include(request => request.Instructor).ThenInclude(u => u!.InstructorProfile).Where(request => request.IsTest == _finance.SimulationEnabled && (status == null || request.Status == status)).OrderByDescending(request => request.Id).ToListAsync());
    }

    public async Task<IActionResult> Revenue(DateTime? from, DateTime? to, bool isTest = false)
    {
        var start = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var end = (to ?? DateTime.Today).Date;
        if (!ModelState.IsValid || start > end || end.Year >= 9999)
        {
            TempData["Error"] = "Khoảng ngày không hợp lệ. Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.";
            return RedirectToAction(nameof(Revenue));
        }
        return View("~/Views/Finance/Revenue.cshtml", await _finance.GetRevenueAsync(null, start, end, isTest));
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(6000000)]
    public async Task<IActionResult> Process(long id, string action, string? note, IFormFile? receipt, bool taxConfirmed, DateTime? paidOn)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Dữ liệu không hợp lệ."; return RedirectToAction("Index"); }
        string? url = null;
        var root = Path.Combine(_env.ContentRootPath, "App_Data");
        if (action == "paid" && !_finance.SimulationEnabled && receipt != null)
        {
            url = await FileUpload.SaveImageAsync(receipt, root, "receipts");
            if (url == null)
            {
                TempData["Error"] = "Biên lai cần là JPG, PNG, WebP, tối đa 5 MB.";
                return RedirectToAction("Index");
            }
        }

        var error = await _finance.ProcessAsync(id, action, note, url, taxConfirmed, AuthHelper.GetUserId(User), paidOn);
        if (error != null)
            FileUpload.Delete(url, root);
        TempData[error == null ? "Success" : "Error"] = error ?? (_finance.SimulationEnabled ? "Đã cập nhật yêu cầu mô phỏng. Không chuyển tiền thật." : "Đã cập nhật yêu cầu rút tiền.");
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemitTax(long id, DateTime remittedOn, string? reference)
    {
        var error = !ModelState.IsValid ? "Ngày nộp không hợp lệ." : await _finance.MarkTaxRemittedAsync(id, remittedOn, reference, AuthHelper.GetUserId(User));
        TempData[error == null ? "Success" : "Error"] = error ?? (_finance.SimulationEnabled ? "Đã xác nhận nộp thuế mô phỏng. Không nộp tiền thật." : "Đã ghi nhận thông tin nộp thuế theo chứng từ.");
        return RedirectToAction(nameof(TaxReport));
    }

    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> TaxReport(DateTime? from, DateTime? to, bool export = false)
    {
        var start = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var end = (to ?? DateTime.Today).Date;
        if (!ModelState.IsValid || start > end || end.Year >= 9999)
        { TempData["Error"] = "Khoảng ngày không hợp lệ."; return RedirectToAction(nameof(TaxReport)); }
        var until = end.AddDays(1);
        ViewBag.SimulationEnabled = _finance.SimulationEnabled;
        var rows = await _db.PayoutRequests.AsNoTracking().Where(p => p.IsTest == _finance.SimulationEnabled && p.Status == "paid"
            && p.PaidAt >= start && p.PaidAt < until && p.TaxAmount != null).OrderByDescending(p => p.PaidAt).ToListAsync();
        ViewBag.From = start; ViewBag.To = end;
        if (!export) return View(rows);
        // CSV mở trong bảng tính: vô hiệu hóa công thức trong trường văn bản.
        string Cell(string? value)
        {
            var text = value ?? "";
            if (text.TrimStart().StartsWith('=') || text.TrimStart().StartsWith('+') || text.TrimStart().StartsWith('-') || text.TrimStart().StartsWith('@') || text.StartsWith('\t') || text.StartsWith('\r')) text = "'" + text;
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }
        var csv = new System.Text.StringBuilder("Che do,Ma yeu cau,Ho ten,MST/So dinh danh,Ngay chi,Gross,Thue suat,Thue TNCN,Thuc nhan,Ngay nop thue,Ma chung tu\r\n");
        foreach (var row in rows)
            csv.AppendLine(string.Join(",", new[] { row.IsTest ? "MO PHONG" : "TIEN THAT", row.Id.ToString(), Cell(row.PayeeNameSnapshot), Cell("'" + row.TaxIdSnapshot), Cell(row.PaidAt?.ToString("yyyy-MM-dd")), row.Amount.ToString("0", System.Globalization.CultureInfo.InvariantCulture), row.TaxRate?.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) ?? "", row.TaxAmount?.ToString("0", System.Globalization.CultureInfo.InvariantCulture) ?? "", row.NetAmount?.ToString("0", System.Globalization.CultureInfo.InvariantCulture) ?? "", Cell(row.TaxRemittedOn?.ToString("yyyy-MM-dd")), Cell(row.TaxRemittanceRef) }));
        var bytes = new System.Text.UTF8Encoding(true).GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"EduFlow-{(_finance.SimulationEnabled ? "MO-PHONG" : "LIVE")}-thue-{start:yyyyMMdd}-{end:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> Orders() => View(await _db.Orders.Include(o => o.Items).Include(o => o.Payments).AsSplitQuery().OrderByDescending(o => o.Id).Take(200).ToListAsync());
}

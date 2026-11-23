using EduFlow.Web.Models;
using EduFlow.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Areas.Admin.Controllers;

// Admin duyệt / từ chối hồ sơ đăng ký làm giảng viên
[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class KycController : Controller
{
    private readonly EduFlow.Web.Services.MailService _mail;
    private readonly AdminKycService _adminKycService;

    public KycController(AdminKycService adminKycService, EduFlow.Web.Services.MailService mail)
    {
        _adminKycService = adminKycService;
        _mail = mail;
    }

    public async Task<IActionResult> Index(string? status = "pending")
    {
        return View(await _adminKycService.ListAsync(status));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long userId)
    {
        var name = await _adminKycService.ApproveAsync(userId);
        if (name != null)
        {
            TempData["Success"] = "Đã duyệt " + name + " làm giảng viên.";
            await _mail.SendUserAsync(userId,"Kết quả duyệt hồ sơ giảng viên","Hồ sơ của bạn đã được phê duyệt.");
        }
        else
            TempData["Error"] = "Hồ sơ không còn ở trạng thái chờ duyệt.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(long userId, string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "Nhập lý do từ chối.";
            return RedirectToAction("Index");
        }

        var name = await _adminKycService.RejectAsync(userId, reason);
        if (name != null)
        {
            TempData["Success"] = "Đã từ chối hồ sơ của " + name;
            await _mail.SendUserAsync(userId,"Kết quả duyệt hồ sơ giảng viên","Hồ sơ bị từ chối. Lý do: "+reason);
        }
        else
            TempData["Error"] = "Hồ sơ không còn ở trạng thái chờ duyệt.";
        return RedirectToAction("Index");
    }
}

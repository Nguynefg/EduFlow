using EduFlow.Web.Areas.Admin.Services;
using EduFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Areas.Admin.Controllers;

// Admin kiểm duyệt khóa học
[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class CoursesController : Controller
{
    private readonly EduFlow.Web.Services.MailService _mail;
    private readonly AdminCourseService _adminCourseService;

    public CoursesController(AdminCourseService adminCourseService, EduFlow.Web.Services.MailService mail)
    {
        _adminCourseService = adminCourseService;
        _mail = mail;
    }

    public async Task<IActionResult> Index(string? status = CourseStatus.Pending, string? q = null)
    {
        return View(await _adminCourseService.ListAsync(status, q));
    }

    // Xem toàn bộ nội dung khóa học trước khi duyệt
    public async Task<IActionResult> Details(long id)
    {
        var course = await _adminCourseService.GetDetailAsync(id);
        if (course == null) return NotFound();
        return View(course);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long id)
    {
        var error=await _adminCourseService.ApproveAsync(id);ShowResult(error,"Đã duyệt và xuất bản khóa học.");
        if(error==null)await _mail.CourseAsync(id,"Đã duyệt và xuất bản",null);
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(long id, string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "Nhập lý do từ chối.";
            return RedirectToAction("Details", new { id });
        }
        var error=await _adminCourseService.RejectAsync(id,reason);ShowResult(error,"Đã từ chối khóa học.");
        if(error==null)await _mail.CourseAsync(id,"Bị từ chối",reason);
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Hide(long id, string? reason)
    {
        ShowResult(await _adminCourseService.HideAsync(id, reason), "Đã ẩn khóa học.");
        return RedirectToAction("Index", new { status = CourseStatus.Hidden });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unhide(long id)
    {
        ShowResult(await _adminCourseService.UnhideAsync(id), "Đã hiện lại khóa học.");
        return RedirectToAction("Index", new { status = CourseStatus.Published });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        ShowResult(await _adminCourseService.DeleteAsync(id), "Đã xóa khóa học.");
        return RedirectToAction("Index", new { status = "all" });
    }

    private void ShowResult(string? error, string successMessage)
    {
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = successMessage;
    }
}

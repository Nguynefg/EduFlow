using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;

// Studio giảng viên: danh sách khóa học, tạo, sửa, gửi duyệt, xóa
[Authorize(Roles = AppRoles.Instructor + "," + AppRoles.Admin)]
public class InstructorController : Controller
{
    private readonly InstructorCourseService _courseService;
    private readonly IWebHostEnvironment _env;
    private readonly MediaLibraryService _media;

    public InstructorController(InstructorCourseService courseService, IWebHostEnvironment env, MediaLibraryService media)
    {
        _courseService = courseService;
        _env = env;
        _media = media;
    }

    // ===== DANH SÁCH KHÓA HỌC CỦA TÔI =====
    public async Task<IActionResult> Index()
    {
        if (RouteData.Values["area"]?.ToString() == "Admin")
            return RedirectToAction("Index", "Courses", new { area = "Admin", status = "all" });
        var courses = await _courseService.GetMyCoursesAsync(AuthHelper.GetUserId(User));
        return View(courses);
    }

    // ===== TẠO KHÓA HỌC =====
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _courseService.GetCategoriesAsync();
        return View("~/Views/Instructor/Form.cshtml", new CourseFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel model)
    {
        var thumbnail = await SaveThumbnail(model);
        if (!ModelState.IsValid) return await ShowForm(model);

        var (course, error) = await _courseService.CreateAsync(AuthHelper.GetUserId(User), model, thumbnail);
        if (course == null)
        {
            ModelState.AddModelError("", error!);
            return await ShowForm(model);
        }

        TempData["Success"] = "Đã tạo bản nháp. Hãy thêm chương và bài học.";
        return RedirectToAction("Index", "Curriculum", new { id = course.Id });
    }

    // ===== SỬA KHÓA HỌC =====
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var model = await _courseService.GetFormAsync(id, AuthHelper.GetUserId(User));
        if (model == null) return NotFound();
        return await ShowForm(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, CourseFormViewModel model)
    {
        model.Id = id;
        var thumbnail = await SaveThumbnail(model);
        if (!ModelState.IsValid) return await ShowForm(model);

        var error = await _courseService.UpdateAsync(id, AuthHelper.GetUserId(User), model, thumbnail);
        if (error != null)
        {
            ModelState.AddModelError("", error);
            return await ShowForm(model);
        }

        TempData["Success"] = "Đã lưu thông tin khóa học.";
        return RedirectToAction("Index");
    }

    // ===== GỬI DUYỆT / RÚT YÊU CẦU / XÓA =====
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(long id)
    {
        var error = await _courseService.SubmitForReviewAsync(id, AuthHelper.GetUserId(User));
        ShowResult(error, "Đã gửi khóa học cho quản trị viên duyệt.");
        if (RouteData.Values["area"]?.ToString() == "Admin")
            return RedirectToAction("Details", "Courses", new { area = "Admin", id });
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(long id)
    {
        var error = await _courseService.WithdrawAsync(id, AuthHelper.GetUserId(User));
        ShowResult(error, "Đã rút yêu cầu duyệt, khóa học trở về bản nháp.");
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var error = await _courseService.DeleteAsync(id, AuthHelper.GetUserId(User));
        ShowResult(error, "Đã xóa khóa học.");
        return RedirectToAction("Index");
    }

    // Lưu ảnh đại diện nếu có chọn file, báo lỗi nếu file không hợp lệ
    private async Task<string?> SaveThumbnail(CourseFormViewModel model)
    {
        if (model.ThumbnailImage == null)
        {
            if (string.IsNullOrEmpty(model.PickedThumbnailUrl)) return null;
            // Ảnh chọn từ thư viện: phải là ảnh trong thư viện của chính giảng viên này
            if (_media.Owns(AuthHelper.GetUserId(User), model.PickedThumbnailUrl, MediaLibraryService.Image))
                return model.PickedThumbnailUrl;
            ModelState.AddModelError("ThumbnailImage", "Ảnh chọn từ thư viện không hợp lệ.");
            return null;
        }
        var url = await FileUpload.SaveImageAsync(model.ThumbnailImage, _env.WebRootPath, "courses");
        if (url == null)
            ModelState.AddModelError("ThumbnailImage", "Chỉ nhận ảnh JPG, PNG hoặc WebP, tối đa 5 MB.");
        return url;
    }

    private async Task<IActionResult> ShowForm(CourseFormViewModel model)
    {
        ViewBag.Categories = await _courseService.GetCategoriesAsync();
        return View("~/Views/Instructor/Form.cshtml", model);
    }

    private void ShowResult(string? error, string successMessage)
    {
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = successMessage;
    }
}

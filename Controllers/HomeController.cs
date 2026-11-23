using EduFlow.Web.Helpers;
using EduFlow.Web.Models.ViewModels;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;

public class HomeController : Controller
{
    private readonly CourseService _courseService;
    private readonly EmailSender _email;
    private readonly IConfiguration _config;

    public HomeController(CourseService courseService, EmailSender email, IConfiguration config)
    {
        _courseService = courseService;
        _email = email;
        _config = config;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _courseService.GetHomeAsync());
    }

    // Trang Về chúng tôi (dùng lại số liệu của trang chủ)
    public async Task<IActionResult> About()
    {
        return View(await _courseService.GetHomeAsync());
    }

    // Trang giới thiệu cho người muốn làm giảng viên (nút "Dạy học" trên menu)
    public async Task<IActionResult> Teach()
    {
        return View(await _courseService.GetHomeAsync());
    }

    // ===== TRANG LIÊN HỆ =====
    [HttpGet]
    public IActionResult Contact()
    {
        return View(new ContactViewModel());
    }

    // Gửi nội dung liên hệ vào email của trung tâm (cấu hình "Contact:ToEmail" trong appsettings.json)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var to = _config["Contact:ToEmail"];
        var sent = !string.IsNullOrEmpty(to) && await _email.SendAsync(to,
            "[EduFlow - Liên hệ] " + model.Subject,
            $"Người gửi: {model.FullName} <{model.Email}>\nChủ đề: {model.Subject}\n\n{model.Message}");

        if (sent)
            TempData["Success"] = "Cảm ơn bạn! EduFlow đã nhận được tin nhắn và sẽ phản hồi qua email sớm nhất.";
        else
            TempData["Error"] = "Chưa gửi được tin nhắn vì hệ thống chưa cấu hình email (mục Smtp và Contact trong appsettings.json).";
        return RedirectToAction("Contact");
    }

    // Link cũ /Home/Courses chuyển sang trang khóa học mới
    public IActionResult Courses()
    {
        return RedirectToAction("Index", "Courses");
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}

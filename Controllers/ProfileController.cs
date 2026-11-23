using EduFlow.Web.Helpers;
using EduFlow.Web.Models.ViewModels;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ProfileService _profileService;
    private readonly IWebHostEnvironment _env;

    public ProfileController(ProfileService profileService, IWebHostEnvironment env)
    {
        _profileService = profileService;
        _env = env;
    }

    // ===== XEM HỒ SƠ =====
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _profileService.GetProfileAsync(AuthHelper.GetUserId(User));
        return View(model);
    }

    // ===== CẬP NHẬT HỒ SƠ =====
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        var userId = AuthHelper.GetUserId(User);

        // Lưu ảnh đại diện mới (nếu có chọn)
        string? newAvatar = null;
        if (model.AvatarImage != null)
        {
            newAvatar = await FileUpload.SaveImageAsync(model.AvatarImage, _env.WebRootPath, "avatars");
            if (newAvatar == null)
                ModelState.AddModelError("AvatarImage", "Chỉ nhận ảnh JPG, PNG hoặc WebP, tối đa 5 MB.");
        }

        if (!ModelState.IsValid)
        {
            // Lấy lại email, ảnh, vai trò để hiển thị form
            var current = await _profileService.GetProfileAsync(userId);
            model.Email = current.Email;
            model.EmailVerifiedAt = current.EmailVerifiedAt;
            model.AvatarUrl = current.AvatarUrl;
            model.Roles = current.Roles;
            return View(model);
        }

        await _profileService.UpdateProfileAsync(userId, model, newAvatar);
        TempData["Success"] = "Đã cập nhật hồ sơ cá nhân.";
        return RedirectToAction("Index");
    }

    // ===== ĐỔI MẬT KHẨU =====
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var error = await _profileService.ChangePasswordAsync(AuthHelper.GetUserId(User), model.CurrentPassword, model.NewPassword);
        if (error != null)
        {
            ModelState.AddModelError("", error);
            return View(model);
        }

        TempData["Success"] = "Đã đổi mật khẩu.";
        return RedirectToAction("Index");
    }
}

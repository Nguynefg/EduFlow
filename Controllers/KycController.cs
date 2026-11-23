using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;

// Học viên gửi hồ sơ KYC (CCCD, mã số thuế, ngân hàng) để đăng ký làm giảng viên
[Authorize]
public class KycController : Controller
{
    private readonly KycService _kycService;

    public KycController(KycService kycService)
    {
        _kycService = kycService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _kycService.GetPageAsync(AuthHelper.GetUserId(User), User.IsInRole(AppRoles.Instructor));
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index([Bind(Prefix = "Form")] KycFormViewModel form)
    {
        var userId = AuthHelper.GetUserId(User);
        var isInstructor = User.IsInRole(AppRoles.Instructor);

        if (!ModelState.IsValid)
        {
            var model = await _kycService.GetPageAsync(userId, isInstructor);
            model.Form = form;
            return View(model);
        }

        var error = await _kycService.SubmitAsync(userId, isInstructor, form);
        if (error != null)
            TempData["Error"] = error;
        else
            TempData["Success"] = "Đã gửi hồ sơ. Quản trị viên sẽ xét duyệt sớm.";
        return RedirectToAction("Index");
    }
}

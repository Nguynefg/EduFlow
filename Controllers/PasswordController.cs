using EduFlow.Web.Models.ViewModels;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace EduFlow.Web.Controllers;
public class PasswordController:Controller
{
    private readonly MailService _mail;
    public PasswordController(MailService mail){_mail=mail;}
    [HttpGet] public IActionResult Forgot()=>View(new ForgotPasswordViewModel());
    [EnableRateLimiting("auth")]
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Forgot(ForgotPasswordViewModel model)
    {
        if(!ModelState.IsValid)return View(model);
        await _mail.ResetRequestAsync(model.Email);ViewBag.Sent=true;return View(model);
    }
    [HttpGet] public IActionResult Reset(string? token)=>View(new ResetPasswordViewModel{Token=token??""});
    [EnableRateLimiting("auth")]
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset(ResetPasswordViewModel model)
    {
        if(!ModelState.IsValid)return View(model);var error=await _mail.ResetAsync(model.Token,model.Password);
        if(error!=null){ModelState.AddModelError("",error);return View(model);}
        TempData["Success"]="Đã đặt mật khẩu mới. Vui lòng đăng nhập lại.";return RedirectToAction("Login","Account");
    }
}

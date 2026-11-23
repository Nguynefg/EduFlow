using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using EduFlow.Web.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduFlow.Web.Controllers;

public class AccountController : Controller
{
    private readonly MailService _mail;
    private readonly IConfiguration _config;
    private bool SetupEnabled => (_config["Security:SetupKey"]?.Length ?? 0) >= 32;
    private readonly AccountService _accountService;
    private readonly IAuthenticationSchemeProvider _schemes;

    public AccountController(AccountService accountService, IAuthenticationSchemeProvider schemes, MailService mail, IConfiguration config)
    {
        _accountService = accountService;
        _schemes = schemes;
        _mail = mail;
        _config = config;
    }

    // ===== ĐĂNG NHẬP =====
    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl)
    {
        // Chưa có admin nào thì hiện link tạo admin đầu tiên
        ViewBag.NeedSetup = SetupEnabled && !await _accountService.HasAdminAsync();
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [EnableRateLimiting("auth")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (user, error) = await _accountService.LoginAsync(model.Email, model.Password);
        if (user == null)
        {
            ModelState.AddModelError("", error!);
            return View(model);
        }

        await HttpContext.SignInAsync(AuthHelper.CreatePrincipal(user),
            new AuthenticationProperties { IsPersistent = model.RememberMe });

        if (Url.IsLocalUrl(model.ReturnUrl)) return Redirect(model.ReturnUrl);
        if (user.RoleCodes.Contains(AppRoles.Admin)) return Redirect("/Admin");
        return RedirectToAction("Index", "Home");
    }

    // ===== ĐĂNG KÝ (tài khoản mới là học viên) =====
    [HttpGet]
    public IActionResult Register(string? returnUrl)
    {
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [EnableRateLimiting("auth")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (user, error) = await _accountService.RegisterAsync(model, AppRoles.Student);
        if (user == null)
        {
            ModelState.AddModelError("", error!);
            return View(model);
        }

        await HttpContext.SignInAsync(AuthHelper.CreatePrincipal(user));

        await _mail.VerifyRequestAsync(user.Id);

        // Đăng ký từ trang "Dạy học" -> chuyển thẳng sang trang gửi hồ sơ giảng viên
        if (Url.IsLocalUrl(model.ReturnUrl))
        {
            TempData["Success"] = "Tạo tài khoản thành công! Bước tiếp theo: điền hồ sơ giảng viên bên dưới.";
            return Redirect(model.ReturnUrl);
        }
        TempData["Success"] = "Tạo tài khoản thành công. Chào mừng bạn đến với EduFlow!";
        return RedirectToAction("Index", "Profile");
    }

    // ===== TẠO ADMIN ĐẦU TIÊN (chỉ dùng được khi CSDL chưa có admin) =====
    [HttpGet]
    public async Task<IActionResult> Setup()
    {
        if (!SetupEnabled || await _accountService.HasAdminAsync()) return NotFound();
        ViewBag.IsSetup = true;
        return View(new RegisterViewModel());
    }

    [EnableRateLimiting("auth")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup(RegisterViewModel model)
    {
        if (!SetupEnabled || await _accountService.HasAdminAsync()) return NotFound();
        ViewBag.IsSetup = true;
        if (!ModelState.IsValid) return View(model);

        var expected = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(_config["Security:SetupKey"]!));
        var supplied = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(model.SetupKey ?? ""));
        if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(expected, supplied))
        {
            ModelState.Remove(nameof(model.SetupKey));
            model.SetupKey = null;
            ModelState.AddModelError("", "Khóa thiết lập không hợp lệ.");
            return View(model);
        }
        var (user, error) = await _accountService.RegisterAsync(model, AppRoles.Admin);
        if (user == null)
        {
            ModelState.AddModelError("", error!);
            return View(model);
        }

        await HttpContext.SignInAsync(AuthHelper.CreatePrincipal(user));
        TempData["Success"] = "Đã tạo tài khoản quản trị viên đầu tiên.";
        return Redirect("/Admin");
    }

    // ===== ĐĂNG NHẬP BẰNG GOOGLE =====
    // Bước 1: chuyển người dùng sang trang đăng nhập của Google
    [EnableRateLimiting("auth")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GoogleLogin(string? returnUrl)
    {
        if (await _schemes.GetSchemeAsync(GoogleDefaults.AuthenticationScheme) == null)
        {
            TempData["Error"] = "Chưa cấu hình đăng nhập Google (ClientId / ClientSecret). Xem hướng dẫn trong docs/README.md.";
            return RedirectToAction("Login");
        }
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleCallback", new { returnUrl })
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    // Bước 2: Google trả về email, tên, ảnh -> đăng nhập hoặc tạo tài khoản EduFlow
    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string? returnUrl)
    {
        var result = await HttpContext.AuthenticateAsync(AuthHelper.ExternalScheme);
        await HttpContext.SignOutAsync(AuthHelper.ExternalScheme);   // xóa cookie tạm

        var email = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;
        if (!result.Succeeded || string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Đăng nhập Google không thành công. Vui lòng thử lại.";
            return RedirectToAction("Login");
        }

        var name = result.Principal!.FindFirst(ClaimTypes.Name)?.Value ?? "";
        var picture = result.Principal.FindFirst("picture")?.Value;
        var (user, error) = await _accountService.LoginWithGoogleAsync(email, name, picture);
        if (user == null)
        {
            TempData["Error"] = error;
            return RedirectToAction("Login");
        }

        await HttpContext.SignInAsync(AuthHelper.CreatePrincipal(user));
        if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        if (user.RoleCodes.Contains(AppRoles.Admin)) return Redirect("/Admin");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> VerifyEmail(string? token)
    {
        var confirmed=await _mail.ConfirmAsync(token);
        TempData[confirmed?"Success":"Error"]=confirmed?"Xác thực email thành công.":"Liên kết không hợp lệ, đã dùng hoặc hết hạn. Bạn có thể gửi lại trong Hồ sơ.";
        return RedirectToAction("Index","Profile");
    }
    [EnableRateLimiting("auth")]
    [HttpPost,Authorize,ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendVerification()
    {
        var sent=await _mail.VerifyRequestAsync(AuthHelper.GetUserId(User));TempData[sent?"Success":"Error"]=sent?"Đã tạo email xác thực.":"Email đã xác thực hoặc chưa thể gửi thư. Kiểm tra cấu hình email.";return RedirectToAction("Index","Profile");
    }

    // ===== ĐĂNG XUẤT =====
    [EnableRateLimiting("auth")]
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}

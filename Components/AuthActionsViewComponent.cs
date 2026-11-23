using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Components;

// Nút đăng nhập / menu tài khoản trên thanh đầu trang
public class AuthActionsViewComponent : ViewComponent
{
    private readonly AccountService _accountService;

    public AuthActionsViewComponent(AccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
            return View((User?)null);

        var user = await _accountService.GetUserAsync(AuthHelper.GetUserId(UserClaimsPrincipal));
        return View(user);
    }
}

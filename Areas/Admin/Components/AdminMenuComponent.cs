using EduFlow.Web.Areas.Admin.Models;
using EduFlow.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Areas.Admin.Components;

// Menu bên trái của khu quản trị
[ViewComponent(Name = "AdminMenu")]
public class AdminMenuComponent : ViewComponent
{
    private readonly AdminKycService _adminKycService;
    private readonly AdminCourseService _adminCourseService;

    public AdminMenuComponent(AdminKycService adminKycService, AdminCourseService adminCourseService)
    {
        _adminKycService = adminKycService;
        _adminCourseService = adminCourseService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = new List<AdminMenuItem>
        {
            new AdminMenuItem("Home", "layout-dashboard", "Tổng quan"),
            new AdminMenuItem("Users", "users", "Quản lý tài khoản"),
            new AdminMenuItem("Kyc", "user-check", "Duyệt giảng viên", await _adminKycService.CountPendingAsync()),
            new AdminMenuItem("Courses", "book-open", "Quản lý khóa học", await _adminCourseService.CountPendingAsync()),
            new AdminMenuItem("Categories", "folder-tree", "Danh mục"),
            new AdminMenuItem("Coupons", "ticket", "Mã giảm giá"),
            new AdminMenuItem("Finance", "wallet", "Duyệt rút tiền")
        };
        var controller = ViewContext.RouteData.Values["controller"]?.ToString();
        if (controller is "Instructor" or "Curriculum") controller = "Courses";
        var deleted = HttpContext.Request.Query["deleted"] == "true";
        return View(new AdminMenuViewModel(controller, deleted, items));
    }
}

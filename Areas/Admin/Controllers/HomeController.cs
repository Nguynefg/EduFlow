using EduFlow.Web.Models;
using EduFlow.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class HomeController : Controller
{
    private readonly EduFlow.Web.Services.ReportService _reports;
    private readonly AdminUserService _adminUserService;

    public HomeController(AdminUserService adminUserService, EduFlow.Web.Services.ReportService reports)
    {
        _adminUserService = adminUserService;
        _reports = reports;
    }

    // Trang tổng quan
    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var start=(from??new DateTime(DateTime.Today.Year,DateTime.Today.Month,1)).Date;var end=(to??DateTime.Today).Date;
        if(!ModelState.IsValid || !EduFlow.Web.Services.ReportService.ValidPeriod(start,end)){TempData["Error"]="Chọn khoảng ngày hợp lệ, tối đa 366 ngày.";return RedirectToAction("Index");}
        var model=await _adminUserService.GetDashboardAsync();model.Report=await _reports.AdminAsync(start,end);return View(model);
    }
}

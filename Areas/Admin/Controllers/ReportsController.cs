using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EduFlow.Web.Areas.Admin.Controllers;
[Area("Admin"),Authorize(Roles=AppRoles.Admin),ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class ReportsController:Controller
{
    private readonly ReportService _reports;private readonly EduFlowDbContext _db;
    public ReportsController(ReportService reports,EduFlowDbContext db){_reports=reports;_db=db;}
    private async Task<bool> Allowed(){var id=AuthHelper.GetUserId(User);return await _db.Users.AnyAsync(u=>u.Id==id && u.IsActive && u.DeletedAt==null && u.UserRoles.Any(r=>r.Role!.Code==AppRoles.Admin));}
    public async Task<IActionResult> Revenue(DateTime? from,DateTime? to)
    {
        if(!await Allowed())return Forbid();var start=(from??new DateTime(DateTime.Today.Year,DateTime.Today.Month,1)).Date;var end=(to??DateTime.Today).Date;
        if(!ModelState.IsValid || !ReportService.ValidPeriod(start,end))return BadRequest("Khoảng ngày không hợp lệ, tối đa 366 ngày.");
        var report=await _reports.AdminAsync(start,end);
        var rows=report.Days.Select(d=>new object?[]{report.Revenue.IsTest?"MÔ PHỎNG":"TIỀN THẬT",d.Date,d.Revenue,d.Orders,d.Sales,d.NewUsers});
        return File(ExcelExport.Create(new[]{"Chế độ","Ngày tạo đơn","Tiền bán (VND)","Đơn thanh toán","Lượt mua khóa học","Tài khoản mới"},rows),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"EduFlow-doanh-thu-{start:yyyyMMdd}-{end:yyyyMMdd}.xlsx");
    }
    public async Task<IActionResult> Users(DateTime? from,DateTime? to)
    {
        if(!await Allowed())return Forbid();var start=(from??new DateTime(DateTime.Today.Year,DateTime.Today.Month,1)).Date;var end=(to??DateTime.Today).Date;
        if(!ModelState.IsValid || !ReportService.ValidPeriod(start,end))return BadRequest("Khoảng ngày không hợp lệ, tối đa 366 ngày.");
        var until=end.AddDays(1);var users=await _db.Users.AsNoTracking().Include(u=>u.UserRoles).ThenInclude(r=>r.Role).Where(u=>u.CreatedAt>=start && u.CreatedAt<until).OrderBy(u=>u.Id).ToListAsync();
        return File(ExcelExport.Create(new[]{"Mã","Họ tên","Email","Vai trò","Trạng thái","Ngày đăng ký"},users.Select(u=>new object?[]{u.Id,u.FullName,u.Email,string.Join(", ",u.RoleCodes),u.DeletedAt!=null?"Đã xóa":u.IsActive?"Hoạt động":"Bị khóa",u.CreatedAt})),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"EduFlow-nguoi-dung-{start:yyyyMMdd}-{end:yyyyMMdd}.xlsx");
    }
}

using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EduFlow.Web.Controllers;
[Authorize,ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class DashboardController:Controller
{
    private readonly ReportService _reports;private readonly LearningService _learning;
    public DashboardController(ReportService reports,LearningService learning){_reports=reports;_learning=learning;}
    public async Task<IActionResult> Student()=>View(await _learning.GetMyCoursesAsync(AuthHelper.GetUserId(User),"all",null));
    [Authorize(Roles=AppRoles.Instructor)]
    public async Task<IActionResult> Instructor(DateTime? from,DateTime? to,bool export=false)
    {
        var start=(from??new DateTime(DateTime.Today.Year,DateTime.Today.Month,1)).Date;var end=(to??DateTime.Today).Date;
        if(!ModelState.IsValid || !ReportService.ValidPeriod(start,end)){TempData["Error"]="Chọn khoảng ngày hợp lệ, tối đa 366 ngày.";return RedirectToAction(nameof(Instructor));}
        var model=await _reports.InstructorAsync(AuthHelper.GetUserId(User),start,end);
        if(!export)return View(model);
        return File(ExcelExport.Create(new[]{"Chế độ","Khóa học","Học viên ghi danh","Đã hoàn thành","Tiến độ TB (%)","Doanh thu trong kỳ (VND)","Giảng viên hưởng (VND)"},model.Courses.Select(c=>{var money=model.Revenue.Courses.FirstOrDefault(r=>r.CourseId==c.CourseId);return new object?[]{model.Revenue.IsTest?"MÔ PHỎNG":"TIỀN THẬT",c.Title,c.Students,c.Completed,c.AverageProgress,money?.Revenue??0,money?.InstructorIncome??0};})),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"EduFlow-giang-vien-{start:yyyyMMdd}-{end:yyyyMMdd}.xlsx");
    }
}

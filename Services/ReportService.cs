using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace EduFlow.Web.Services;
public class ReportService
{
    private readonly EduFlowDbContext _db;
    private readonly FinanceService _finance;
    public ReportService(EduFlowDbContext db,FinanceService finance){_db=db;_finance=finance;}
    public static bool ValidPeriod(DateTime from,DateTime to) => from<=to && to.Year<9999 && (to-from).TotalDays<366;
    public async Task<AdminReportViewModel> AdminAsync(DateTime from,DateTime to)
    {
        if(!ValidPeriod(from,to)) throw new ArgumentException("Chọn khoảng thời gian tối đa 366 ngày.");
        var end=to.AddDays(1);var test=_finance.SimulationEnabled;
        var orders=await _db.Orders.AsNoTracking().Where(o=>o.Status==OrderStatus.Paid && o.CreatedAt>=from && o.CreatedAt<end
            && _db.Payments.Any(p=>p.OrderId==o.Id && p.Status=="success" && p.IsTest==test && (test || p.Gateway!="demo")))
            .Select(o=>new {o.CreatedAt,o.TotalPrice,Sales=o.Items.Count}).ToListAsync();
        var users=await _db.Users.AsNoTracking().Where(u=>u.CreatedAt>=from && u.CreatedAt<end).Select(u=>u.CreatedAt).ToListAsync();
        var byDay=orders.GroupBy(o=>o.CreatedAt.Date).ToDictionary(g=>g.Key);
        var newUsers=users.GroupBy(d=>d.Date).ToDictionary(g=>g.Key,g=>g.Count());
        var model=new AdminReportViewModel{Revenue=await _finance.GetRevenueAsync(null,from,to,test)};
        for(var day=from;day<=to;day=day.AddDays(1))
        {
            byDay.TryGetValue(day,out var items);
            model.Days.Add(new DailyReportRow{Date=day,Revenue=items?.Sum(o=>o.TotalPrice)??0,Orders=items?.Count()??0,Sales=items?.Sum(o=>o.Sales)??0,NewUsers=newUsers.GetValueOrDefault(day)});
        }
        return model;
    }
    public async Task<InstructorReportViewModel> InstructorAsync(long id,DateTime from,DateTime to)
    {
        if(!ValidPeriod(from,to)) throw new ArgumentException("Chọn khoảng thời gian tối đa 366 ngày.");
        var courses=await _db.Courses.AsNoTracking().Include(c=>c.Sections).ThenInclude(s=>s.Lessons).Where(c=>c.InstructorId==id && c.DeletedAt==null).ToListAsync();
        var ids=courses.Select(c=>c.Id).ToList();
        var enrollments=await _db.Enrollments.AsNoTracking().Include(e=>e.Progress).Where(e=>ids.Contains(e.CourseId) && e.Status==EnrollmentStatus.Active && e.User!.DeletedAt==null).ToListAsync();
        var model=new InstructorReportViewModel{Revenue=await _finance.GetRevenueAsync(id,from,to,_finance.SimulationEnabled),Students=enrollments.Select(e=>e.UserId).Distinct().Count()};
        var all=new List<decimal>();
        foreach(var course in courses)
        {
            var lessons=course.Sections.SelectMany(s=>s.Lessons).Select(l=>l.Id).ToHashSet();
            var students=enrollments.Where(e=>e.CourseId==course.Id).ToList();
            var progress=students.Select(e=>lessons.Count==0?0m:e.Progress.Count(p=>p.IsCompleted && lessons.Contains(p.LessonId))*100m/lessons.Count).ToList();
            all.AddRange(progress);
            model.Courses.Add(new CourseLearningRow{CourseId=course.Id,Title=course.Title,Students=students.Count,Completed=progress.Count(p=>p==100),AverageProgress=progress.Count==0?0:Math.Round(progress.Average(),2)});
        }
        model.AverageProgress=all.Count==0?0:Math.Round(all.Average(),2);return model;
    }
}

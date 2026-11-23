namespace EduFlow.Web.Models.ViewModels;
public class DailyReportRow
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
    public int Sales { get; set; }
    public int NewUsers { get; set; }
}
public class AdminReportViewModel
{
    public RevenueViewModel Revenue { get; set; } = new();
    public List<DailyReportRow> Days { get; set; } = new();
    public int NewUsers => Days.Sum(d=>d.NewUsers);
    public int Orders => Days.Sum(d=>d.Orders);
}
public class CourseLearningRow
{
    public long CourseId { get; set; }
    public string Title { get; set; } = "";
    public int Students { get; set; }
    public int Completed { get; set; }
    public decimal AverageProgress { get; set; }
}
public class InstructorReportViewModel
{
    public RevenueViewModel Revenue { get; set; } = new();
    public List<CourseLearningRow> Courses { get; set; } = new();
    public int Students { get; set; }
    public int Enrollments => Courses.Sum(c=>c.Students);
    public decimal AverageProgress { get; set; }
}

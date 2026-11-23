namespace EduFlow.Web.Models.ViewModels;

public class RevenueViewModel
{
    public bool SimulationEnabled { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public bool IsTest { get; set; }
    public bool IsAdmin { get; set; }
    public decimal? CurrentInstructorRate { get; set; }
    public decimal WithdrawableBalance { get; set; }
    public decimal HeldAmount { get; set; }
    public decimal WithheldTax { get; set; }
    public decimal NetPaid { get; set; }
    public decimal RemittedTax { get; set; }
    public decimal PaidAmount { get; set; }
    public List<CourseRevenueRow> Courses { get; set; } = new();
    public int Sales => Courses.Sum(c => c.Sales);
    public decimal Revenue => Courses.Sum(c => c.Revenue);
    public decimal InstructorIncome => Courses.Sum(c => c.InstructorIncome);
    public decimal PlatformIncome => Revenue - InstructorIncome;
    public decimal PlatformPercent => Revenue == 0 ? 0 : PlatformIncome / Revenue * 100;
}

public class CourseRevenueRow
{
    public long CourseId { get; set; }
    public string Title { get; set; } = "";
    public string Instructor { get; set; } = "";
    public int Sales { get; set; }
    public decimal Revenue { get; set; }
    public decimal InstructorIncome { get; set; }
    public decimal PlatformIncome => Revenue - InstructorIncome;
}

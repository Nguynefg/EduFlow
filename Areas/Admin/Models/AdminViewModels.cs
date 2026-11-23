using System.ComponentModel.DataAnnotations;
using EduFlow.Web.Models;

namespace EduFlow.Web.Areas.Admin.Models;

public class AdminDashboardViewModel
{
    public EduFlow.Web.Models.ViewModels.AdminReportViewModel Report { get; set; } = new();
    public int TotalUsers { get; set; }
    public int Students { get; set; }
    public int Instructors { get; set; }
    public int Admins { get; set; }
    public int Locked { get; set; }
    public int Deleted { get; set; }
    public int NewThisWeek { get; set; }
    public int PendingKyc { get; set; }
    public List<User> RecentUsers { get; set; } = [];
}

public class AdminUsersViewModel
{
    public List<User> Users { get; set; } = [];
    public string? Query { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
    public bool Deleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int Total { get; set; }
    public int TotalPages => Math.Max(1, (Total + PageSize - 1) / PageSize);
    public long CurrentUserId { get; set; }
}

public class AdminUserFormViewModel
{
    public long? Id { get; set; }

    [Required(ErrorMessage = "Nhập họ và tên."), StringLength(150), Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(255), Display(Name = "Email đăng nhập")]
    public string Email { get; set; } = "";

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ."), StringLength(20), Display(Name = "Số điện thoại")]
    public string? PhoneNumber { get; set; }

    [StringLength(2000), Display(Name = "Tiểu sử")]
    public string? Bio { get; set; }

    [Display(Name = "Vai trò")]
    public List<string> Roles { get; set; } = new List<string>();

    [StringLength(100), DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string? Password { get; set; }

    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp."), DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")]
    public string? ConfirmPassword { get; set; }
}

public class AdminCoursesViewModel
{
    public List<Course> Courses { get; set; } = new();
    public string? Status { get; set; }
    public string? Query { get; set; }
    public int PendingCount { get; set; }
}

public class AdminCategoriesViewModel
{
    public List<Category> Categories { get; set; } = new();
    public Dictionary<int, int> CourseCounts { get; set; } = new();
}

public class AdminKycViewModel
{
    public List<InstructorProfile> Profiles { get; set; } = [];
    public string? Status { get; set; }
    public int PendingCount { get; set; }
}

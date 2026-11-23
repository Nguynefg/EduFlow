using System.ComponentModel.DataAnnotations;

namespace EduFlow.Web.Models.ViewModels;

/// <summary>Form học viên gửi hồ sơ KYC để làm giảng viên (bảng instructor_profiles).</summary>
public class KycFormViewModel
{
    [Required(ErrorMessage = "Nhập chức danh."), StringLength(255), Display(Name = "Chức danh / chuyên môn")]
    public string Headline { get; set; } = "";

    [Required(ErrorMessage = "Nhập số CCCD."), RegularExpression(@"^\d{12}$", ErrorMessage = "Số CCCD gồm đúng 12 chữ số."), Display(Name = "Số CCCD")]
    public string IdentityCard { get; set; } = "";

    [RegularExpression(@"^(\d{10}(-\d{3})?|\d{12})$", ErrorMessage = "Nhập MST 10 số, 10 số kèm -3 số, hoặc số định danh cá nhân 12 số."), Display(Name = "Mã số thuế (không bắt buộc)")]
    public string? TaxId { get; set; }

    [Required(ErrorMessage = "Nhập tên ngân hàng."), StringLength(100), Display(Name = "Ngân hàng")]
    public string BankName { get; set; } = "";

    [Required(ErrorMessage = "Nhập số tài khoản."), RegularExpression(@"^\d{6,20}$", ErrorMessage = "Số tài khoản gồm 6–20 chữ số."), Display(Name = "Số tài khoản")]
    public string BankAccountNumber { get; set; } = "";

    [Required(ErrorMessage = "Nhập tên chủ tài khoản."), StringLength(100), Display(Name = "Tên chủ tài khoản (viết hoa không dấu)")]
    public string BankAccountName { get; set; } = "";
}

public class KycPageViewModel
{
    public InstructorProfile? Profile { get; set; }
    public bool IsInstructor { get; set; }
    public KycFormViewModel Form { get; set; } = new();
    /// <summary>Được gửi khi chưa có hồ sơ hoặc hồ sơ bị từ chối.</summary>
    public bool CanSubmit => !IsInstructor && Profile?.KycStatus is null or KycStatus.Rejected;
}

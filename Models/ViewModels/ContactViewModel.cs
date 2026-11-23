using System.ComponentModel.DataAnnotations;

namespace EduFlow.Web.Models.ViewModels;

// Form trang Liên hệ
public class ContactViewModel
{
    [Required(ErrorMessage = "Nhập họ và tên."), StringLength(150), Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ."), Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Chọn chủ đề."), Display(Name = "Chủ đề")]
    public string Subject { get; set; } = "Hỗ trợ học viên";

    [Required(ErrorMessage = "Nhập nội dung."), StringLength(3000, MinimumLength = 10, ErrorMessage = "Nội dung từ 10 đến 3000 ký tự."), Display(Name = "Nội dung")]
    public string Message { get; set; } = "";
}

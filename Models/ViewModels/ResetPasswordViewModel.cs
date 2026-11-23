using System.ComponentModel.DataAnnotations;
namespace EduFlow.Web.Models.ViewModels;
public class ResetPasswordViewModel
{
    [Required] public string Token { get; set; } = "";
    [Required(ErrorMessage="Nhập mật khẩu mới.")] public string Password { get; set; } = "";
    [Compare(nameof(Password),ErrorMessage="Mật khẩu xác nhận không khớp.")] public string ConfirmPassword { get; set; } = "";
}

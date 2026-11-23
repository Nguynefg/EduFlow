using System.ComponentModel.DataAnnotations;

namespace EduFlow.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ."), Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu."), DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Nhập họ và tên."), StringLength(150), Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(255), Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu."), StringLength(100), DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Nhập lại mật khẩu."), Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp."), DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")]
    public string ConfirmPassword { get; set; } = "";

    [StringLength(512), DataType(DataType.Password)]
    public string? SetupKey { get; set; }

    // Trang chuyển tới sau khi đăng ký (ví dụ /Kyc khi đăng ký từ trang Dạy học)
    public string? ReturnUrl { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ."), Display(Name = "Email tài khoản")]
    public string Email { get; set; } = "";
}

public class ChangePasswordViewModel
{
    // Không bắt buộc: tài khoản tạo bằng Google chưa có mật khẩu, được đặt mật khẩu lần đầu
    [DataType(DataType.Password), Display(Name = "Mật khẩu hiện tại")]
    public string? CurrentPassword { get; set; }

    [Required(ErrorMessage = "Nhập mật khẩu mới."), StringLength(100), DataType(DataType.Password), Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Nhập lại mật khẩu mới."), Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp."), DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu mới")]
    public string ConfirmPassword { get; set; } = "";
}

public class ProfileViewModel
{
    public DateTime? EmailVerifiedAt { get; set; }
    public string Email { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public List<string> Roles { get; set; } = [];

    [Required(ErrorMessage = "Nhập họ và tên."), StringLength(150), Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ."), StringLength(20), Display(Name = "Số điện thoại")]
    public string? PhoneNumber { get; set; }

    [StringLength(2000), Display(Name = "Tiểu sử / giới thiệu bản thân")]
    public string? Bio { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public IFormFile? AvatarImage { get; set; }

    [Url(ErrorMessage = "Link không hợp lệ."), StringLength(300), Display(Name = "Facebook")]
    public string? Facebook { get; set; }

    [Url(ErrorMessage = "Link không hợp lệ."), StringLength(300), Display(Name = "GitHub")]
    public string? Github { get; set; }

    [Url(ErrorMessage = "Link không hợp lệ."), StringLength(300), Display(Name = "LinkedIn")]
    public string? Linkedin { get; set; }
}

/// <summary>Các link mạng xã hội, lưu dạng JSON trong cột users.social_links.</summary>
public class SocialLinks
{
    public string? Facebook { get; set; }
    public string? Github { get; set; }
    public string? Linkedin { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduFlow.Web.Models;

[Table("users")]
public class User
{
    [Key, Column("id")] public long Id { get; set; }
    [Column("email")] public string Email { get; set; } = "";
    [Column("password_hash")] public string? PasswordHash { get; set; }
    [Column("full_name")] public string FullName { get; set; } = "";
    [Column("avatar_url")] public string? AvatarUrl { get; set; }
    [Column("phone_number")] public string? PhoneNumber { get; set; }
    [Column("bio")] public string? Bio { get; set; }
    [Column("social_links")] public string? SocialLinks { get; set; }
    [Column("is_active")] public bool IsActive { get; set; } = true;
    [Column("email_verified_at")] public DateTime? EmailVerifiedAt { get; set; }
    [Column("last_login_at")] public DateTime? LastLoginAt { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
    [Column("deleted_at")] public DateTime? DeletedAt { get; set; }

    public List<UserRole> UserRoles { get; set; } = new();
    public InstructorProfile? InstructorProfile { get; set; }

    // Danh sách mã vai trò (admin, instructor, student) - không lưu vào CSDL
    [NotMapped]
    public List<string> RoleCodes => UserRoles.Where(x => x.Role != null).Select(x => x.Role!.Code).ToList();
}

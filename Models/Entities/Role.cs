using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduFlow.Web.Models;

[Table("roles")]
public class Role
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("code")] public string Code { get; set; } = "";
    [Column("display_name")] public string DisplayName { get; set; } = "";
}

[Table("user_roles")]
public class UserRole
{
    [Column("user_id")] public long UserId { get; set; }
    [Column("role_id")] public int RoleId { get; set; }
    [Column("assigned_at")] public DateTime AssignedAt { get; set; } = DateTime.Now;

    public User? User { get; set; }
    public Role? Role { get; set; }
}

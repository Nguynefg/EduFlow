using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduFlow.Web.Models;

[Table("categories")]
public class Category
{
    [Key, Column("id")]
    public int Id { get; set; }
    [Column("parent_id")] public int? ParentId { get; set; }
    [Column("name")] public string Name { get; set; } = "";
    [Column("slug")] public string Slug { get; set; } = "";
    [Column("is_active")] public bool IsActive { get; set; } = true;

    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = new();
}

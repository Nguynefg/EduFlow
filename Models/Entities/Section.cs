using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduFlow.Web.Models;

[Table("sections")]
public class Section
{
    [Key, Column("id")] public long Id { get; set; }
    [Column("course_id")] public long CourseId { get; set; }
    [Column("title")] public string Title { get; set; } = "";
    [Column("order_index")] public int OrderIndex { get; set; }

    public Course? Course { get; set; }
    public List<Lesson> Lessons { get; set; } = new();
}

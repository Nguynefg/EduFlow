using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EduFlow.Web.Models;

[Table("course_reviews")]
public class CourseReview
{
    [Column("course_id")] public long CourseId { get; set; }
    [Column("user_id")] public long UserId { get; set; }
    [Column("rating")] public byte Rating { get; set; }
    [Column("comment")] public string? Comment { get; set; }
    [Column("instructor_reply")] public string? InstructorReply { get; set; }
    [Column("is_hidden")] public bool IsHidden { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
    public User? User { get; set; }
    public Course? Course { get; set; }
}
[Table("discussions")]
public class Discussion
{
    [Key, Column("id")] public long Id { get; set; }
    [Column("lesson_id")] public long LessonId { get; set; }
    [Column("user_id")] public long UserId { get; set; }
    [Column("parent_id")] public long? ParentId { get; set; }
    [Column("title"), StringLength(255)] public string? Title { get; set; }
    [Column("content")] public string Content { get; set; } = "";
    [Column("video_timestamp_sec")] public int? VideoTimestampSec { get; set; }
    [Column("upvote_count")] public int UpvoteCount { get; set; }
    [Column("is_pinned")] public bool IsPinned { get; set; }
    [Column("is_resolved")] public bool IsResolved { get; set; }
    [Column("is_hidden")] public bool IsHidden { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
    public User? User { get; set; }
    public Lesson? Lesson { get; set; }
}
[Table("discussion_votes")]
public class DiscussionVote
{
    [Column("discussion_id")] public long DiscussionId { get; set; }
    [Column("user_id")] public long UserId { get; set; }
}

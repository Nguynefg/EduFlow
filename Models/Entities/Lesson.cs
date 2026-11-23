using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduFlow.Web.Models;

// Bài học: lesson_type = video / article / quiz
[Table("lessons")]
public class Lesson
{
    [Key, Column("id")] public long Id { get; set; }
    [Column("section_id")] public long SectionId { get; set; }
    [Column("title")] public string Title { get; set; } = "";
    [Column("lesson_type")] public string LessonType { get; set; } = "video";
    [Column("video_url")] public string? VideoUrl { get; set; }
    [Column("duration_sec")] public int DurationSec { get; set; }
    [Column("content")] public string? Content { get; set; }
    [Column("pass_score")] public int? PassScore { get; set; }
    [Column("is_free_preview")] public bool IsFreePreview { get; set; }
    [Column("order_index")] public int OrderIndex { get; set; }

    public Section? Section { get; set; }
    public List<LessonResource> Resources { get; set; } = new();
    public List<QuizQuestion> Questions { get; set; } = new();
}

[Table("lesson_resources")]
public class LessonResource
{
    [Key, Column("id")] public long Id { get; set; }
    [Column("lesson_id")] public long LessonId { get; set; }
    [Column("file_name")] public string FileName { get; set; } = "";
    [Column("file_url")] public string FileUrl { get; set; } = "";
    [Column("file_size")] public long FileSize { get; set; }

    public Lesson? Lesson { get; set; }
}

[Table("quiz_questions")]
public class QuizQuestion
{
    [Key, Column("id")] public long Id { get; set; }
    [Column("lesson_id")] public long LessonId { get; set; }
    [Column("question_text")] public string QuestionText { get; set; } = "";
    [Column("question_type")] public string QuestionType { get; set; } = "single";   // single = trắc nghiệm, fill = điền khuyết
    [Column("order_index")] public int OrderIndex { get; set; }

    public Lesson? Lesson { get; set; }
    public List<QuizOption> Options { get; set; } = new();
}

[Table("quiz_options")]
public class QuizOption
{
    [Key, Column("id")] public long Id { get; set; }
    [Column("question_id")] public long QuestionId { get; set; }
    [Column("option_text")] public string OptionText { get; set; } = "";
    [Column("is_correct")] public bool IsCorrect { get; set; }

    public QuizQuestion? Question { get; set; }
}

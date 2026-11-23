namespace EduFlow.Web.Models.ViewModels;

// ===== TRANG CHI TIẾT KHÓA HỌC =====
public class CourseDetailViewModel
{
    public ReviewsViewModel Reviews { get; set; } = new();
    public Course Course { get; set; } = null!;
    public Enrollment? Enrollment { get; set; }          // null = chưa mua
    public bool IsOwner { get; set; }                    // giảng viên của khóa học
    public int StudentCount { get; set; }
    public HashSet<long> CompletedLessonIds { get; set; } = new();
    public HashSet<long> LockedLessonIds { get; set; } = new();
    public long? ContinueLessonId { get; set; }

    public bool IsEnrolled => Enrollment != null;
}

// ===== KHÓA HỌC ĐÃ MUA =====
public class MyCoursesViewModel
{
    public List<MyCourseItem> Items { get; set; } = new();
    public string Filter { get; set; } = "all";          // all / learning / completed / not-started
    public string? Query { get; set; }
    public int TotalCount { get; set; }
    public int LearningCount { get; set; }
    public int CompletedCount { get; set; }
    public int NotStartedCount { get; set; }
    public long StudySeconds { get; set; }
    public int TotalCompletedLessons { get; set; }
}

public class MyCourseItem
{
    public int ResumePositionSec { get; set; }
    public int WatchedPercent { get; set; }
    public long StudySeconds { get; set; }
    public Enrollment Enrollment { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public int LessonCount { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalSeconds { get; set; }
    public long? ContinueLessonId { get; set; }
    public string? ContinueLessonTitle { get; set; }
    public DateTime LastActivity { get; set; }

    public int Percent => LessonCount == 0 ? 0 : (int)Math.Round(CompletedLessons * 100.0 / LessonCount, MidpointRounding.AwayFromZero);
    public bool IsCompleted => LessonCount > 0 && CompletedLessons >= LessonCount;
    public bool IsStarted => CompletedLessons > 0 || LastActivity > Enrollment.EnrolledAt;
}

// ===== TRANG HỌC =====
public class LearnViewModel
{
    public int? StartAt { get; set; }
    public DiscussionViewModel? Discussions { get; set; }
    public Course Course { get; set; } = null!;
    public Enrollment Enrollment { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;          // bài đang học (kèm tài liệu, câu hỏi)
    public List<Lesson> Lessons { get; set; } = new();   // tất cả bài theo thứ tự
    public Dictionary<long, LessonProgress> Progress { get; set; } = new();
    public HashSet<long> LockedLessonIds { get; set; } = new();
    public Lesson? Previous { get; set; }
    public Lesson? Next { get; set; }
    public int StudentCount { get; set; }
    public QuizResultViewModel? QuizResult { get; set; }

    public int CompletedCount => Lessons.Count(l => IsCompleted(l.Id));
    public int Percent => Lessons.Count == 0 ? 0 : (int)Math.Round(CompletedCount * 100.0 / Lessons.Count, MidpointRounding.AwayFromZero);
    public bool IsCompleted(long lessonId) => Progress.TryGetValue(lessonId, out var p) && p.IsCompleted;
    public LessonProgress? CurrentProgress => Progress.GetValueOrDefault(Lesson.Id);
}

// Kết quả làm bài kiểm tra (lưu tạm trong TempData dạng JSON)
public class QuizResultViewModel
{
    public long LessonId { get; set; }
    public int Score { get; set; }
    public int PassScore { get; set; }
    public int CorrectCount { get; set; }
    public int Total { get; set; }
    public bool Passed { get; set; }
    public Dictionary<long, QuizAnswerResult> Answers { get; set; } = new();
}

public class QuizAnswerResult
{
    public string? Given { get; set; }        // id lựa chọn (trắc nghiệm) hoặc chữ đã điền
    public bool IsCorrect { get; set; }
}

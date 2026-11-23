using System.Data;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Services;

// Mua khóa học, khóa học đã mua, học bài, làm bài kiểm tra và lưu tiến độ
public class LearningService
{
    private readonly EduFlowDbContext _db;

    public LearningService(EduFlowDbContext db)
    {
        _db = db;
    }

    // ================= DÙNG CHUNG =================

    // Tất cả bài học theo đúng thứ tự chương → bài
    public static List<Lesson> OrderedLessons(Course course) =>
        course.Sections.OrderBy(s => s.OrderIndex)
              .SelectMany(s => s.Lessons.OrderBy(l => l.OrderIndex))
              .ToList();

    // Bài bị khóa: nằm sau một bài kiểm tra chưa đạt (học viên phải đạt điểm mới học tiếp)
    public static HashSet<long> LockedLessons(List<Lesson> lessons, ISet<long> completed)
    {
        var locked = new HashSet<long>();
        var blocked = false;
        foreach (var lesson in lessons)
        {
            if (blocked) locked.Add(lesson.Id);
            if (!completed.Contains(lesson.Id)) blocked = true;
        }
        return locked;
    }

    // Bài nên học tiếp: bài vừa học dở, nếu không thì bài đầu tiên chưa xong và chưa bị khóa
    private static Lesson? ContinueLesson(List<Lesson> lessons, List<LessonProgress> progress, ISet<long> locked)
    {
        var done = progress.Where(p => p.IsCompleted).Select(p => p.LessonId).ToHashSet();
        var last = progress.Where(p => !p.IsCompleted).OrderByDescending(p => p.UpdatedAt).FirstOrDefault();
        if (last != null)
        {
            var lesson = lessons.FirstOrDefault(l => l.Id == last.LessonId && !locked.Contains(l.Id));
            if (lesson != null) return lesson;
        }
        return lessons.FirstOrDefault(l => !done.Contains(l.Id) && !locked.Contains(l.Id)) ?? lessons.FirstOrDefault();
    }

    public async Task<Enrollment?> GetEnrollmentAsync(long userId, long courseId)
    {
        return await _db.Enrollments
            .Include(e => e.Progress)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId && e.Status == EnrollmentStatus.Active && e.User!.IsActive && e.User.DeletedAt == null);
    }

    public async Task<int> CountStudentsAsync(long courseId)
    {
        return await _db.Enrollments.CountAsync(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
    }

    // ================= CHI TIẾT KHÓA HỌC =================
    public async Task<CourseDetailViewModel?> GetCourseDetailAsync(long courseId, long? userId)
    {
        var course = await _db.Courses
            .Include(c => c.Instructor).ThenInclude(u => u!.InstructorProfile)
            .Include(c => c.Category)
            .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Resources)
            .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Questions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null);
        if (course == null) return null;

        var isOwner = userId != null && course.InstructorId == userId;
        var enrollment = userId == null ? null : await GetEnrollmentAsync(userId.Value, courseId);

        // Khóa học chưa công khai: chỉ giảng viên và người đã mua mới xem được
        if (course.Status != CourseStatus.Published && !isOwner && enrollment == null) return null;

        var model = new CourseDetailViewModel
        {
            Course = course,
            Enrollment = enrollment,
            IsOwner = isOwner,
            StudentCount = await CountStudentsAsync(courseId)
        };

        if (enrollment != null)
        {
            var lessons = OrderedLessons(course);
            model.CompletedLessonIds = enrollment.Progress.Where(p => p.IsCompleted).Select(p => p.LessonId).ToHashSet();
            model.LockedLessonIds = LockedLessons(lessons, model.CompletedLessonIds);
            model.ContinueLessonId = ContinueLesson(lessons, enrollment.Progress, model.LockedLessonIds)?.Id;
        }
        return model;
    }

    // ================= KHÓA HỌC ĐÃ MUA =================
    public async Task<MyCoursesViewModel> GetMyCoursesAsync(long userId, string? filter, string? q)
    {
        var enrollments = await _db.Enrollments
            .Include(e => e.Progress)
            .Include(e => e.Course).ThenInclude(c => c!.Instructor)
            .Include(e => e.Course).ThenInclude(c => c!.Category)
            .Include(e => e.Course).ThenInclude(c => c!.Sections).ThenInclude(s => s.Lessons)
            .AsSplitQuery()
            .Where(e => e.UserId == userId && e.Status == EnrollmentStatus.Active && e.Course!.DeletedAt == null)
            .ToListAsync();

        var items = new List<MyCourseItem>();
        foreach (var e in enrollments)
        {
            var lessons = OrderedLessons(e.Course!);
            var ids = lessons.Select(l => l.Id).ToHashSet();
            var progress = e.Progress.Where(p => ids.Contains(p.LessonId)).ToList();
            var done = progress.Where(p => p.IsCompleted).Select(p => p.LessonId).ToHashSet();
            var next = ContinueLesson(lessons, progress, LockedLessons(lessons, done));
            var resume = next == null ? null : progress.FirstOrDefault(p => p.LessonId == next.Id);
            items.Add(new MyCourseItem
            {
                Enrollment = e,
                Course = e.Course!,
                LessonCount = lessons.Count,
                CompletedLessons = done.Count,
                TotalSeconds = lessons.Sum(l => l.DurationSec),
                ContinueLessonId = next?.Id,
                ContinueLessonTitle = next?.Title,
                ResumePositionSec = resume?.LastPositionSec ?? 0,
                WatchedPercent = next?.LessonType == LessonType.Video && next.DurationSec > 0 ? Math.Min(100, (int)Math.Floor((resume?.WatchedSec ?? 0) * 100m / next.DurationSec)) : 0,
                StudySeconds = progress.Sum(p => (long)Math.Max(0,p.WatchedSec)),
                LastActivity = progress.Max(p => p.UpdatedAt) ?? e.EnrolledAt
            });
        }

        var studySeconds = enrollments.Sum(e => e.Progress.Where(p => e.Course!.Sections.SelectMany(s => s.Lessons).Any(l => l.Id == p.LessonId)).Sum(p => (long)Math.Max(0,p.WatchedSec)));
        var model = new MyCoursesViewModel
        {
            StudySeconds = studySeconds,
            Filter = filter is "learning" or "completed" or "not-started" ? filter : "all",
            Query = q?.Trim(),
            TotalCount = items.Count,
            CompletedCount = items.Count(i => i.IsCompleted),
            NotStartedCount = items.Count(i => !i.IsStarted),
            TotalCompletedLessons = items.Sum(i => i.CompletedLessons)
        };
        model.LearningCount = model.TotalCount - model.CompletedCount - model.NotStartedCount;

        IEnumerable<MyCourseItem> list = items;
        if (!string.IsNullOrEmpty(model.Query))
            list = list.Where(i => i.Course.Title.Contains(model.Query, StringComparison.OrdinalIgnoreCase)
                                || (i.Course.Instructor?.FullName ?? "").Contains(model.Query, StringComparison.OrdinalIgnoreCase));
        list = model.Filter switch
        {
            "learning" => list.Where(i => i.IsStarted && !i.IsCompleted),
            "completed" => list.Where(i => i.IsCompleted),
            "not-started" => list.Where(i => !i.IsStarted),
            _ => list
        };
        model.Items = list.OrderByDescending(i => i.LastActivity).ToList();
        return model;
    }

    // ================= TRANG HỌC =================
    // Trả về null nếu chưa mua khóa học. lessonId null = tự chọn bài nên học tiếp.
    public async Task<LearnViewModel?> GetLearnAsync(long userId, long courseId, long? lessonId)
    {
        var enrollment = await GetEnrollmentAsync(userId, courseId);
        if (enrollment == null) return null;

        var course = await _db.Courses
            .Include(c => c.Instructor).ThenInclude(u => u!.InstructorProfile)
            .Include(c => c.Category)
            .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Resources)
            .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Questions).ThenInclude(q => q.Options)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null);
        if (course == null) return null;

        var lessons = OrderedLessons(course);
        var progress = enrollment.Progress.ToDictionary(p => p.LessonId);
        var done = progress.Values.Where(p => p.IsCompleted).Select(p => p.LessonId).ToHashSet();
        var locked = LockedLessons(lessons, done);

        var model = new LearnViewModel
        {
            Course = course,
            Enrollment = enrollment,
            Lessons = lessons,
            Progress = progress,
            LockedLessonIds = locked,
            StudentCount = await CountStudentsAsync(courseId)
        };
        if (lessons.Count == 0) return model;

        var current = lessonId == null ? null : lessons.FirstOrDefault(l => l.Id == lessonId);
        current ??= ContinueLesson(lessons, enrollment.Progress, locked)!;
        model.Lesson = current;
        var index = lessons.IndexOf(current);
        model.Previous = index > 0 ? lessons[index - 1] : null;
        model.Next = index < lessons.Count - 1 ? lessons[index + 1] : null;
        return model;
    }

    // Lấy (hoặc tạo) dòng tiến độ của bài học; null nếu chưa mua khóa học chứa bài này
    private async Task<(Enrollment Enrollment, Lesson Lesson, LessonProgress Progress)?> FindProgressAsync(long userId, long lessonId)
    {
        var lesson = await _db.Lessons
            .Include(l => l.Section)
            .Include(l => l.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null || !await _db.Courses.AnyAsync(c => c.Id == lesson.Section!.CourseId && c.DeletedAt == null)) return null;
        var enrollment = await _db.Enrollments.FromSqlInterpolated($"SELECT * FROM enrollments WITH (UPDLOCK,HOLDLOCK) WHERE user_id={userId} AND course_id={lesson.Section!.CourseId}")
            .Include(e => e.Progress).FirstOrDefaultAsync(e => e.Status == EnrollmentStatus.Active && e.User!.IsActive && e.User.DeletedAt == null);
        if (enrollment == null) return null;

        var progress = enrollment.Progress.FirstOrDefault(p => p.LessonId == lessonId);
        if (progress == null)
        {
            progress = new LessonProgress { EnrollmentId = enrollment.Id, LessonId = lessonId };
            enrollment.Progress.Add(progress);
        }
        return (enrollment, lesson, progress);
    }

    // Kiểm tra bài có đang bị khóa không (nằm sau bài kiểm tra chưa đạt)
    public async Task<bool> IsLockedAsync(Enrollment enrollment, long lessonId)
    {
        var course = await _db.Courses.Include(c => c.Sections).ThenInclude(s => s.Lessons)
            .FirstAsync(c => c.Id == enrollment.CourseId);
        var lessons = OrderedLessons(course);
        var done = enrollment.Progress.Where(p => p.IsCompleted).Select(p => p.LessonId).ToHashSet();
        return LockedLessons(lessons, done).Contains(lessonId);
    }

    // Tính lại % hoàn thành của khóa học
    public async Task UpdateCourseProgressAsync(Enrollment enrollment)
    {
        var lessonIds = await _db.Lessons.Where(l => l.Section!.CourseId == enrollment.CourseId).Select(l => l.Id).ToListAsync();
        var done = enrollment.Progress.Count(p => p.IsCompleted && lessonIds.Contains(p.LessonId));
        enrollment.ProgressPercent = lessonIds.Count == 0 ? 0 : Math.Round(done * 100m / lessonIds.Count, 2);
        if (lessonIds.Count > 0 && done >= lessonIds.Count) enrollment.CompletedAt ??= DateTime.Now;
        else enrollment.CompletedAt = null;
    }

    // Đánh dấu hoàn thành bài video / bài viết. Trả về % tiến độ khóa học (null = không hợp lệ).
    public async Task<decimal?> CompleteLessonAsync(long userId, long lessonId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var found = await FindProgressAsync(userId, lessonId);
        if (found == null) return null;
        var (enrollment, lesson, progress) = found.Value;
        if (lesson.LessonType == LessonType.Quiz) return null;                // bài kiểm tra phải làm đạt
        if (await IsLockedAsync(enrollment, lessonId)) return null;

        if (lesson.LessonType == LessonType.Video && !progress.IsCompleted) return null;
        progress.IsCompleted = true;
        progress.UpdatedAt = DateTime.Now;
        await UpdateCourseProgressAsync(enrollment);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return enrollment.ProgressPercent;
    }

    // Chấm bài kiểm tra. answers: id câu hỏi → id lựa chọn (trắc nghiệm) hoặc chữ đã điền (điền khuyết)
    public async Task<QuizResultViewModel?> SubmitQuizAsync(long userId, long lessonId, Dictionary<long, string?> answers)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var found = await FindProgressAsync(userId, lessonId);
        if (found == null) return null;
        var (enrollment, lesson, progress) = found.Value;
        if (lesson.LessonType != LessonType.Quiz || lesson.Questions.Count == 0) return null;
        if (await IsLockedAsync(enrollment, lessonId)) return null;

        var result = new QuizResultViewModel { LessonId = lessonId, PassScore = lesson.PassScore ?? 70, Total = lesson.Questions.Count };
        foreach (var q in lesson.Questions)
        {
            var given = answers.GetValueOrDefault(q.Id)?.Trim();
            bool correct;
            if (q.QuestionType == "fill")
                correct = !string.IsNullOrEmpty(given) && q.Options.Any(o => string.Equals(o.OptionText.Trim(), given, StringComparison.OrdinalIgnoreCase));
            else
                correct = long.TryParse(given, out var optionId) && q.Options.Any(o => o.Id == optionId && o.IsCorrect);
            if (correct) result.CorrectCount++;
            result.Answers[q.Id] = new QuizAnswerResult { Given = given, IsCorrect = correct };
        }
        result.Score = (int)Math.Round(result.CorrectCount * 100.0 / result.Total);
        result.Passed = result.Score >= result.PassScore;

        // Giữ điểm cao nhất; đạt thì tính là hoàn thành
        progress.QuizScore = Math.Max(progress.QuizScore ?? 0, result.Score);
        if (result.Passed) progress.IsCompleted = true;
        progress.UpdatedAt = DateTime.Now;
        await UpdateCourseProgressAsync(enrollment);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return result;
    }
}

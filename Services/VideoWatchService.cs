using System.Data;
using EduFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Services;
public class VideoWatchService
{
    private readonly EduFlowDbContext _db;
    private readonly LearningService _learning;
    public VideoWatchService(EduFlowDbContext db, LearningService learning)
    {
        _db = db;
        _learning = learning;
    }

    // Mở một phiên xem cho bài hiện tại. Mở tab mới sẽ thay phiên cũ.
    public async Task<Guid?> StartAsync(long userId, long lessonId, int position)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var lesson = await _db.Lessons.Include(lesson => lesson.Section)
            .FirstOrDefaultAsync(lesson => lesson.Id == lessonId && lesson.LessonType == LessonType.Video && lesson.Section!.Course!.DeletedAt == null);
        if (lesson == null || lesson.DurationSec <= 0) return null;

        var enrollment = await _db.Enrollments
            .FromSqlInterpolated($"SELECT * FROM enrollments WITH (UPDLOCK,HOLDLOCK) WHERE user_id={userId} AND course_id={lesson.Section!.CourseId}")
            .Include(enrollment => enrollment.Progress)
            .FirstOrDefaultAsync(enrollment => enrollment.Status == EnrollmentStatus.Active && enrollment.User!.IsActive && enrollment.User.DeletedAt == null);
        if (enrollment == null || await _learning.IsLockedAsync(enrollment, lessonId)) return null;

        // Không cho hai tab ở hai đoạn khác nhau cùng cộng thời gian cho một bài.
        await _db.VideoWatchSessions.Where(session => session.EnrollmentId == enrollment.Id)
            .ExecuteDeleteAsync();
        var session = new VideoWatchSession
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollment.Id,
            LessonId = lesson.Id,
            PositionSec = Math.Clamp(position, 0, lesson.DurationSec),
            LastSeenUtc = DateTime.UtcNow
        };
        var progress = enrollment.Progress.FirstOrDefault(p => p.LessonId == lessonId);
        if (progress == null)
        {
            progress = new LessonProgress { EnrollmentId = enrollment.Id, LessonId = lessonId, LastPositionSec = session.PositionSec };
            enrollment.Progress.Add(progress);
        }
        // YouTube có thể báo 0 trước khi seek tới vị trí cũ; không xóa vị trí đã lưu ở bước mở phiên.
        else if (session.PositionSec > 0) progress.LastPositionSec = session.PositionSec;
        progress.UpdatedAt = DateTime.Now;
        await _learning.UpdateCourseProgressAsync(enrollment);
        _db.VideoWatchSessions.Add(session);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return session.Id;
    }

    public static bool IsPlausible(int oldPosition, int position, double elapsed, bool playing) => playing && elapsed >= 1 && elapsed <= 35 && position > oldPosition && position - oldPosition <= elapsed * 1.25 + 1;
    public static List<(int Start, int End)> Merge(IEnumerable<(int Start, int End)> ranges)
    {
        var result = new List<(int Start, int End)>();
        foreach (var r in ranges.OrderBy(r => r.Start))
        {
            if (r.End <= r.Start)
                continue;
            if (result.Count == 0 || result[^1].End < r.Start)
                result.Add(r);
            else
                result[^1] = (result[^1].Start, Math.Max(result[^1].End, r.End));
        }

        return result;
    }

    public async Task<object?> HeartbeatAsync(long userId, Guid sessionId, int position, bool playing)
    {
        // Lấy mã ghi danh trước, rồi khóa theo cùng thứ tự với hàm StartAsync.
        var snapshot = await _db.VideoWatchSessions.AsNoTracking().FirstOrDefaultAsync(session => session.Id == sessionId);
        if (snapshot == null) return null;
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var enrollment = await _db.Enrollments
            .FromSqlInterpolated($"SELECT * FROM enrollments WITH (UPDLOCK,HOLDLOCK) WHERE id={snapshot.EnrollmentId}")
            .Include(enrollment => enrollment.Progress)
            .FirstOrDefaultAsync(enrollment => enrollment.UserId == userId && enrollment.Status == EnrollmentStatus.Active && enrollment.User!.IsActive && enrollment.User.DeletedAt == null);
        if (enrollment == null || await _learning.IsLockedAsync(enrollment, snapshot.LessonId)) return null;
        var session = await _db.VideoWatchSessions
            .FromSqlInterpolated($"SELECT * FROM video_watch_sessions WITH (UPDLOCK,HOLDLOCK) WHERE id={sessionId}")
            .FirstOrDefaultAsync();
        if (session == null) return null;
        var lesson = await _db.Lessons.FirstOrDefaultAsync(courseLesson => courseLesson.Id == session.LessonId && courseLesson.Section!.CourseId == enrollment.CourseId && courseLesson.Section.Course!.DeletedAt == null);
        if (lesson == null || lesson.DurationSec <= 0)
            return null;
        var now = DateTime.UtcNow;
        position = Math.Clamp(position, 0, lesson.DurationSec);
        var progress = enrollment.Progress.FirstOrDefault(lessonProgress => lessonProgress.LessonId == lesson.Id);
        if (progress == null)
        {
            progress = new()
            {
                EnrollmentId = enrollment.Id,
                LessonId = lesson.Id
            };
            enrollment.Progress.Add(progress);
        }

        if (IsPlausible(session.PositionSec, position, (now - session.LastSeenUtc).TotalSeconds, playing))
        {
            var existing = await _db.VideoWatchSegments.Where(segment => segment.EnrollmentId == enrollment.Id && segment.LessonId == lesson.Id).ToListAsync();
            var ranges = Merge(existing.Select(segment => (segment.StartSec, segment.EndSec)).Append((session.PositionSec, position)));
            // Gộp khoảng đã xem để xem lại hoặc mở hai tab không được tính thời gian hai lần.
            foreach (var old in existing.Where(segment => !ranges.Any(range => range.Start == segment.StartSec)))
                _db.VideoWatchSegments.Remove(old);
            foreach (var range in ranges)
            {
                var row = existing.FirstOrDefault(segment => segment.StartSec == range.Start);
                if (row == null)
                    _db.VideoWatchSegments.Add(new() { EnrollmentId = enrollment.Id, LessonId = lesson.Id, StartSec = range.Start, EndSec = range.End });
                else
                    row.EndSec = range.End;
            }

            progress.WatchedSec = Math.Max(progress.WatchedSec, ranges.Sum(range => range.End - range.Start));
            if (progress.WatchedSec >= Math.Ceiling(lesson.DurationSec * 0.8))
                progress.IsCompleted = true;
        }

        session.PositionSec = position;
        session.LastSeenUtc = now;
        progress.LastPositionSec = position;
        progress.UpdatedAt = DateTime.Now;
        if (progress.WatchedSec >= Math.Ceiling(lesson.DurationSec * 0.8)) progress.IsCompleted = true;
        await _learning.UpdateCourseProgressAsync(enrollment);
        var lessonIds = await _db.Lessons.Where(l => l.Section!.CourseId == enrollment.CourseId).Select(l => l.Id).ToListAsync();
        var completedCount = enrollment.Progress.Count(p => p.IsCompleted && lessonIds.Contains(p.LessonId));
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return new
        {
            completed = progress.IsCompleted,
            watched = progress.WatchedSec,
            duration = lesson.DurationSec,
            percent = enrollment.ProgressPercent,
            completedCount,
            position = progress.LastPositionSec,
            savedAt = progress.UpdatedAt
        };
    }
}

using System.Data;
using EduFlow.Web.Models;
using Microsoft.EntityFrameworkCore;
namespace EduFlow.Web.Services;
// Dùng bảng phiên xem hiện có cho bài viết/quiz, không tin số giây do client gửi.
public class StudyClockService
{
    private readonly EduFlowDbContext _db;private readonly LearningService _learning;
    public StudyClockService(EduFlowDbContext db,LearningService learning){_db=db;_learning=learning;}
    public static int CountSeconds(double elapsed,bool wasActive)=>wasActive&&elapsed>=1&&elapsed<=35?(int)Math.Floor(elapsed):0;
    public async Task<Guid?> StartAsync(long userId,long lessonId)
    {
        var lesson=await _db.Lessons.Include(l=>l.Section).FirstOrDefaultAsync(l=>l.Id==lessonId&&l.LessonType!=LessonType.Video&&l.Section!.Course!.DeletedAt==null);if(lesson==null)return null;
        if(!await _db.Users.AnyAsync(u=>u.Id==userId&&u.IsActive&&u.DeletedAt==null))return null;
        await using var tx=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var enrollment=await _db.Enrollments.FromSqlInterpolated($"SELECT * FROM enrollments WITH (UPDLOCK,HOLDLOCK) WHERE user_id={userId} AND course_id={lesson.Section!.CourseId}").Include(e=>e.Progress).FirstOrDefaultAsync(e=>e.Status==EnrollmentStatus.Active);
        if(enrollment==null||await _learning.IsLockedAsync(enrollment,lessonId))return null;
        await _db.VideoWatchSessions.Where(s=>s.EnrollmentId==enrollment.Id).ExecuteDeleteAsync();
        var session=new VideoWatchSession{Id=Guid.NewGuid(),EnrollmentId=enrollment.Id,LessonId=lessonId,PositionSec=1,LastSeenUtc=DateTime.UtcNow};_db.VideoWatchSessions.Add(session);await _db.SaveChangesAsync();await tx.CommitAsync();return session.Id;
    }
    public async Task<int?> TickAsync(long userId,Guid id,bool active)
    {
        var snapshot=await _db.VideoWatchSessions.AsNoTracking().FirstOrDefaultAsync(s=>s.Id==id);if(snapshot==null)return null;
        await using var tx=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var enrollment=await _db.Enrollments.FromSqlInterpolated($"SELECT * FROM enrollments WITH (UPDLOCK,HOLDLOCK) WHERE id={snapshot.EnrollmentId}").Include(e=>e.Progress).FirstOrDefaultAsync(e=>e.UserId==userId&&e.Status==EnrollmentStatus.Active&&e.User!.IsActive&&e.User.DeletedAt==null);
        if(enrollment==null||await _learning.IsLockedAsync(enrollment,snapshot.LessonId))return null;
        var session=await _db.VideoWatchSessions.FromSqlInterpolated($"SELECT * FROM video_watch_sessions WITH (UPDLOCK,HOLDLOCK) WHERE id={id}").FirstOrDefaultAsync();if(session==null)return null;
        var lesson=await _db.Lessons.FirstOrDefaultAsync(l=>l.Id==session.LessonId&&l.LessonType!=LessonType.Video&&l.Section!.CourseId==enrollment.CourseId&&l.Section.Course!.DeletedAt==null);if(lesson==null)return null;
        var now=DateTime.UtcNow;var seconds=CountSeconds((now-session.LastSeenUtc).TotalSeconds,session.PositionSec==1);
        var progress=enrollment.Progress.FirstOrDefault(p=>p.LessonId==lesson.Id);if(progress==null){progress=new LessonProgress{EnrollmentId=enrollment.Id,LessonId=lesson.Id};enrollment.Progress.Add(progress);}
        progress.WatchedSec=(int)Math.Min(int.MaxValue,(long)Math.Max(0,progress.WatchedSec)+seconds);progress.UpdatedAt=DateTime.Now;
        session.PositionSec=active?1:0;session.LastSeenUtc=now;await _db.SaveChangesAsync();await tx.CommitAsync();return progress.WatchedSec;
    }
}

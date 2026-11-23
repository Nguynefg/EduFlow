using System.Data;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace EduFlow.Web.Services;

public class CommunityService
{
    private readonly EduFlowDbContext _db;
    public CommunityService(EduFlowDbContext db) { _db = db; }
    private Task<bool> Active(long userId) => _db.Users.AnyAsync(u => u.Id == userId && u.IsActive && u.DeletedAt == null);
    private Task<bool> Admin(long userId) => _db.UserRoles.AnyAsync(r => r.UserId == userId && r.Role!.Code == "admin");
    private async Task<bool> Manager(long userId, Course course) => await Active(userId) && (course.InstructorId == userId || await Admin(userId));
    private async Task<decimal> Progress(long userId, long courseId)
    {
        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
        if (enrollment == null) return 0;
        var lessons = _db.Lessons.Where(l => l.Section!.CourseId == courseId);
        var total = await lessons.CountAsync();
        var done = await _db.LessonProgress.CountAsync(p => p.EnrollmentId == enrollment.Id && p.IsCompleted && lessons.Any(l => l.Id == p.LessonId));
        return total == 0 ? 0 : done * 100m / total;
    }
    public async Task<ReviewsViewModel> ReviewsAsync(Course course, long? userId, int page)
    {
        var visible = _db.CourseReviews.AsNoTracking().Where(r => r.CourseId == course.Id && !r.IsHidden);
        var stars = await visible.GroupBy(r => r.Rating).Select(g => new { Star = g.Key, Count = g.Count() }).ToListAsync();
        var total = stars.Sum(g => g.Count);
        var model = new ReviewsViewModel { CourseId = course.Id, Total = total, Stars = stars.ToDictionary(g => g.Star,g => g.Count), Average = total == 0 ? 0 : Math.Round(stars.Sum(g => g.Star * g.Count) / (decimal)total,2) };
        model.Page = Math.Clamp(page,1,model.Pages);
        model.Items = await visible.Include(r => r.User).OrderByDescending(r => r.CreatedAt).ThenBy(r => r.UserId).Skip((model.Page-1)*10).Take(10).ToListAsync();
        if (userId.HasValue && await Active(userId.Value))
        {
            var percent = await Progress(userId.Value,course.Id);
            model.Percent = (int)decimal.Floor(percent);
            model.CanReview = userId != course.InstructorId && percent >= 30;
            model.CanReply = await Manager(userId.Value,course);
            model.IsAdmin = await Admin(userId.Value);
            model.Mine = await _db.CourseReviews.AsNoTracking().FirstOrDefaultAsync(r => r.CourseId == course.Id && r.UserId == userId.Value);
        }
        return model;
    }
    private async Task UpdateRating(Course course)
    {
        var ratings = _db.CourseReviews.Where(r => r.CourseId == course.Id && !r.IsHidden);
        var total = await ratings.CountAsync();
        var sum = await ratings.SumAsync(r => (int?)r.Rating) ?? 0;
        course.AvgRating = total == 0 ? 0 : Math.Round(sum/(decimal)total,2);
        await _db.SaveChangesAsync();
    }
    public async Task<string?> ReviewAsync(long userId, long courseId, int rating, string? comment)
    {
        if (rating < 1 || rating > 5 || comment?.Length > 3000) return "Chọn 1–5 sao, nhận xét tối đa 3000 ký tự.";
        if (!await Active(userId)) return "Tài khoản không được phép thao tác.";
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var course = await _db.Courses.FromSqlInterpolated($"SELECT * FROM courses WITH (UPDLOCK,HOLDLOCK) WHERE id={courseId}").FirstOrDefaultAsync();
        if (course == null || course.DeletedAt != null || course.InstructorId == userId || await Progress(userId,courseId) < 30)
            return "Bạn cần có quyền học và hoàn thành ít nhất 30% khóa học để đánh giá.";
        var review = await _db.CourseReviews.FindAsync(courseId,userId);
        if (review == null) { review = new CourseReview { CourseId=courseId, UserId=userId }; _db.CourseReviews.Add(review); }
        review.Rating=(byte)rating; review.Comment=comment?.Trim(); review.UpdatedAt=DateTime.Now;
        // Sửa nhận xét không tự bỏ trạng thái bị admin ẩn.
        await _db.SaveChangesAsync(); await UpdateRating(course); await tx.CommitAsync(); return null;
    }
    public async Task<string?> ReviewReplyAsync(long userId,long courseId,long reviewerId,string? reply)
    {
        if (string.IsNullOrWhiteSpace(reply) || reply.Length > 3000) return "Nhập phản hồi tối đa 3000 ký tự.";
        var course=await _db.Courses.FirstOrDefaultAsync(c=>c.Id==courseId && c.DeletedAt==null);
        if(course==null || !await Manager(userId,course)) return "Bạn không có quyền phản hồi đánh giá này.";
        var review=await _db.CourseReviews.FindAsync(courseId,reviewerId);
        if(review==null) return "Không tìm thấy đánh giá.";
        review.InstructorReply=reply.Trim(); await _db.SaveChangesAsync(); return null;
    }
    public async Task<string?> HideReviewAsync(long userId,long courseId,long reviewerId,bool hidden)
    {
        if(!await Active(userId) || !await Admin(userId)) return "Chỉ admin được ẩn/hiện đánh giá.";
        await using var tx=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var course=await _db.Courses.FromSqlInterpolated($"SELECT * FROM courses WITH (UPDLOCK,HOLDLOCK) WHERE id={courseId}").FirstOrDefaultAsync();
        var review=await _db.CourseReviews.FindAsync(courseId,reviewerId);
        if(course==null || review==null) return "Không tìm thấy đánh giá.";
        review.IsHidden=hidden; await _db.SaveChangesAsync(); await UpdateRating(course); await tx.CommitAsync();return null;
    }
    // Quyền hỏi đáp giống quyền học, kiểm tra cả bài đang bị khóa ở server.
    private async Task<(Lesson Lesson,bool Manager)?> Access(long userId,long lessonId)
    {
        if(!await Active(userId)) return null;
        var lesson=await _db.Lessons.Include(l=>l.Section).ThenInclude(s=>s!.Course).FirstOrDefaultAsync(l=>l.Id==lessonId);
        if(lesson?.Section?.Course==null || lesson.Section.Course.DeletedAt!=null) return null;
        if(await Manager(userId,lesson.Section.Course)) return (lesson,true);
        var enrollment=await _db.Enrollments.FirstOrDefaultAsync(e=>e.UserId==userId && e.CourseId==lesson.Section.CourseId && e.Status==EnrollmentStatus.Active);
        if(enrollment==null) return null;
        var lessons=await _db.Lessons.Where(l=>l.Section!.CourseId==enrollment.CourseId).OrderBy(l=>l.Section!.OrderIndex).ThenBy(l=>l.OrderIndex).ThenBy(l=>l.Id).ToListAsync();
        var done=await _db.LessonProgress.Where(p=>p.EnrollmentId==enrollment.Id && p.IsCompleted).Select(p=>p.LessonId).ToListAsync();
        return LearningService.LockedLessons(lessons,done.ToHashSet()).Contains(lessonId) ? null : (lesson,false);
    }
    public async Task<DiscussionViewModel?> DiscussionsAsync(long userId,long lessonId,string? query,int page)
    {
        var access=await Access(userId,lessonId); if(access==null) return null;
        var manager=access.Value.Manager;
        var roots=_db.Discussions.AsNoTracking().Where(d=>d.LessonId==lessonId && d.ParentId==null && (manager || !d.IsHidden));
        query=query?.Trim(); if(query?.Length>100) query=query[..100];
        if(!string.IsNullOrEmpty(query)) roots=roots.Where(d=>(d.Title!=null && d.Title.Contains(query)) || d.Content.Contains(query) || _db.Discussions.Any(a=>a.ParentId==d.Id && a.LessonId==lessonId && (manager || !a.IsHidden) && a.Content.Contains(query)));
        var model=new DiscussionViewModel{Lesson=access.Value.Lesson, CourseId=access.Value.Lesson.Section!.CourseId, CourseTitle=access.Value.Lesson.Section.Course!.Title, UserId=userId,CanModerate=manager, Query=query, Total=await roots.CountAsync()};
        model.Page=Math.Clamp(page,1,model.Pages);
        model.Questions=await roots.Include(d=>d.User).OrderByDescending(d=>d.IsPinned).ThenByDescending(d=>d.CreatedAt).ThenByDescending(d=>d.Id).Skip((model.Page-1)*10).Take(10).ToListAsync();
        var ids=model.Questions.Select(d=>d.Id).ToList();
        model.Answers=await _db.Discussions.AsNoTracking().Include(d=>d.User).Where(d=>d.LessonId==lessonId && d.ParentId!=null && ids.Contains(d.ParentId.Value) && (manager || !d.IsHidden)).OrderBy(d=>d.CreatedAt).ThenBy(d=>d.Id).ToListAsync();
        var all=ids.Concat(model.Answers.Select(d=>d.Id)).ToList();
        model.VotedIds=(await _db.DiscussionVotes.Where(v=>v.UserId==userId && all.Contains(v.DiscussionId)).Select(v=>v.DiscussionId).ToListAsync()).ToHashSet();return model;
    }
    public async Task<string?> PostAsync(long userId,long lessonId,string? title,string? content,long? parentId,int? timestamp)
    {
        if(string.IsNullOrWhiteSpace(content) || content.Length>5000 || title?.Length>255) return "Nhập nội dung tối đa 5000 ký tự, tiêu đề tối đa 255 ký tự.";
        var access=await Access(userId,lessonId);if(access==null) return "Bạn không có quyền hỏi đáp tại bài học này.";
        if(parentId==null && string.IsNullOrWhiteSpace(title)) return "Nhập tiêu đề câu hỏi.";
        if(timestamp.HasValue && (parentId!=null || access.Value.Lesson.LessonType!=LessonType.Video || timestamp<0 || timestamp>access.Value.Lesson.DurationSec)) return "Mốc thời gian không hợp lệ với video này.";
        await using var tx=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if(parentId.HasValue){var parent=await _db.Discussions.FromSqlInterpolated($"SELECT * FROM discussions WITH (UPDLOCK,HOLDLOCK) WHERE id={parentId.Value}").FirstOrDefaultAsync();if(parent==null || parent.ParentId!=null || parent.LessonId!=lessonId || parent.IsHidden) return "Câu hỏi không còn nhận phản hồi.";}
        _db.Discussions.Add(new Discussion{LessonId=lessonId,UserId=userId,ParentId=parentId,Title=parentId==null?title?.Trim():null,Content=content.Trim(),VideoTimestampSec=timestamp});
        await _db.SaveChangesAsync();await tx.CommitAsync();return null;
    }
    public async Task<string?> VoteAsync(long userId,long lessonId,long id)
    {
        if(await Access(userId,lessonId)==null) return "Bạn không có quyền bình chọn.";
        await using var tx=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var item=await _db.Discussions.FromSqlInterpolated($"SELECT * FROM discussions WITH (UPDLOCK,HOLDLOCK) WHERE id={id}").FirstOrDefaultAsync();
        if(item==null || item.LessonId!=lessonId || item.IsHidden || item.UserId==userId) return "Không thể bình chọn nội dung này hoặc nội dung của chính mình.";
        if(item.ParentId.HasValue && !await _db.Discussions.AnyAsync(p=>p.Id==item.ParentId && !p.IsHidden)) return "Câu hỏi đã bị ẩn.";
        var vote=await _db.DiscussionVotes.FindAsync(id,userId);
        if(vote==null)_db.DiscussionVotes.Add(new DiscussionVote{DiscussionId=id,UserId=userId});else _db.DiscussionVotes.Remove(vote);
        await _db.SaveChangesAsync();item.UpvoteCount=await _db.DiscussionVotes.CountAsync(v=>v.DiscussionId==id);await _db.SaveChangesAsync();await tx.CommitAsync();return null;
    }
    public async Task<string?> ModerateAsync(long userId,long lessonId,long id,string action)
    {
        var access=await Access(userId,lessonId);if(access==null || !access.Value.Manager) return "Chỉ giảng viên sở hữu hoặc admin được quản lý hỏi đáp.";
        await using var tx=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var item=await _db.Discussions.FromSqlInterpolated($"SELECT * FROM discussions WITH (UPDLOCK,HOLDLOCK) WHERE id={id}").FirstOrDefaultAsync();
        if(item==null || item.LessonId!=lessonId) return "Không tìm thấy nội dung.";
        if(action=="hide")item.IsHidden=true;else if(action=="show")item.IsHidden=false;
        else if(action=="pin" && item.ParentId==null)item.IsPinned=!item.IsPinned;
        else if(action=="resolve" && item.ParentId==null)item.IsResolved=!item.IsResolved;
        else return "Thao tác không hợp lệ.";
        await _db.SaveChangesAsync();await tx.CommitAsync();return null;
    }
    public async Task<List<Lesson>> ManageAsync(long userId)
    {
        if(!await Active(userId)) return new();
        var admin=await Admin(userId);
        return await _db.Lessons.AsNoTracking().Include(l=>l.Section).ThenInclude(s=>s!.Course).Where(l=>l.Section!.Course!.DeletedAt==null && (admin || l.Section.Course.InstructorId==userId)).OrderBy(l=>l.Section!.Course!.Title).ThenBy(l=>l.Section!.OrderIndex).ThenBy(l=>l.OrderIndex).ToListAsync();
    }
}

using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using Microsoft.EntityFrameworkCore;
namespace EduFlow.Web.Services;
public class ProtectedVideoService
{
    private readonly EduFlowDbContext _db;private readonly LearningService _learning;private readonly IWebHostEnvironment _env;private readonly MediaOwnershipService _ownership;
    public ProtectedVideoService(EduFlowDbContext db,LearningService learning,IWebHostEnvironment env,MediaOwnershipService ownership){_db=db;_learning=learning;_env=env;_ownership=ownership;}
    public async Task<Lesson?> LessonAsync(long? userId,long lessonId)
    {
        var lesson=await _db.Lessons.Include(l=>l.Section).ThenInclude(s=>s!.Course).ThenInclude(c=>c!.Instructor).FirstOrDefaultAsync(l=>l.Id==lessonId);
        var course=lesson?.Section?.Course;if(course==null||course.DeletedAt!=null)return null;
        var user=userId.HasValue?await _db.Users.Include(u=>u.UserRoles).ThenInclude(r=>r.Role).FirstOrDefaultAsync(u=>u.Id==userId&&u.IsActive&&u.DeletedAt==null):null;
        if(user!=null&&(course.InstructorId==user.Id||user.RoleCodes.Contains(AppRoles.Admin)))return lesson;
        if(lesson!.IsFreePreview&&course.Status==CourseStatus.Published&&course.Instructor?.IsActive==true&&course.Instructor.DeletedAt==null)return lesson;
        if(user==null)return null;
        var enrollment=await _learning.GetEnrollmentAsync(user.Id,course.Id);
        return enrollment!=null&&!await _learning.IsLockedAsync(enrollment,lessonId)?lesson:null;
    }
    public async Task<Course?> CourseAsync(long? userId,long id)
    {
        var course=await _db.Courses.Include(c=>c.Instructor).FirstOrDefaultAsync(c=>c.Id==id&&c.DeletedAt==null);if(course==null)return null;
        if(course.Status==CourseStatus.Published&&course.Instructor?.IsActive==true&&course.Instructor.DeletedAt==null)return course;
        if(userId.HasValue&&await _db.Users.AnyAsync(u=>u.Id==userId&&u.IsActive&&u.DeletedAt==null&&(course.InstructorId==u.Id||u.UserRoles.Any(r=>r.Role!.Code==AppRoles.Admin))))return course;
        return null;
    }
    public Task<bool> OwnsVideoAsync(long ownerId, string? url) => _ownership.CanUseAsync(ownerId, url);
    public async Task<LessonResource?> ResourceAsync(long? userId, long id)
    {
        var resource = await _db.LessonResources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return resource != null && await LessonAsync(userId, resource.LessonId) != null ? resource : null;
    }
    public string? ResourcePath(string? url)
    {
        const string prefix = "/uploads/resources/";
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith(prefix, StringComparison.Ordinal)) return null;
        var name = url[prefix.Length..];
        if (name != Path.GetFileName(name) || name.Contains('\\') || !FileUpload.DocumentExtensions.Contains(Path.GetExtension(name).ToLowerInvariant())) return null;
        foreach (var root in new[] { Path.Combine(_env.ContentRootPath, "App_Data"), _env.WebRootPath })
        {
            if (string.IsNullOrEmpty(root)) continue;
            var path = Path.Combine(root, "uploads", "resources", name);
            if (File.Exists(path)) return path;
        }
        return null;
    }
    public string? LocalPath(string? url)
    {
        if(string.IsNullOrWhiteSpace(url)||!url.StartsWith("/uploads/",StringComparison.Ordinal)||url.Contains('\\')||url.Contains('?')||url.Contains('#')||url.Split('/').Any(p=>p is "." or ".."))return null;
        var ext=Path.GetExtension(url).ToLowerInvariant();if(ext!=".mp4"&&ext!=".webm")return null;
        foreach(var root in new[]{Path.Combine(_env.ContentRootPath,"App_Data"),_env.WebRootPath})
        {
            if(string.IsNullOrEmpty(root))continue;
            var boundary=Path.GetFullPath(Path.Combine(root,"uploads"))+Path.DirectorySeparatorChar;
            var file=Path.GetFullPath(Path.Combine(root,url.TrimStart('/')));
            if(file.StartsWith(boundary,StringComparison.OrdinalIgnoreCase)&&File.Exists(file))return file;
        }
        return null;
    }
}

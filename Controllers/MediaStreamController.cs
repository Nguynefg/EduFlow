using EduFlow.Web.Helpers;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;
namespace EduFlow.Web.Controllers;
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class MediaStreamController:Controller
{
    private readonly ProtectedVideoService _video;
    public MediaStreamController(ProtectedVideoService video){_video=video;}
    private long? UserId=>User.Identity?.IsAuthenticated==true?AuthHelper.GetUserId(User):null;
    private async Task<IActionResult> StreamVideo(long ownerId, string? url)
    {
        if (!await _video.OwnsVideoAsync(ownerId,url)) return NotFound();
        var path=_video.LocalPath(url);if(path==null)return NotFound();Response.Headers.CacheControl="private, no-store";Response.Headers["X-Content-Type-Options"]="nosniff";
        return PhysicalFile(path,Path.GetExtension(path).Equals(".webm",StringComparison.OrdinalIgnoreCase)?"video/webm":"video/mp4",enableRangeProcessing:true);
    }
    public async Task<IActionResult> Resource(long id)
    {
        var resource = await _video.ResourceAsync(UserId, id);
        if (resource == null) return NotFound();
        var path = _video.ResourcePath(resource.FileUrl);
        if (path == null) return NotFound();
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(path, "application/octet-stream", Path.GetFileName(resource.FileName));
    }
    public async Task<IActionResult> Lesson(long id)
    {var lesson=await _video.LessonAsync(UserId,id);return lesson==null?NotFound():await StreamVideo(lesson.Section!.Course!.InstructorId, lesson.VideoUrl);}
    public async Task<IActionResult> Trailer(long id)
    {var course=await _video.CourseAsync(UserId,id);return course==null?NotFound():await StreamVideo(course.InstructorId, course.TrailerUrl);}
    public async Task<IActionResult> Attachment(long lessonId,int index)
    {var lesson=await _video.LessonAsync(UserId,lessonId);if(lesson==null)return NotFound();var sources=VideoHelper.LocalSources(lesson.Content);return index>=0&&index<sources.Count?await StreamVideo(lesson.Section!.Course!.InstructorId, sources[index]):NotFound();}
    public async Task<IActionResult> CourseAsset(long id,int index)
    {var course=await _video.CourseAsync(UserId,id);if(course==null)return NotFound();var sources=VideoHelper.LocalSources(course.Description);return index>=0&&index<sources.Count?await StreamVideo(course.InstructorId, sources[index]):NotFound();}
}

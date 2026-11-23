using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using Microsoft.EntityFrameworkCore;
namespace EduFlow.Web.Services;

public class MediaOwnershipService
{
    private readonly EduFlowDbContext _db;
    private readonly MediaLibraryService _media;
    private readonly IWebHostEnvironment _env;
    public MediaOwnershipService(EduFlowDbContext db, MediaLibraryService media, IWebHostEnvironment env)
    { _db = db; _media = media; _env = env; }

    public async Task<bool> CanUseAsync(long ownerId, string? url, string? uploadedVideo = null)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        if (VideoHelper.YouTubeEmbed(url) != null) return true;
        if (_media.Owns(ownerId, url, MediaLibraryService.Video)) return true;
        const string prefix = "/uploads/videos/";
        if (!url.StartsWith(prefix, StringComparison.Ordinal)) return false;
        var relative = url[prefix.Length..];
        if (relative.Contains('\\') || relative.Contains(':') || relative.Contains('?') || relative.Contains('#')) return false;
        var parts = relative.Split('/');
        if (parts.Length == 2 && parts[0] != ownerId.ToString()) return false;
        if (parts.Length != 1 && parts.Length != 2) return false;
        var name = parts[^1];
        var ext = Path.GetExtension(name).ToLowerInvariant();
        if (!FileUpload.VideoExtensions.Contains(ext) || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "N", out _) || name != Path.GetFileName(name)) return false;
        var exists = File.Exists(Path.Combine(_env.ContentRootPath, "App_Data", "uploads", "videos", relative))
            || File.Exists(Path.Combine(_env.WebRootPath, "uploads", "videos", relative));
        if (!exists) return false;
        // Video mới có thư mục theo chủ khóa học. Tham số uploadedVideo không bind từ form.
        if (parts.Length == 2) return true;
        if (url == uploadedVideo) return true;
        // Video cũ chưa có thư mục chủ sở hữu: chỉ nhận khi không bị gắn sang khóa của người khác.
        if (await _db.Lessons.AnyAsync(l => l.VideoUrl == url && l.Section!.Course!.InstructorId != ownerId)) return false;
        return await _db.Lessons.AnyAsync(l => l.VideoUrl == url && l.Section!.Course!.InstructorId == ownerId && l.Section.Course.DeletedAt == null);
    }
    public async Task<bool> CanUseHtmlAsync(long ownerId, string? html)
    {
        foreach (var url in VideoHelper.LocalSources(html))
            if (!await CanUseAsync(ownerId, url)) return false;
        return true;
    }
}

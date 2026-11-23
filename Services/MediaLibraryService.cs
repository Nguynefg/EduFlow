using System.Text.RegularExpressions;
using EduFlow.Web.Helpers;

namespace EduFlow.Web.Services;

// Một file trong thư viện media
public record MediaItem(string Name, string DisplayName, string Url, string Kind, long Size, DateTime UploadedAt)
{
    public string PreviewUrl => Kind=="video"?"/Media/Video?url="+Uri.EscapeDataString(Url):Url;
}

// Thư viện ảnh / video của giảng viên.
// Lưu thẳng trên ổ đĩa: wwwroot/uploads/media/{userId}/ — mỗi giảng viên một thư mục riêng, không cần bảng CSDL.
// Tên file: {mã ngẫu nhiên 8 ký tự}-{tên gốc đã làm sạch}.{đuôi}
public class MediaLibraryService
{
    public const string Image = "image";
    public const string Video = "video";
    public const long MaxBytesPerUser = 2L * 1024 * 1024 * 1024;   // 2 GB mỗi giảng viên

    private readonly IWebHostEnvironment _env;

    public MediaLibraryService(IWebHostEnvironment env)
    {
        _env = env;
    }

    private string Folder(long userId) => Path.Combine(_env.WebRootPath, "uploads", "media", userId.ToString());
    private string PrivateFolder(long userId)=>Path.Combine(_env.ContentRootPath,"App_Data","uploads","media",userId.ToString());
    private static string BaseUrl(long userId) => $"/uploads/media/{userId}/";

    public static string? KindOf(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (FileUpload.ImageExtensions.Contains(ext) || ext == ".gif") return Image;
        if (FileUpload.VideoExtensions.Contains(ext)) return Video;
        return null;
    }

    public List<MediaItem> List(long userId, string? kind = null)
    {
        return new[]{Folder(userId),PrivateFolder(userId)}.Where(Directory.Exists).SelectMany(Directory.EnumerateFiles).Select(f=>ToItem(userId,new FileInfo(f))).Where(i=>i!=null&&(kind==null||i.Kind==kind)).Select(i=>i!).OrderByDescending(i=>i.UploadedAt).ToList();
    }
    public long UsedBytes(long userId)=>new[]{Folder(userId),PrivateFolder(userId)}.Where(Directory.Exists).SelectMany(Directory.EnumerateFiles).Sum(f=>new FileInfo(f).Length);
    public string? VideoPath(long userId,string url)=>Owns(userId,url,Video)?SafePath(userId,url[BaseUrl(userId).Length..]):null;

    // Lưu file mới. Trả về (file, null) hoặc (null, lỗi)
    public async Task<(MediaItem? Item, string? Error)> UploadAsync(long userId, IFormFile file)
    {
        var kind = KindOf(file.FileName);
        if (kind == null) return (null, $"{file.FileName}: chỉ nhận ảnh JPG, PNG, WebP, GIF hoặc video MP4, WebM.");
        var max = kind == Image ? FileUpload.MaxImageBytes : FileUpload.MaxVideoBytes;
        if (file.Length == 0 || file.Length > max)
            return (null, $"{file.FileName}: {(kind == Image ? "ảnh tối đa 5 MB" : "video tối đa 500 MB")}.");
        if (UsedBytes(userId) + file.Length > MaxBytesPerUser)
            return (null, "Thư viện đã đầy (tối đa 2 GB). Hãy xóa bớt file không dùng.");

        var dir = kind==Video?PrivateFolder(userId):Folder(userId);
        Directory.CreateDirectory(dir);
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var name = Guid.NewGuid().ToString("N")[..8] + "-" + Slug(Path.GetFileNameWithoutExtension(file.FileName)) + ext;
        var path = Path.Combine(dir, name);
        await using (var stream = new FileStream(path, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }
        return (ToItem(userId, new FileInfo(path)), null);
    }

    public bool Delete(long userId, string name)
    {
        var path = SafePath(userId, name);
        if (path == null || !File.Exists(path)) return false;
        File.Delete(path);
        return true;
    }

    // Link có thuộc thư viện của giảng viên này và file còn tồn tại không (chống chọn file của người khác)
    public bool Owns(long userId, string? url, string? kind = null)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith(BaseUrl(userId))) return false;
        var path = SafePath(userId, url[BaseUrl(userId).Length..]);
        return path != null && File.Exists(path) && (kind == null || KindOf(path) == kind);
    }

    // Chỉ chấp nhận tên file nằm ngay trong thư mục của người dùng (chặn ../)
    private string? SafePath(long userId, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name)) return null;
        var privatePath=Path.Combine(PrivateFolder(userId),name);
        return File.Exists(privatePath)?privatePath:Path.Combine(Folder(userId),name);
    }

    private static MediaItem? ToItem(long userId, FileInfo f)
    {
        var kind = KindOf(f.Name);
        if (kind == null) return null;
        var display = Regex.Replace(f.Name, "^[0-9a-f]{8}-", "");
        return new MediaItem(f.Name, display, BaseUrl(userId) + f.Name, kind, f.Length, f.CreationTime);
    }

    // Tên file an toàn cho URL: bỏ dấu tiếng Việt, chỉ giữ chữ, số, gạch nối
    private static string Slug(string s)
    {
        var normalized = s.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark);
        var ascii = new string(chars.ToArray()).Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant();
        ascii = Regex.Replace(ascii, "[^a-z0-9]+", "-").Trim('-');
        if (ascii.Length > 60) ascii = ascii[..60].Trim('-');
        return ascii.Length == 0 ? "file" : ascii;
    }
}

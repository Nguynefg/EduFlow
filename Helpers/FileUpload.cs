namespace EduFlow.Web.Helpers;

// Lưu file upload vào wwwroot/uploads/<thư mục>
public static class FileUpload
{
    public static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    public static readonly string[] VideoExtensions = { ".mp4", ".webm" };
    public static readonly string[] DocumentExtensions = { ".pdf", ".zip", ".rar", ".docx", ".pptx", ".txt" };

    public const long MaxImageBytes = 5 * 1024 * 1024;        // 5 MB
    public const long MaxVideoBytes = 500L * 1024 * 1024;     // 500 MB
    public const long MaxDocumentBytes = 50 * 1024 * 1024;    // 50 MB

    // Trả về đường dẫn web (/uploads/...), hoặc null nếu sai đuôi file / quá dung lượng
    public static async Task<string?> SaveAsync(IFormFile file, string webRootPath, string folder,
        string[] allowedExtensions, long maxBytes)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension) || file.Length == 0 || file.Length > maxBytes)
            return null;

        var directory = Path.Combine(webRootPath, "uploads", folder);
        Directory.CreateDirectory(directory);
        var fileName = Guid.NewGuid().ToString("N") + extension;
        using (var stream = new FileStream(Path.Combine(directory, fileName), FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return "/uploads/" + folder + "/" + fileName;
    }

    public static Task<string?> SaveImageAsync(IFormFile file, string webRootPath, string folder)
        => SaveAsync(file, webRootPath, folder, ImageExtensions, MaxImageBytes);

    // Xóa file đã upload (chỉ xóa file nằm trong /uploads/)
    public static void Delete(string? url, string webRootPath)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith("/uploads/")) return;
        var path = Path.Combine(webRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        var boundary = Path.GetFullPath(Path.Combine(webRootPath, "uploads")) + Path.DirectorySeparatorChar;
        if (!Path.GetFullPath(path).StartsWith(boundary, StringComparison.OrdinalIgnoreCase)) return;
        if (File.Exists(path)) File.Delete(path);
    }
}

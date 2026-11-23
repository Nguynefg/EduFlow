using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;

// Thư viện ảnh / video của giảng viên: trang quản lý + API cho hộp chọn file trong trình soạn thảo
[Authorize(Roles = AppRoles.Instructor + "," + AppRoles.Admin)]
public class MediaController : Controller
{
    private readonly MediaLibraryService _media;

    public MediaController(MediaLibraryService media)
    {
        _media = media;
    }

    private long UserId => AuthHelper.GetUserId(User);

    // ===== TRANG THƯ VIỆN =====
    public IActionResult Index(string? kind)
    {
        if (kind is not (MediaLibraryService.Image or MediaLibraryService.Video)) kind = null;
        ViewBag.Kind = kind;
        ViewBag.UsedBytes = _media.UsedBytes(UserId);
        return View(_media.List(UserId, kind));
    }

    // Danh sách file dạng JSON (hộp chọn file)
    [HttpGet]
    public IActionResult List(string? kind)
    {
        if (kind is not (MediaLibraryService.Image or MediaLibraryService.Video)) kind = null;
        return Json(_media.List(UserId, kind));
    }

    [ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
    public IActionResult Video(string url)
    {
        if(string.IsNullOrWhiteSpace(url))return NotFound();
        var owner=UserId;
        if(User.IsInRole(AppRoles.Admin))
        {
            var match=System.Text.RegularExpressions.Regex.Match(url,@"^/uploads/media/(\d+)/");
            if(match.Success&&long.TryParse(match.Groups[1].Value,out var id))owner=id;
        }
        var path=_media.VideoPath(owner,url);return path==null?NotFound():PhysicalFile(path,Path.GetExtension(path)==".webm"?"video/webm":"video/mp4",enableRangeProcessing:true);
    }

    // Tải lên một hoặc nhiều file. Gọi bằng JavaScript thì trả JSON, gửi form thường thì quay lại trang.
    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(600_000_000), RequestFormLimits(MultipartBodyLengthLimit = 600_000_000)]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        var saved = new List<MediaItem>();
        var errors = new List<string>();
        foreach (var file in files)
        {
            var (item, error) = await _media.UploadAsync(UserId, file);
            if (item != null) saved.Add(item);
            else errors.Add(error!);
        }
        if (files.Count == 0) errors.Add("Chưa chọn file nào.");

        if (Request.Headers["X-Requested-With"] == "fetch")
            return Json(new { items = saved, errors });

        if (saved.Count > 0) TempData["Success"] = $"Đã tải lên {saved.Count} file.";
        if (errors.Count > 0) TempData["Error"] = string.Join(" ", errors);
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(string name, string? kind)
    {
        if (_media.Delete(UserId, name)) TempData["Success"] = "Đã xóa file.";
        else TempData["Error"] = "Không tìm thấy file.";
        return RedirectToAction("Index", new { kind });
    }
}

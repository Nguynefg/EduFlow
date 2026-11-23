using System.Text.Json;
using EduFlow.Web.Helpers;
using EduFlow.Web.Models.ViewModels;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;

// Học viên: khóa học đã mua + trang học bài
[Authorize, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class LearningController : Controller
{
    private readonly CommunityService _community;
    private readonly LearningService _learningService;
    private readonly VideoWatchService _videoWatch;

    public LearningController(LearningService learningService, VideoWatchService videoWatch, CommunityService community)
    {
        _learningService = learningService;
        _videoWatch = videoWatch;
        _community = community;
    }

    private long UserId => AuthHelper.GetUserId(User);

    // Danh sách khóa học đã mua
    public async Task<IActionResult> Index(string? filter, string? q)
    {
        return View(await _learningService.GetMyCoursesAsync(UserId, filter, q));
    }

    // Trang học: /Learning/Learn/{courseId}?lessonId=...
    public async Task<IActionResult> Learn(long id, long? lessonId, string? q, int qaPage = 1, int? seek = null)
    {
        var model = await _learningService.GetLearnAsync(UserId, id, lessonId);
        if (model == null)
        {
            TempData["Error"] = "Bạn cần đăng ký khóa học này trước khi vào học.";
            return RedirectToAction("Details", "Courses", new { id });
        }
        if (model.Lessons.Count == 0)
        {
            TempData["Error"] = "Khóa học chưa có bài học nào.";
            return RedirectToAction("Details", "Courses", new { id });
        }
        // Bài bị khóa: quay về bài kiểm tra đang chặn
        if (model.LockedLessonIds.Contains(model.Lesson.Id))
        {
            var blocking = model.Lessons.TakeWhile(l => l.Id != model.Lesson.Id)
                .FirstOrDefault(l => !model.IsCompleted(l.Id));
            TempData["Error"] = "Bạn cần hoàn thành bài trước để mở khóa bài này.";
            return RedirectToAction("Learn", new { id, lessonId = blocking?.Id });
        }

        if (TempData["QuizResult"] is string json)
        {
            var result = JsonSerializer.Deserialize<QuizResultViewModel>(json);
            if (result?.LessonId == model.Lesson.Id) model.QuizResult = result;
        }
        if (model.Lesson.LessonType == "video" && seek.HasValue && seek >= 0 && seek <= model.Lesson.DurationSec) model.StartAt = seek;
        model.Discussions = await _community.DiscussionsAsync(UserId, model.Lesson.Id, q, qaPage);
        if (model.Discussions != null) model.Discussions.InLearn = true;
        return View(model);
    }

    // Đánh dấu hoàn thành bài (video / bài viết). Gọi bằng form hoặc fetch.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(long id, long lessonId, long? nextLessonId)
    {
        var percent = await _learningService.CompleteLessonAsync(UserId, lessonId);
        if (Request.Headers["X-Requested-With"] == "fetch")
            return percent == null ? BadRequest() : Json(new { percent });

        if (percent == null) TempData["Error"] = "Không thể đánh dấu hoàn thành bài này.";
        return RedirectToAction("Learn", new { id, lessonId = percent != null && nextLessonId != null ? nextLessonId : lessonId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> StartVideo(long lessonId, int position) {
        var id = await _videoWatch.StartAsync(UserId, lessonId, position);
        return id == null ? BadRequest("Không thể theo dõi video. Kiểm tra quyền học và thời lượng video.") : Json(new { session = id });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Position(Guid session, int position, bool playing) {
        var result = await _videoWatch.HeartbeatAsync(UserId, session, position, playing);
        return result == null ? Conflict(new { code = "session_changed", message = "Phiên theo dõi đã thay đổi, đang kết nối lại." }) : Json(result);
    }

    // Nộp bài kiểm tra
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz(long id, long lessonId, Dictionary<long, string?> answers)
    {
        var result = await _learningService.SubmitQuizAsync(UserId, lessonId, answers ?? new());
        if (result == null)
            TempData["Error"] = "Không thể nộp bài kiểm tra này.";
        else
            TempData["QuizResult"] = JsonSerializer.Serialize(result);
        return RedirectToAction("Learn", new { id, lessonId });
    }
}

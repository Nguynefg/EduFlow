using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;

// Giảng viên quản lý nội dung khóa học: chương, bài học, tài liệu, câu hỏi quiz
[Authorize(Roles = AppRoles.Instructor + "," + AppRoles.Admin)]
public class CurriculumController : Controller
{
    private readonly CurriculumService _curriculumService;
    private readonly IWebHostEnvironment _env;

    public CurriculumController(CurriculumService curriculumService, IWebHostEnvironment env)
    {
        _curriculumService = curriculumService;
        _env = env;
    }

    private long InstructorId => AuthHelper.GetUserId(User);

    // ===== TRANG QUẢN LÝ NỘI DUNG =====
    public async Task<IActionResult> Index(long id)
    {
        var course = await _curriculumService.GetCourseContentAsync(id, InstructorId);
        if (course == null) return NotFound();
        return View("~/Views/Curriculum/Index.cshtml", course);
    }

    // ===== CHƯƠNG =====
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSection(long id, string title)
    {
        ShowError(await _curriculumService.AddSectionAsync(id, InstructorId, title));
        return RedirectToAction("Index", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameSection(long id, long sectionId, string title)
    {
        ShowError(await _curriculumService.RenameSectionAsync(sectionId, InstructorId, title));
        return RedirectToAction("Index", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSection(long id, long sectionId)
    {
        ShowError(await _curriculumService.DeleteSectionAsync(sectionId, InstructorId));
        return RedirectToAction("Index", new { id });
    }

    // Lưu thứ tự sau khi kéo - thả (gọi bằng JavaScript). Có sectionId = sắp xếp bài học, không có = sắp xếp chương.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder(long id, long? sectionId, List<long> ids)
    {
        if (sectionId == null)
            await _curriculumService.ReorderSectionsAsync(id, InstructorId, ids);
        else
            await _curriculumService.ReorderLessonsAsync(sectionId.Value, InstructorId, ids);
        return NoContent();
    }

    // ===== BÀI HỌC =====
    [HttpGet]
    public async Task<IActionResult> AddLesson(long id, long sectionId)
    {
        return await ShowLessonForm(new LessonFormViewModel { CourseId = id, SectionId = sectionId });
    }

    [HttpGet]
    public async Task<IActionResult> EditLesson(long id, long lessonId)
    {
        var lesson = await _curriculumService.GetLessonAsync(lessonId, InstructorId);
        if (lesson == null) return NotFound();

        var model = new LessonFormViewModel
        {
            Id = lesson.Id,
            CourseId = id,
            SectionId = lesson.SectionId,
            Title = lesson.Title,
            LessonType = lesson.LessonType,
            VideoUrl = lesson.VideoUrl,
            CurrentVideoUrl = lesson.VideoUrl,
            DurationSeconds = lesson.DurationSec,
            Content = lesson.Content,
            PassScore = lesson.PassScore,
            IsFreePreview = lesson.IsFreePreview,
            CurrentResources = lesson.Resources
        };
        return await ShowLessonForm(model);
    }

    // Lưu bài học (thêm mới hoặc sửa). Cho phép upload file lớn (video tối đa 500 MB).
    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(600_000_000), RequestFormLimits(MultipartBodyLengthLimit = 600_000_000)]
    public async Task<IActionResult> SaveLesson(LessonFormViewModel model)
    {
        var course = await _curriculumService.GetCourseContentAsync(model.CourseId, InstructorId);
        if (course == null) return NotFound();
        // Upload video (nếu có)
        string? videoUrl = null;
        if (model.LessonType == LessonType.Video && model.VideoFile != null)
        {
            videoUrl = await FileUpload.SaveAsync(model.VideoFile, Path.Combine(_env.ContentRootPath,"App_Data"), $"videos/{course.InstructorId}",
                FileUpload.VideoExtensions, FileUpload.MaxVideoBytes);
            if (videoUrl == null)
                ModelState.AddModelError("VideoFile", "Chỉ nhận video .mp4 hoặc .webm, tối đa 500 MB.");
        }

        // Upload tài liệu đính kèm (nếu có)
        var resources = new List<LessonResource>();
        foreach (var file in model.ResourceFiles)
        {
            var url = await FileUpload.SaveAsync(file, Path.Combine(_env.ContentRootPath,"App_Data"), "resources",
                FileUpload.DocumentExtensions, FileUpload.MaxDocumentBytes);
            if (url == null)
                ModelState.AddModelError("ResourceFiles", $"File {file.FileName} không hợp lệ (PDF, ZIP, RAR, DOCX, PPTX, TXT, tối đa 50 MB).");
            else
                resources.Add(new LessonResource { FileName = Path.GetFileName(file.FileName), FileUrl = url, FileSize = file.Length });
        }

        if (ModelState.IsValid)
        {
            var error = await _curriculumService.SaveLessonAsync(InstructorId, model, videoUrl, resources);
            if (error == null)
            {
                TempData["Success"] = "Đã lưu bài học \"" + model.Title + "\".";
                return RedirectToAction("Index", new { id = model.CourseId });
            }
            ModelState.AddModelError("", error);
        }

        // Có lỗi: xóa file vừa upload để không bị rác, rồi hiện lại form
        FileUpload.Delete(videoUrl, Path.Combine(_env.ContentRootPath,"App_Data"));
        foreach (var r in resources) FileUpload.Delete(r.FileUrl, Path.Combine(_env.ContentRootPath,"App_Data"));
        if (model.Id != null)
        {
            var lesson = await _curriculumService.GetLessonAsync(model.Id.Value, InstructorId);
            model.CurrentResources = lesson?.Resources ?? new List<LessonResource>();
            model.CurrentVideoUrl = lesson?.VideoUrl;
        }
        return await ShowLessonForm(model);
    }

    // Form bài học kèm mục lục khóa học (menu bên trái)
    private async Task<IActionResult> ShowLessonForm(LessonFormViewModel model)
    {
        var course = await _curriculumService.GetCourseContentAsync(model.CourseId, InstructorId);
        if (course == null) return NotFound();
        ViewBag.Course = course;
        return View("~/Views/Curriculum/LessonForm.cshtml", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLesson(long id, long lessonId)
    {
        ShowError(await _curriculumService.DeleteLessonAsync(lessonId, InstructorId));
        return RedirectToAction("Index", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResource(long id, long lessonId, long resourceId)
    {
        var fileUrl = await _curriculumService.DeleteResourceAsync(resourceId, InstructorId);
        FileUpload.Delete(fileUrl, Path.Combine(_env.ContentRootPath,"App_Data"));
        FileUpload.Delete(fileUrl, _env.WebRootPath);
        return RedirectToAction("EditLesson", new { id, lessonId });
    }

    // ===== CÂU HỎI QUIZ =====
    // Gọi bằng JavaScript từ trang Sửa bài kiểm tra. Thành công: trả về HTML danh sách câu hỏi mới.
    // Lỗi: mã 400 kèm thông báo.

    [HttpGet]
    public async Task<IActionResult> Questions(long id, long lessonId) => await QuestionList(lessonId, null);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveQuestion(long id, QuestionFormViewModel model)
        => await QuestionList(model.LessonId, await _curriculumService.SaveQuestionAsync(InstructorId, model));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(long id, long lessonId, long questionId)
        => await QuestionList(lessonId, await _curriculumService.DeleteQuestionAsync(lessonId, questionId, InstructorId));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DuplicateQuestion(long id, long lessonId, long questionId)
        => await QuestionList(lessonId, await _curriculumService.DuplicateQuestionAsync(lessonId, questionId, InstructorId));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReorderQuestions(long id, long lessonId, List<long> ids)
        => await QuestionList(lessonId, await _curriculumService.ReorderQuestionsAsync(lessonId, InstructorId, ids));

    private async Task<IActionResult> QuestionList(long lessonId, string? error)
    {
        if (error != null) return BadRequest(error);
        var quiz = await _curriculumService.GetQuizAsync(lessonId, InstructorId);
        if (quiz == null) return NotFound("Không tìm thấy bài kiểm tra.");
        return PartialView("~/Views/Curriculum/_QuizQuestions.cshtml", quiz);
    }

    private void ShowError(string? error)
    {
        if (error != null) TempData["Error"] = error;
    }
}

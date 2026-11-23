using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using EduFlow.Web.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Services;

// Giảng viên quản lý nội dung khóa học: chương, bài học, tài liệu, câu hỏi quiz
public class CurriculumService
{
    private readonly EduFlowDbContext _db;
    private readonly MediaOwnershipService _ownership;

    public CurriculumService(EduFlowDbContext db, MediaOwnershipService ownership)
    {
        _db = db;
        _ownership = ownership;
    }

    // Quyền quản trị lấy từ CSDL, không nhận từ form gửi lên.
    private Task<bool> IsAdminAsync(long userId)
    {
        return _db.UserRoles.AnyAsync(role => role.UserId == userId
            && role.Role!.Code == AppRoles.Admin && role.User!.IsActive && role.User.DeletedAt == null);
    }

    // Lấy khóa học kèm toàn bộ nội dung (chỉ của giảng viên đó)
    public async Task<Course?> GetCourseContentAsync(long courseId, long instructorId)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        return await _db.Courses
            .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                    .ThenInclude(l => l.Questions.OrderBy(q => q.OrderIndex))
                        .ThenInclude(q => q.Options)
            .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Resources)
            .FirstOrDefaultAsync(c => c.Id == courseId && (c.InstructorId == instructorId || isAdmin) && c.DeletedAt == null);
    }

    // ================= CHƯƠNG =================

    public async Task<string?> AddSectionAsync(long courseId, long instructorId, string title)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        var course = await _db.Courses.Include(c => c.Sections)
            .FirstOrDefaultAsync(c => c.Id == courseId && (c.InstructorId == instructorId || isAdmin) && c.DeletedAt == null);
        if (course == null) return "Không tìm thấy khóa học.";
        if (string.IsNullOrWhiteSpace(title)) return "Nhập tên chương.";

        course.Sections.Add(new Section
        {
            Title = title.Trim(),
            OrderIndex = course.Sections.Count + 1
        });
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> RenameSectionAsync(long sectionId, long instructorId, string title)
    {
        var section = await FindSectionAsync(sectionId, instructorId);
        if (section == null) return "Không tìm thấy chương.";
        if (string.IsNullOrWhiteSpace(title)) return "Nhập tên chương.";
        section.Title = title.Trim();
        await _db.SaveChangesAsync();
        return null;
    }

    // Xóa chương sẽ xóa luôn các bài học bên trong (ON DELETE CASCADE)
    public async Task<string?> DeleteSectionAsync(long sectionId, long instructorId)
    {
        var section = await FindSectionAsync(sectionId, instructorId);
        if (section == null) return "Không tìm thấy chương.";
        _db.Sections.Remove(section);
        await _db.SaveChangesAsync();
        return null;
    }

    // Lưu thứ tự chương sau khi kéo - thả. ids = danh sách id theo thứ tự mới.
    public async Task ReorderSectionsAsync(long courseId, long instructorId, List<long> ids)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        var sections = await _db.Sections
            .Where(s => s.CourseId == courseId && (s.Course!.InstructorId == instructorId || isAdmin))
            .ToListAsync();
        foreach (var section in sections)
            section.OrderIndex = ids.IndexOf(section.Id) + 1;
        await _db.SaveChangesAsync();
    }

    public async Task ReorderLessonsAsync(long sectionId, long instructorId, List<long> ids)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        var lessons = await _db.Lessons
            .Where(l => l.SectionId == sectionId && (l.Section!.Course!.InstructorId == instructorId || isAdmin))
            .ToListAsync();
        foreach (var lesson in lessons)
            lesson.OrderIndex = ids.IndexOf(lesson.Id) + 1;
        await _db.SaveChangesAsync();
    }

    private async Task<Section?> FindSectionAsync(long sectionId, long instructorId)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        return await _db.Sections.Include(s => s.Lessons)
            .FirstOrDefaultAsync(s => s.Id == sectionId && (s.Course!.InstructorId == instructorId || isAdmin) && s.Course.DeletedAt == null);
    }

    // ================= BÀI HỌC =================

    // Lấy bài học (kèm tài liệu) để sửa
    public async Task<Lesson?> GetLessonAsync(long lessonId, long instructorId)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        return await _db.Lessons
            .Include(l => l.Resources)
            .Include(l => l.Section)
            .FirstOrDefaultAsync(l => l.Id == lessonId && (l.Section!.Course!.InstructorId == instructorId || isAdmin));
    }

    // Thêm mới (model.Id = null) hoặc sửa bài học.
    // uploadedVideoUrl: link video vừa upload (null nếu không upload).
    // newResources: các tài liệu vừa upload.
    public async Task<string?> SaveLessonAsync(long instructorId, LessonFormViewModel model,
        string? uploadedVideoUrl, List<LessonResource> newResources)
    {
        if (model.LessonType != LessonType.Video && model.LessonType != LessonType.Article && model.LessonType != LessonType.Quiz)
            return "Loại bài không hợp lệ.";

        Lesson? lesson;
        if (model.Id == null)
        {
            var section = await FindSectionAsync(model.SectionId, instructorId);
            if (section == null || section.CourseId != model.CourseId) return "Không tìm thấy chương.";
            lesson = new Lesson { SectionId = section.Id, OrderIndex = section.Lessons.Count + 1 };
            _db.Lessons.Add(lesson);
        }
        else
        {
            lesson = await GetLessonAsync(model.Id.Value, instructorId);
            if (lesson == null) return "Không tìm thấy bài học.";
            // Chặn ghi nhầm: bài học phải thuộc đúng khóa học và chương đang sửa
            if (lesson.Section!.CourseId != model.CourseId || lesson.SectionId != model.SectionId)
                return "Bài học không thuộc chương này. Hãy mở lại trang và thử lại.";
        }

        var ownerId = await _db.Courses.Where(c => c.Id == model.CourseId).Select(c => c.InstructorId).FirstAsync();
        var candidateVideo = uploadedVideoUrl ?? (string.IsNullOrWhiteSpace(model.VideoUrl) ? lesson.VideoUrl : model.VideoUrl.Trim());
        if (model.LessonType == LessonType.Video && !await _ownership.CanUseAsync(ownerId, candidateVideo, uploadedVideoUrl)
            || model.LessonType == LessonType.Article && !await _ownership.CanUseHtmlAsync(ownerId, model.Content))
            return "Video phải thuộc thư viện của chủ khóa học hoặc được tải trực tiếp lên bài học này.";
        lesson.Title = model.Title.Trim();
        lesson.LessonType = model.LessonType;
        lesson.IsFreePreview = model.IsFreePreview;
        lesson.DurationSec = model.DurationSeconds;

        // Chỉ giữ dữ liệu đúng với loại bài
        if (model.LessonType == LessonType.Video)
        {
            if (model.DurationSeconds <= 0) return "Cần thời lượng video lớn hơn 0 để theo dõi tiến độ.";
            var videoUrl = uploadedVideoUrl ?? (string.IsNullOrWhiteSpace(model.VideoUrl) ? lesson.VideoUrl : model.VideoUrl.Trim());
            if (string.IsNullOrEmpty(videoUrl)) return "Nhập link video hoặc tải video lên.";
            if (VideoHelper.YouTubeEmbed(videoUrl)==null && !videoUrl.StartsWith("/uploads/",StringComparison.Ordinal))
                return "Tải video MP4/WebM lên hoặc dùng YouTube; không hỗ trợ link MP4 bên ngoài không được bảo vệ.";
            lesson.VideoUrl = videoUrl;
            lesson.Content = null;
            lesson.PassScore = null;
        }
        else if (model.LessonType == LessonType.Article)
        {
            if (string.IsNullOrWhiteSpace(model.Content)) return "Nhập nội dung bài viết.";
            // Lọc bỏ mã độc (script, onclick...) trong HTML trước khi lưu
            lesson.Content = RichText.Sanitize(model.Content);
            lesson.VideoUrl = null;
            lesson.PassScore = null;
        }
        else
        {
            lesson.PassScore = model.PassScore ?? 70;
            lesson.VideoUrl = null;
            lesson.Content = null;
            lesson.DurationSec = 0;
        }

        // Câu hỏi soạn trước khi bài kiểm tra được lưu
        if (model.LessonType == LessonType.Quiz && !string.IsNullOrWhiteSpace(model.DraftQuestionsJson))
        {
            List<QuestionFormViewModel>? drafts;
            try
            {
                drafts = System.Text.Json.JsonSerializer.Deserialize<List<QuestionFormViewModel>>(model.DraftQuestionsJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (System.Text.Json.JsonException) { return "Dữ liệu câu hỏi không hợp lệ. Hãy tải lại trang và thử lại."; }

            var order = lesson.Id == 0 ? 0 : await _db.QuizQuestions.CountAsync(q => q.LessonId == lesson.Id);
            var n = 0;
            foreach (var draft in drafts ?? new())
            {
                n++;
                var error = BuildQuestion(draft, out var question);
                if (error != null) return $"Câu hỏi {n}: {error}";
                question!.OrderIndex = ++order;
                lesson.Questions.Add(question);
            }
        }

        foreach (var resource in newResources)
            lesson.Resources.Add(resource);

        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> DeleteLessonAsync(long lessonId, long instructorId)
    {
        var lesson = await GetLessonAsync(lessonId, instructorId);
        if (lesson == null) return "Không tìm thấy bài học.";
        _db.Lessons.Remove(lesson);
        await _db.SaveChangesAsync();
        return null;
    }

    // Xóa tài liệu đính kèm, trả về link file để controller xóa file trên ổ đĩa
    public async Task<string?> DeleteResourceAsync(long resourceId, long instructorId)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        var resource = await _db.LessonResources
            .FirstOrDefaultAsync(r => r.Id == resourceId && (r.Lesson!.Section!.Course!.InstructorId == instructorId || isAdmin));
        if (resource == null) return null;
        _db.LessonResources.Remove(resource);
        await _db.SaveChangesAsync();
        return resource.FileUrl;
    }

    // ================= CÂU HỎI QUIZ =================
    public const int MaxOptions = 6;

    // Bài kiểm tra kèm câu hỏi + lựa chọn (chỉ của giảng viên đó)
    public async Task<Lesson?> GetQuizAsync(long lessonId, long instructorId)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        return await _db.Lessons
            .Include(l => l.Questions.OrderBy(q => q.OrderIndex)).ThenInclude(q => q.Options.OrderBy(o => o.Id))
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.LessonType == LessonType.Quiz
                                      && (l.Section!.Course!.InstructorId == instructorId || isAdmin) && l.Section.Course.DeletedAt == null);
    }

    // Thêm (model.Id = null) hoặc sửa câu hỏi.
    //  - Trắc nghiệm: 2–6 lựa chọn, đúng 1 đáp án đúng (CorrectIndex)
    //  - Điền khuyết: 1–6 đáp án được chấp nhận, tất cả đều đúng
    public async Task<string?> SaveQuestionAsync(long instructorId, QuestionFormViewModel model)
    {
        var lesson = await GetQuizAsync(model.LessonId, instructorId);
        if (lesson == null) return "Không tìm thấy bài kiểm tra.";
        var error = BuildQuestion(model, out var built);
        if (error != null) return error;

        QuizQuestion? question;
        if (model.Id == null)
        {
            question = new QuizQuestion { OrderIndex = lesson.Questions.Count + 1 };
            lesson.Questions.Add(question);
        }
        else
        {
            question = lesson.Questions.FirstOrDefault(q => q.Id == model.Id);
            if (question == null) return "Không tìm thấy câu hỏi.";
            _db.QuizOptions.RemoveRange(question.Options);
            question.Options.Clear();
        }

        question.QuestionText = built!.QuestionText;
        question.QuestionType = built.QuestionType;
        question.Options.AddRange(built.Options);
        await _db.SaveChangesAsync();
        return null;
    }

    // Kiểm tra dữ liệu câu hỏi và dựng QuizQuestion (chưa gắn vào bài học). Trả về thông báo lỗi nếu không hợp lệ.
    private static string? BuildQuestion(QuestionFormViewModel model, out QuizQuestion? question)
    {
        question = null;
        if (string.IsNullOrWhiteSpace(model.QuestionText)) return "Nhập nội dung câu hỏi.";
        if (model.QuestionText.Length > 2000) return "Câu hỏi tối đa 2000 ký tự.";

        var type = model.QuestionType == "fill" ? "fill" : "single";
        // Giữ vị trí gốc để biết đáp án đúng sau khi bỏ dòng trống
        var rows = (model.Options ?? new()).Select((text, index) => (Text: text?.Trim(), Index: index))
                                .Where(o => !string.IsNullOrEmpty(o.Text)).ToList();
        if (rows.Count > MaxOptions) return $"Tối đa {MaxOptions} lựa chọn.";
        if (rows.Any(o => o.Text!.Length > 500)) return "Mỗi lựa chọn tối đa 500 ký tự.";
        if (rows.Select(o => o.Text!.ToLower()).Distinct().Count() != rows.Count) return "Các lựa chọn không được trùng nhau.";

        List<QuizOption> options;
        if (type == "fill")
        {
            if (rows.Count == 0) return "Nhập ít nhất 1 đáp án đúng.";
            options = rows.Select(o => new QuizOption { OptionText = o.Text!, IsCorrect = true }).ToList();
        }
        else
        {
            if (rows.Count < 2) return "Câu trắc nghiệm cần ít nhất 2 lựa chọn.";
            if (!rows.Any(o => o.Index == model.CorrectIndex)) return "Chọn đáp án đúng là một lựa chọn đã nhập.";
            options = rows.Select(o => new QuizOption { OptionText = o.Text!, IsCorrect = o.Index == model.CorrectIndex }).ToList();
        }

        question = new QuizQuestion { QuestionText = model.QuestionText.Trim(), QuestionType = type, Options = options };
        return null;
    }

    public async Task<string?> DeleteQuestionAsync(long lessonId, long questionId, long instructorId)
    {
        var lesson = await GetQuizAsync(lessonId, instructorId);
        var question = lesson?.Questions.FirstOrDefault(q => q.Id == questionId);
        if (lesson == null || question == null) return "Không tìm thấy câu hỏi.";
        _db.QuizQuestions.Remove(question);
        // Đánh lại số thứ tự cho liền mạch
        var order = 1;
        foreach (var q in lesson.Questions.Where(q => q.Id != questionId)) q.OrderIndex = order++;
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> DuplicateQuestionAsync(long lessonId, long questionId, long instructorId)
    {
        var lesson = await GetQuizAsync(lessonId, instructorId);
        var source = lesson?.Questions.FirstOrDefault(q => q.Id == questionId);
        if (lesson == null || source == null) return "Không tìm thấy câu hỏi.";
        // Chèn bản sao ngay sau câu gốc
        foreach (var q in lesson.Questions.Where(q => q.OrderIndex > source.OrderIndex)) q.OrderIndex++;
        lesson.Questions.Add(new QuizQuestion
        {
            QuestionText = source.QuestionText,
            QuestionType = source.QuestionType,
            OrderIndex = source.OrderIndex + 1,
            Options = source.Options.Select(o => new QuizOption { OptionText = o.OptionText, IsCorrect = o.IsCorrect }).ToList()
        });
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> ReorderQuestionsAsync(long lessonId, long instructorId, List<long> ids)
    {
        var lesson = await GetQuizAsync(lessonId, instructorId);
        if (lesson == null) return "Không tìm thấy bài kiểm tra.";
        foreach (var q in lesson.Questions)
        {
            var index = ids.IndexOf(q.Id);
            q.OrderIndex = index < 0 ? ids.Count + q.OrderIndex : index + 1;
        }
        await _db.SaveChangesAsync();
        return null;
    }
}

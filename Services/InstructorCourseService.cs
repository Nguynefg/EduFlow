using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Services;

// Giảng viên quản lý khóa học của mình: tạo, sửa, gửi duyệt, xóa
public class InstructorCourseService
{
    private readonly EduFlowDbContext _db;
    private readonly MediaOwnershipService _ownership;

    public InstructorCourseService(EduFlowDbContext db, MediaOwnershipService ownership)
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

    // Danh sách khóa học của giảng viên
    public async Task<List<Course>> GetMyCoursesAsync(long instructorId)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        return await _db.Courses
            .Include(c => c.Category)
            .Include(c => c.Sections).ThenInclude(s => s.Lessons)
            .Where(c => (c.InstructorId == instructorId || isAdmin) && c.DeletedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // Giảng viên chỉ sửa khóa của mình; admin được quản lý mọi khóa học.
    public async Task<Course?> GetMyCourseAsync(long courseId, long instructorId)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        return await _db.Courses.FirstOrDefaultAsync(c =>
            c.Id == courseId && (c.InstructorId == instructorId || isAdmin) && c.DeletedAt == null);
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
    }

    // Đổ dữ liệu khóa học vào form sửa
    public async Task<CourseFormViewModel?> GetFormAsync(long courseId, long instructorId)
    {
        var c = await GetMyCourseAsync(courseId, instructorId);
        if (c == null) return null;
        return new CourseFormViewModel
        {
            Id = c.Id,
            Title = c.Title,
            CategoryId = c.CategoryId,
            Level = c.Level,
            ShortDesc = c.ShortDesc,
            Description = RichText.ToEditorHtml(c.Description),
            Objectives = c.Objectives,
            Requirements = c.Requirements,
            ThumbnailUrl = c.ThumbnailUrl,
            TrailerUrl = c.TrailerUrl,
            Price = c.Price,
            DiscountPrice = c.DiscountPrice,
            DiscountStart = c.DiscountStart,
            DiscountEnd = c.DiscountEnd,
            Status = c.Status,
            RejectReason = c.RejectReason
        };
    }

    // Kiểm tra dữ liệu chung khi tạo / sửa. Trả về lỗi hoặc null.
    private async Task<string?> CheckFormAsync(CourseFormViewModel model)
    {
        if (!CourseLevel.All.Contains(model.Level)) return "Cấp độ không hợp lệ.";
        if (!await _db.Categories.AnyAsync(c => c.Id == model.CategoryId && c.IsActive)) return "Danh mục không hợp lệ.";
        if (model.DiscountPrice != null && model.DiscountPrice >= model.Price)
            return "Giá khuyến mãi phải nhỏ hơn giá gốc.";
        if (model.DiscountStart != null && model.DiscountEnd != null && model.DiscountEnd <= model.DiscountStart)
            return "Ngày kết thúc khuyến mãi phải sau ngày bắt đầu.";
        return null;
    }

    // ===== TẠO KHÓA HỌC (luôn là bản nháp) =====
    public async Task<(Course? Course, string? Error)> CreateAsync(long instructorId, CourseFormViewModel model, string? thumbnailUrl)
    {
        var error = await CheckFormAsync(model);
        if (error != null) return (null, error);

        if (!await _ownership.CanUseAsync(instructorId, model.TrailerUrl) || !await _ownership.CanUseHtmlAsync(instructorId, model.Description))
            return (null, "Video phải thuộc chủ khóa học.");
        var course = new Course
        {
            InstructorId = instructorId,
            Status = CourseStatus.Draft,
            Slug = await MakeUniqueSlugAsync(model.Title, null),
            ThumbnailUrl = thumbnailUrl
        };
        Apply(course, model);
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        return (course, null);
    }

    // ===== SỬA KHÓA HỌC =====
    public async Task<string?> UpdateAsync(long courseId, long instructorId, CourseFormViewModel model, string? newThumbnailUrl)
    {
        var course = await GetMyCourseAsync(courseId, instructorId);
        if (course == null) return "Không tìm thấy khóa học.";
        var error = await CheckFormAsync(model);
        if (error != null) return error;

        if (!await _ownership.CanUseAsync(course.InstructorId, model.TrailerUrl) || !await _ownership.CanUseHtmlAsync(course.InstructorId, model.Description))
            return "Video phải thuộc chủ khóa học.";
        if (course.Title != model.Title)
            course.Slug = await MakeUniqueSlugAsync(model.Title, course.Id);
        if (newThumbnailUrl != null) course.ThumbnailUrl = newThumbnailUrl;
        Apply(course, model);
        course.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    private static void Apply(Course course, CourseFormViewModel model)
    {
        course.Title = model.Title.Trim();
        course.CategoryId = model.CategoryId;
        course.Level = model.Level;
        course.ShortDesc = model.ShortDesc;
        course.Description = RichText.Sanitize(model.Description);   // lọc mã độc HTML
        course.Objectives = model.Objectives;
        course.Requirements = model.Requirements;
        course.TrailerUrl = model.TrailerUrl;
        course.Price = model.Price;
        course.DiscountPrice = model.DiscountPrice;
        course.DiscountStart = model.DiscountStart;
        course.DiscountEnd = model.DiscountEnd;
    }

    // ===== GỬI DUYỆT =====  Bản nháp hoặc bị từ chối -> chờ duyệt
    public async Task<string?> SubmitForReviewAsync(long courseId, long instructorId)
    {
        var isAdmin = await IsAdminAsync(instructorId);
        var course = await _db.Courses.Include(c => c.Sections).ThenInclude(s => s.Lessons)
            .FirstOrDefaultAsync(c => c.Id == courseId && (c.InstructorId == instructorId || isAdmin) && c.DeletedAt == null);
        if (course == null) return "Không tìm thấy khóa học.";
        if (course.Status != CourseStatus.Draft && course.Status != CourseStatus.Rejected)
            return "Chỉ gửi duyệt được bản nháp hoặc khóa học bị từ chối.";
        if (!course.Sections.Any(s => s.Lessons.Any()))
            return "Khóa học cần ít nhất 1 chương có bài học trước khi gửi duyệt.";

        course.Status = CourseStatus.Pending;
        course.RejectReason = null;
        course.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    // ===== RÚT YÊU CẦU DUYỆT =====  Chờ duyệt -> bản nháp
    public async Task<string?> WithdrawAsync(long courseId, long instructorId)
    {
        var course = await GetMyCourseAsync(courseId, instructorId);
        if (course == null || course.Status != CourseStatus.Pending) return "Khóa học không ở trạng thái chờ duyệt.";
        course.Status = CourseStatus.Draft;
        await _db.SaveChangesAsync();
        return null;
    }

    // ===== XÓA KHÓA HỌC (xóa mềm, chỉ khi chưa xuất bản) =====
    public async Task<string?> DeleteAsync(long courseId, long instructorId)
    {
        var course = await GetMyCourseAsync(courseId, instructorId);
        if (course == null) return "Không tìm thấy khóa học.";
        if (!await IsAdminAsync(instructorId) && (course.Status == CourseStatus.Published || course.PublishedAt != null))
            return "Khóa học đã từng xuất bản nên không thể xóa. Liên hệ quản trị viên nếu cần ẩn.";
        course.DeletedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    // Tạo slug không trùng: lap-trinh-c, lap-trinh-c-2, lap-trinh-c-3...
    private async Task<string> MakeUniqueSlugAsync(string title, long? courseId)
    {
        var baseSlug = SlugHelper.ToSlug(title);
        var slug = baseSlug;
        var i = 2;
        while (await _db.Courses.AnyAsync(c => c.Slug == slug && c.Id != courseId))
        {
            slug = baseSlug + "-" + i;
            i++;
        }
        return slug;
    }
}

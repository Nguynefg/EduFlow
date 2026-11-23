using EduFlow.Web.Areas.Admin.Models;
using EduFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Areas.Admin.Services;

// Admin kiểm duyệt khóa học: duyệt, từ chối, ẩn, hiện lại, xóa
public class AdminCourseService
{
    private readonly EduFlowDbContext _db;

    public AdminCourseService(EduFlowDbContext db)
    {
        _db = db;
    }

    public async Task<int> CountPendingAsync()
    {
        return await _db.Courses.CountAsync(c => c.Status == CourseStatus.Pending && c.DeletedAt == null);
    }

    // Danh sách khóa học theo trạng thái + từ khóa
    public async Task<AdminCoursesViewModel> ListAsync(string? status, string? q)
    {
        var query = _db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Where(c => c.DeletedAt == null);
        if (!string.IsNullOrEmpty(status) && status != "all")
            query = query.Where(c => c.Status == status);
        if (!string.IsNullOrEmpty(q))
            query = query.Where(c => c.Title.Contains(q) || c.Instructor!.FullName.Contains(q));

        return new AdminCoursesViewModel
        {
            Status = status,
            Query = q,
            Courses = await query.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt).ToListAsync(),
            PendingCount = await CountPendingAsync()
        };
    }

    // Xem chi tiết toàn bộ nội dung khóa học
    public async Task<Course?> GetDetailAsync(long id)
    {
        return await _db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                    .ThenInclude(l => l.Questions.OrderBy(q => q.OrderIndex))
                        .ThenInclude(q => q.Options)
            .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Resources)
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
    }

    // Duyệt -> xuất bản
    public async Task<string?> ApproveAsync(long id)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        if (course == null || course.Status != CourseStatus.Pending) return "Khóa học không ở trạng thái chờ duyệt.";
        course.Status = CourseStatus.Published;
        course.RejectReason = null;
        course.PublishedAt ??= DateTime.Now;
        course.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    // Từ chối kèm lý do
    public async Task<string?> RejectAsync(long id, string reason)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        if (course == null || course.Status != CourseStatus.Pending) return "Khóa học không ở trạng thái chờ duyệt.";
        course.Status = CourseStatus.Rejected;
        course.RejectReason = reason;
        course.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    // Ẩn khóa học đang công khai (vi phạm, cần sửa...)
    public async Task<string?> HideAsync(long id, string? reason)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        if (course == null || course.Status != CourseStatus.Published) return "Chỉ ẩn được khóa học đang công khai.";
        course.Status = CourseStatus.Hidden;
        course.RejectReason = reason;
        course.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    // Hiện lại khóa học đã ẩn
    public async Task<string?> UnhideAsync(long id)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        if (course == null || course.Status != CourseStatus.Hidden) return "Khóa học không ở trạng thái ẩn.";
        course.Status = CourseStatus.Published;
        course.RejectReason = null;
        course.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    // Xóa mềm khóa học vi phạm
    public async Task<string?> DeleteAsync(long id)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        if (course == null) return "Không tìm thấy khóa học.";
        course.DeletedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }
}

using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Services;

// Khách / học viên xem khóa học đã xuất bản
public class CourseService
{
    private readonly EduFlowDbContext _db;

    public CourseService(EduFlowDbContext db)
    {
        _db = db;
    }

    // Chỉ lấy khóa học đang công khai, chưa xóa, của giảng viên chưa bị xóa
    private IQueryable<Course> PublishedCourses()
    {
        return _db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Where(c => c.Status == CourseStatus.Published && c.DeletedAt == null && c.Instructor!.DeletedAt == null && c.Instructor.IsActive && c.Category!.IsActive);
    }

    public Task<long?> IdBySlugAsync(string slug)=>PublishedCourses().Where(c=>c.Slug==slug).Select(c=>(long?)c.Id).FirstOrDefaultAsync();
    public Task<int?> CategoryBySlugAsync(string slug)=>_db.Categories.Where(c=>c.Slug==slug&&c.IsActive).Select(c=>(int?)c.Id).FirstOrDefaultAsync();

    // ===== TRANG CHỦ =====
    public async Task<HomePageViewModel> GetHomeAsync()
    {
        // Lấy tất cả khóa học công khai kèm chương/bài để đếm số bài, thời lượng
        var courses = await PublishedCourses()
            .Include(c => c.Sections).ThenInclude(s => s.Lessons)
            .ToListAsync();

        // Danh mục cấp 1: đếm khóa học của chính nó và của danh mục con
        var categories = await _db.Categories.Where(c => c.IsActive && c.ParentId == null).OrderBy(c => c.Name).ToListAsync();
        var cards = new List<CategoryCardViewModel>();
        foreach (var category in categories)
        {
            var inCategory = courses
                .Where(c => c.CategoryId == category.Id || c.Category!.ParentId == category.Id)
                .OrderByDescending(c => c.PublishedAt)
                .ToList();
            cards.Add(new CategoryCardViewModel
            {
                Category = category,
                CourseCount = inCategory.Count,
                LessonCount = inCategory.Sum(c => c.Sections.Sum(s => s.Lessons.Count)),
                Courses = inCategory.Take(2).ToList()
            });
        }

        return new HomePageViewModel
        {
            TotalCourses = courses.Count,
            TotalLessons = courses.Sum(c => c.Sections.Sum(s => s.Lessons.Count)),
            TotalInstructors = courses.Select(c => c.InstructorId).Distinct().Count(),
            TotalStudents = await _db.UserRoles.CountAsync(x => x.Role!.Code == AppRoles.Student && x.User!.DeletedAt == null),
            Featured = courses.OrderByDescending(c => c.AvgRating).ThenByDescending(c => c.PublishedAt).FirstOrDefault(),
            TopCategories = cards.OrderByDescending(c => c.CourseCount).Take(8).ToList(),
            SubjectTabs = cards.Where(c => c.CourseCount > 0).OrderByDescending(c => c.CourseCount).Take(6).ToList(),
            RecommendedCourses = courses.OrderByDescending(c => c.PublishedAt).Take(8).ToList(),
            PopularCourses = courses.OrderByDescending(c => c.AvgRating).ThenBy(c => c.CurrentPrice).Take(8).ToList()
        };
    }

    // ===== DANH SÁCH + LỌC =====
    public async Task<CourseListViewModel> SearchAsync(string? q, int? categoryId, List<string>? levels,
        string? price, string? sort, int page, decimal? minRating = null)
    {
        q=q?.Trim();if(q?.Length>100)q=q[..100];
        var now=DateTime.Now;
        var model = new CourseListViewModel
        {
            MinRating = minRating is >= 1 and <= 5 ? minRating : null,
            Query = q,
            CategoryId = categoryId,
            Levels = (levels ?? new List<string>()).Where(l => CourseLevel.All.Contains(l)).ToList(),
            Price = price is "free" or "paid" ? price : null,
            Sort = sort is "newest" or "rating" or "price-asc" or "price-desc" ? sort : "popular",
            Categories = await GetActiveCategoriesAsync()
        };

        // 1. Lọc theo từ khóa (tên khóa học, mô tả ngắn, tên danh mục) và danh mục
        var query = PublishedCourses();
        if (!string.IsNullOrEmpty(q))
            query = query.Where(c => c.Title.Contains(q)
                || (c.ShortDesc != null && c.ShortDesc.Contains(q))
                || c.Category!.Name.Contains(q)
                || (c.Category.Parent != null && c.Category.Parent.Name.Contains(q)));
        if (categoryId != null)
            query = query.Where(c => c.CategoryId == categoryId || c.Category!.ParentId == categoryId);

        if(model.MinRating.HasValue)query=query.Where(c=>c.AvgRating>=model.MinRating.Value);

        // 2. Đếm số khóa học theo cấp độ và theo miễn phí / trả phí (để hiện bên cạnh ô tick)
        foreach (var level in CourseLevel.All)
            model.LevelCounts[level] = await query.CountAsync(c => c.Level == level);
        model.FreeCount = await query.CountAsync(c => (c.DiscountPrice != null && (c.DiscountStart == null || c.DiscountStart <= now) && (c.DiscountEnd == null || c.DiscountEnd >= now) ? c.DiscountPrice : c.Price) == 0);
        model.PaidCount = await query.CountAsync(c => (c.DiscountPrice != null && (c.DiscountStart == null || c.DiscountStart <= now) && (c.DiscountEnd == null || c.DiscountEnd >= now) ? c.DiscountPrice : c.Price) > 0);

        // 3. Lọc theo cấp độ và loại giá
        if (model.Levels.Count > 0)
            query = query.Where(c => model.Levels.Contains(c.Level));
        if (model.Price == "free")
            query = query.Where(c => (c.DiscountPrice != null && (c.DiscountStart == null || c.DiscountStart <= now) && (c.DiscountEnd == null || c.DiscountEnd >= now) ? c.DiscountPrice : c.Price) == 0);
        if (model.Price == "paid")
            query = query.Where(c => (c.DiscountPrice != null && (c.DiscountStart == null || c.DiscountStart <= now) && (c.DiscountEnd == null || c.DiscountEnd >= now) ? c.DiscountPrice : c.Price) > 0);

        // 4. Sắp xếp (giá dùng giá đang khuyến mãi nếu còn hạn)
        query = model.Sort switch
        {
            "newest" => query.OrderByDescending(c => c.PublishedAt),
            "rating" => query.OrderByDescending(c => c.AvgRating).ThenByDescending(c => c.PublishedAt),
            "price-asc" => query.OrderBy(c => c.DiscountPrice != null && (c.DiscountStart == null || c.DiscountStart <= now) && (c.DiscountEnd == null || c.DiscountEnd >= now) ? c.DiscountPrice : c.Price),
            "price-desc" => query.OrderByDescending(c => c.DiscountPrice != null && (c.DiscountStart == null || c.DiscountStart <= now) && (c.DiscountEnd == null || c.DiscountEnd >= now) ? c.DiscountPrice : c.Price),
            _ => query.OrderByDescending(c => c.AvgRating).ThenByDescending(c => c.PublishedAt)   // phổ biến
        };

        // 5. Phân trang
        model.Total = await query.CountAsync();
        model.Page = Math.Clamp(page, 1, model.TotalPages);
        model.Courses = await query
            .Include(c => c.Sections).ThenInclude(s => s.Lessons)
            .Skip((model.Page - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync();
        return model;
    }

    // ===== MENU "KHÓA HỌC" TRÊN HEADER =====
    public async Task<MegaMenuViewModel> GetMegaMenuAsync()
    {
        var categories = await GetActiveCategoriesAsync();

        // Đếm khóa học công khai theo từng danh mục
        var counts = await PublishedCourses()
            .GroupBy(c => c.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToListAsync();

        var items = new List<CategoryCardViewModel>();
        foreach (var category in categories)
        {
            // Danh mục cha cộng thêm số khóa học của danh mục con
            var ids = categories.Where(c => c.Id == category.Id || c.ParentId == category.Id).Select(c => c.Id).ToList();
            items.Add(new CategoryCardViewModel
            {
                Category = category,
                CourseCount = counts.Where(x => ids.Contains(x.CategoryId)).Sum(x => x.Count)
            });
        }

        return new MegaMenuViewModel
        {
            Categories = items.OrderByDescending(x => x.CourseCount).ThenBy(x => x.Category.Name).Take(10).ToList(),
            PopularCourses = await PublishedCourses().OrderByDescending(c => c.AvgRating).ThenByDescending(c => c.PublishedAt).Take(4).ToListAsync()
        };
    }

    public async Task<List<Category>> GetActiveCategoriesAsync()
    {
        return await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
    }

    // ===== CHI TIẾT (kèm chương, bài, tài liệu) =====
    public async Task<Course?> GetDetailAsync(long id)
    {
        return await PublishedCourses()
            .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // ===== XEM THỬ BÀI HỌC MIỄN PHÍ =====
    public async Task<LessonPreviewViewModel?> GetPreviewLessonAsync(long lessonId)
    {
        var lesson = await _db.Lessons
            .Include(l => l.Resources)
            .Include(l => l.Section)
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.IsFreePreview);
        if (lesson == null) return null;

        var course = await GetDetailAsync(lesson.Section!.CourseId);
        if (course == null) return null;
        return new LessonPreviewViewModel { Course = course, Lesson = lesson };
    }
}

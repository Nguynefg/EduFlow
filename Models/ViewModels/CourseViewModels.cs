using System.ComponentModel.DataAnnotations;

namespace EduFlow.Web.Models.ViewModels;

// ===== FORM TẠO / SỬA KHÓA HỌC (giảng viên) =====
public class CourseFormViewModel
{
    public long? Id { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? PickedThumbnailUrl { get; set; }   // ảnh bìa chọn từ thư viện media
    public string Status { get; set; } = CourseStatus.Draft;
    public string? RejectReason { get; set; }

    [Required(ErrorMessage = "Nhập tên khóa học."), StringLength(255), Display(Name = "Tên khóa học")]
    public string Title { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Chọn danh mục."), Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    [Display(Name = "Cấp độ")]
    public string Level { get; set; } = CourseLevel.Beginner;

    [StringLength(500), Display(Name = "Mô tả ngắn (hiện trên thẻ khóa học)")]
    public string? ShortDesc { get; set; }

    [Display(Name = "Mô tả chi tiết")]
    public string? Description { get; set; }

    [Display(Name = "Bạn sẽ học được gì (mỗi dòng một ý)")]
    public string? Objectives { get; set; }

    [Display(Name = "Yêu cầu đầu vào (mỗi dòng một ý)")]
    public string? Requirements { get; set; }

    [Display(Name = "Ảnh đại diện khóa học")]
    public IFormFile? ThumbnailImage { get; set; }

    [Url(ErrorMessage = "Link không hợp lệ."), StringLength(500), Display(Name = "Link video giới thiệu (YouTube)")]
    public string? TrailerUrl { get; set; }

    [Range(0, 100000000, ErrorMessage = "Giá từ 0 đến 100.000.000."), Display(Name = "Giá gốc (0 = miễn phí)")]
    public decimal Price { get; set; }

    [Range(0, 100000000, ErrorMessage = "Giá từ 0 đến 100.000.000."), Display(Name = "Giá khuyến mãi")]
    public decimal? DiscountPrice { get; set; }

    [Display(Name = "Khuyến mãi từ ngày")]
    public DateTime? DiscountStart { get; set; }

    [Display(Name = "Đến ngày")]
    public DateTime? DiscountEnd { get; set; }
}

// ===== FORM THÊM / SỬA BÀI HỌC =====
public class LessonFormViewModel
{
    public long? Id { get; set; }
    public long CourseId { get; set; }
    public long SectionId { get; set; }
    public string? CurrentVideoUrl { get; set; }
    public List<LessonResource> CurrentResources { get; set; } = new();

    [Required(ErrorMessage = "Nhập tên bài học."), StringLength(255), Display(Name = "Tên bài học")]
    public string Title { get; set; } = "";

    [Display(Name = "Loại bài")]
    public string LessonType { get; set; } = Models.LessonType.Video;

    [StringLength(500), Display(Name = "Link video (YouTube hoặc link .mp4)")]
    public string? VideoUrl { get; set; }

    [Display(Name = "Hoặc tải video lên (.mp4, .webm, tối đa 500 MB)")]
    public IFormFile? VideoFile { get; set; }

    [Range(0, 86400, ErrorMessage = "Thời lượng không hợp lệ."), Display(Name = "Thời lượng")]
    public int DurationSeconds { get; set; }   // tự lấy từ video khi chọn / tải lên

    [Display(Name = "Nội dung bài viết")]
    public string? Content { get; set; }

    [Range(0, 100, ErrorMessage = "Điểm đạt từ 0 đến 100."), Display(Name = "Điểm đạt (%)")]
    public int? PassScore { get; set; } = 70;

    [Display(Name = "Cho phép học thử miễn phí")]
    public bool IsFreePreview { get; set; }

    [Display(Name = "Tài liệu đính kèm (PDF, ZIP, ...)")]
    public List<IFormFile> ResourceFiles { get; set; } = new();

    // Câu hỏi soạn sẵn khi bài kiểm tra CHƯA lưu (JSON mảng QuestionFormViewModel) — lưu cùng bài học
    public string? DraftQuestionsJson { get; set; }
}

// ===== FORM THÊM CÂU HỎI QUIZ =====
public class QuestionFormViewModel
{
    public long? Id { get; set; }          // null = thêm mới
    public long LessonId { get; set; }

    [Required(ErrorMessage = "Nhập câu hỏi."), Display(Name = "Câu hỏi")]
    public string QuestionText { get; set; } = "";

    // single = trắc nghiệm (2–6 lựa chọn, 1 đáp án đúng), fill = điền khuyết (các đáp án được chấp nhận)
    public string QuestionType { get; set; } = "single";

    public List<string?> Options { get; set; } = new();
    public int CorrectIndex { get; set; }
}

// ===== TRANG NGOÀI (khách xem) =====
public class HomePageViewModel
{
    public Course? Featured { get; set; }
    public List<CategoryCardViewModel> TopCategories { get; set; } = new();     // Lưới "Danh mục nổi bật"
    public List<CategoryCardViewModel> SubjectTabs { get; set; } = new();       // Tab "Khám phá chủ đề"
    public List<Course> RecommendedCourses { get; set; } = new();              // Khóa học mới
    public List<Course> PopularCourses { get; set; } = new();                  // Đánh giá cao nhất
    public int TotalCourses { get; set; }
    public int TotalInstructors { get; set; }
    public int TotalStudents { get; set; }
    public int TotalLessons { get; set; }
}

// Danh mục cấp 1 kèm số khóa học (tính cả danh mục con) và vài khóa học tiêu biểu
public class CategoryCardViewModel
{
    public Category Category { get; set; } = null!;
    public int CourseCount { get; set; }
    public int LessonCount { get; set; }
    public List<Course> Courses { get; set; } = new();
}

// Dữ liệu cho menu "Khóa học" thả xuống trên header
public class MegaMenuViewModel
{
    public List<CategoryCardViewModel> Categories { get; set; } = new();   // Danh mục + số khóa học
    public List<Course> PopularCourses { get; set; } = new();             // 4 khóa học đánh giá cao
}

public class CourseListViewModel
{
    public decimal? MinRating { get; set; }
    public List<Course> Courses { get; set; } = new();
    public List<Category> Categories { get; set; } = new();

    // Bộ lọc đang chọn
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public List<string> Levels { get; set; } = new();     // beginner / intermediate / advanced (chọn nhiều)
    public string? Price { get; set; }                    // free / paid / null = tất cả
    public string Sort { get; set; } = "popular";         // popular / newest / price-asc / price-desc / rating

    // Số khóa học theo từng lựa chọn (hiện bên cạnh ô tick)
    public Dictionary<string, int> LevelCounts { get; set; } = new();
    public int FreeCount { get; set; }
    public int PaidCount { get; set; }

    // Phân trang
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int Total { get; set; }
    public int TotalPages => Math.Max(1, (Total + PageSize - 1) / PageSize);
}

public class LessonPreviewViewModel
{
    public Course Course { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}

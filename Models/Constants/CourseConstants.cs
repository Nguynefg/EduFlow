namespace EduFlow.Web.Models;

// Giá trị cột courses.status
public static class CourseStatus
{
    public const string Draft = "draft";          // Bản nháp
    public const string Pending = "pending";      // Chờ duyệt
    public const string Published = "published";  // Đã xuất bản
    public const string Rejected = "rejected";    // Bị từ chối
    public const string Hidden = "hidden";        // Admin tạm ẩn
}

// Giá trị cột courses.level
public static class CourseLevel
{
    public const string Beginner = "beginner";
    public const string Intermediate = "intermediate";
    public const string Advanced = "advanced";

    public static readonly string[] All = { Beginner, Intermediate, Advanced };
}

// Giá trị cột enrollments.status
public static class EnrollmentStatus
{
    public const string Active = "active";
    public const string Revoked = "revoked";       // Bị thu hồi (hoàn tiền...)
}

// Giá trị cột orders.status
public static class OrderStatus
{
    public const string Pending = "pending";
    public const string Paid = "paid";
    public const string Failed = "failed";
    public const string Cancelled = "cancelled";
}

// Giá trị cột lessons.lesson_type
public static class LessonType
{
    public const string Video = "video";
    public const string Article = "article";
    public const string Quiz = "quiz";
}

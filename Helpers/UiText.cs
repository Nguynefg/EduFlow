using EduFlow.Web.Models;

namespace EduFlow.Web.Helpers;

/// <summary>Chuyển mã trong CSDL thành chữ tiếng Việt để hiển thị.</summary>
public static class UiText
{
    public static string OrderStatus(string s) => s switch { "pending" => "Chờ thanh toán", "paid" => "Đã thanh toán", "failed" => "Thất bại", "cancelled" => "Đã hủy", _ => s };
    public static string PaymentStatus(string s) => s switch { "pending" => "Đang xử lý", "success" => "Thành công", "failed" => "Thất bại", _ => s };
    public static string PayoutStatus(string s) => s switch { "pending" => "Chờ duyệt", "approved" => "Đã duyệt, chờ chuyển khoản", "paid" => "Đã chuyển khoản", "rejected" => "Bị từ chối", _ => s };
    public static string Role(string role) => role switch
    {
        AppRoles.Admin => "Quản trị viên",
        AppRoles.Instructor => "Giảng viên",
        AppRoles.Student => "Học viên",
        _ => role
    };

    public static string CourseStatus(string status) => status switch
    {
        Models.CourseStatus.Draft => "Bản nháp",
        Models.CourseStatus.Pending => "Chờ duyệt",
        Models.CourseStatus.Published => "Đang công khai",
        Models.CourseStatus.Rejected => "Bị từ chối",
        Models.CourseStatus.Hidden => "Đã ẩn",
        _ => status
    };

    // Class màu cho nhãn trạng thái khóa học
    public static string CourseStatusCss(string status) => status switch
    {
        Models.CourseStatus.Published => "approved",
        Models.CourseStatus.Pending => "pending",
        Models.CourseStatus.Rejected => "rejected",
        _ => ""
    };

    public static string Level(string level) => level switch
    {
        CourseLevel.Beginner => "Cơ bản",
        CourseLevel.Intermediate => "Trung cấp",
        CourseLevel.Advanced => "Nâng cao",
        _ => level
    };

    public static string LessonType(string type) => type switch
    {
        Models.LessonType.Video => "Video",
        Models.LessonType.Article => "Bài viết",
        Models.LessonType.Quiz => "Bài kiểm tra",
        _ => type
    };

    public static string Money(decimal value) => value.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + " ₫";

    public static string Price(decimal price) =>
        price == 0 ? "Miễn phí" : price.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + " ₫";

    // Đổi số giây thành dạng 05:30 hoặc 1:05:30
    public static string Duration(int seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        return t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"mm\:ss");
    }

    public static string Kyc(string? status) => status switch
    {
        KycStatus.Pending => "Chờ xét duyệt",
        KycStatus.Approved => "Đã duyệt",
        KycStatus.Rejected => "Bị từ chối",
        _ => "Chưa gửi hồ sơ"
    };
}

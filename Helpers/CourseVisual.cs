namespace EduFlow.Web.Helpers;

// Chọn icon và màu cho danh mục / ảnh bìa khóa học (khi khóa học chưa có ảnh)
public static class CourseVisual
{
    // Icon Lucide theo tên danh mục
    public static string Icon(string? categoryName)
    {
        var name = (categoryName ?? "").ToLower();
        if (name.Contains("web")) return "monitor-smartphone";
        if (name.Contains("lập trình") || name.Contains("python") || name.Contains("c#")) return "code-xml";
        if (name.Contains("dữ liệu") || name.Contains("sql")) return "database";
        if (name.Contains("ai") || name.Contains("trí tuệ")) return "brain-circuit";
        if (name.Contains("thiết kế") || name.Contains("đồ họa")) return "palette";
        if (name.Contains("marketing")) return "megaphone";
        if (name.Contains("kinh doanh") || name.Contains("quản")) return "briefcase";
        if (name.Contains("ngoại ngữ") || name.Contains("tiếng")) return "languages";
        if (name.Contains("tin học") || name.Contains("văn phòng") || name.Contains("excel")) return "sheet";
        if (name.Contains("nhiếp ảnh") || name.Contains("video")) return "camera";
        if (name.Contains("âm nhạc")) return "music";
        if (name.Contains("sức khỏe") || name.Contains("thể")) return "heart-pulse";
        return "graduation-cap";
    }

    // Nền ảnh bìa: xanh navy đậm + một màu nhấn (giống ảnh bìa trong mẫu)
    private static readonly string[] Covers =
    {
        "cover-blue", "cover-amber", "cover-emerald", "cover-sky", "cover-rose", "cover-indigo"
    };

    public static string Cover(long courseId) => Covers[courseId % Covers.Length];
}

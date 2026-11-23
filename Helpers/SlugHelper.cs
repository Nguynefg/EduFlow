using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EduFlow.Web.Helpers;

public static class SlugHelper
{
    // "Lập trình C# cơ bản" -> "lap-trinh-c-co-ban"
    public static string ToSlug(string text)
    {
        text = text.ToLower().Replace("đ", "d");

        // Bỏ dấu tiếng Việt
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);

        var slug = Regex.Replace(builder.ToString(), "[^a-z0-9]+", "-").Trim('-');
        if (slug.Length > 200) slug = slug.Substring(0, 200);
        return slug == "" ? "khoa-hoc" : slug;
    }
}

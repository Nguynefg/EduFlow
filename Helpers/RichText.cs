using System.Text.RegularExpressions;
using Ganss.Xss;
using Microsoft.AspNetCore.Html;

namespace EduFlow.Web.Helpers;

// Nội dung soạn bằng trình soạn thảo (mô tả khóa học, bài viết):
// - Sanitize(): lọc mã độc (script, onclick, javascript:...) trước khi lưu
// - Render(): hiển thị ra trang; nội dung cũ chỉ là chữ thường thì giữ xuống dòng
public static class RichText
{
    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    private static HtmlSanitizer CreateSanitizer()
    {
        var s = new HtmlSanitizer();
        // Cho phép chèn video từ thư viện media
        foreach (var tag in new[] { "video", "source", "figure", "figcaption" }) s.AllowedTags.Add(tag);
        foreach (var attr in new[] { "controls", "preload", "poster", "playsinline", "loading", "data-filename" }) s.AllowedAttributes.Add(attr);
        return s;
    }

    public static string? Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return null;
        // Trình soạn thảo dùng link xem trước riêng; khi lưu trả về mã file gốc.
        html = Regex.Replace(html, @"src=""/Media/Video\?url=([^""<>]+)""", match =>
        {
            var url = Uri.UnescapeDataString(System.Net.WebUtility.HtmlDecode(match.Groups[1].Value));
            return url.StartsWith("/uploads/", StringComparison.Ordinal) ? "src=\"" + System.Net.WebUtility.HtmlEncode(url) + "\"" : match.Value;
        }, RegexOptions.IgnoreCase);
        var clean = Sanitizer.Sanitize(html).Trim();
        // Summernote để lại <p><br></p> khi ô soạn thảo trống
        var text = Regex.Replace(clean, "<[^>]+>|&nbsp;", "").Trim();
        if (text.Length == 0 && !Regex.IsMatch(clean, "<(img|video)", RegexOptions.IgnoreCase)) return null;
        return clean;
    }

    public static IHtmlContent Render(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return HtmlString.Empty;
        if (!LooksLikeHtml(content))
        {
            // Dữ liệu cũ dạng chữ thường: mã hóa rồi đổi xuống dòng thành <br>
            var encoded = System.Net.WebUtility.HtmlEncode(content.Trim()).Replace("\r\n", "\n").Replace("\n", "<br />");
            return new HtmlString("<p>" + encoded + "</p>");
        }
        return new HtmlString(Sanitizer.Sanitize(content));
    }

    // Đổi nội dung chữ thường thành HTML để nạp vào trình soạn thảo
    public static string? ToEditorHtml(string? content)
    {
        if (string.IsNullOrWhiteSpace(content) || LooksLikeHtml(content)) return content;
        return Render(content).ToString();
    }

    private static bool LooksLikeHtml(string s) => Regex.IsMatch(s, @"<\s*[a-zA-Z][^>]*>");
}

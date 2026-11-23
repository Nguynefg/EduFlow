using System.Text.RegularExpressions;

namespace EduFlow.Web.Helpers;

public static class VideoHelper
{
    public static bool IsProtectedFile(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        if (path.Equals("/uploads/resources", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/uploads/resources/", StringComparison.OrdinalIgnoreCase)) return true;
        return path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) && FileUpload.VideoExtensions.Contains(Path.GetExtension(path).ToLowerInvariant());
    }
    public static string? ProtectHtml(string? html,string endpoint)
    {
        if(string.IsNullOrEmpty(html))return html;
        html=RichText.Sanitize(html);
        if(string.IsNullOrEmpty(html))return html;
        var index=0;
        return Regex.Replace(html,@"src=""(/uploads/[^""<>]+\.(?:mp4|webm))""",m=>"src=\""+System.Net.WebUtility.HtmlEncode(endpoint+(endpoint.Contains('?')?"&":"?")+"index="+(index++))+"\"",RegexOptions.IgnoreCase);
    }
    public static List<string> LocalSources(string? html) => Regex.Matches(RichText.Sanitize(html)??"",@"src=""(/uploads/[^""<>]+\.(?:mp4|webm))""",RegexOptions.IgnoreCase).Select(m=>System.Net.WebUtility.HtmlDecode(m.Groups[1].Value)).ToList();
    // Đổi link YouTube thành link nhúng. Không phải YouTube thì trả về null (dùng thẻ <video>).
    public static string? YouTubeEmbed(string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        var match = Regex.Match(url, @"(?:youtube\.com/(?:watch\?v=|embed/|shorts/)|youtu\.be/)([A-Za-z0-9_-]{11})");
        return match.Success ? "https://www.youtube.com/embed/" + match.Groups[1].Value : null;
    }
}

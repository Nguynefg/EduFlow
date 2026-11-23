using System.Security.Claims;
using EduFlow.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EduFlow.Web.Helpers;

public static class AuthHelper
{
    public static string PasswordStamp(User user) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(user.PasswordHash ?? "SSO")));

    // Tên cookie tạm dùng khi đăng nhập bằng Google
    public const string ExternalScheme = "External";

    // Tạo thông tin đăng nhập lưu trong cookie (user phải Include UserRoles.Role)
    public static ClaimsPrincipal CreatePrincipal(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("password_stamp", PasswordStamp(user)),
            new Claim(ClaimTypes.Email, user.Email)
        };
        foreach (var role in user.RoleCodes)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    // Lấy id của người đang đăng nhập
    public static long GetUserId(ClaimsPrincipal user)
    {
        return long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    // Mật khẩu: ít nhất 10 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt
    public static string? CheckPassword(string? password)
    {
        if (password == null || password.Length < 10) return "Mật khẩu phải có ít nhất 10 ký tự.";
        if (!password.Any(char.IsUpper)) return "Mật khẩu phải có chữ hoa.";
        if (!password.Any(char.IsLower)) return "Mật khẩu phải có chữ thường.";
        if (!password.Any(char.IsDigit)) return "Mật khẩu phải có chữ số.";
        if (password.All(char.IsLetterOrDigit)) return "Mật khẩu phải có ký tự đặc biệt.";
        return null;
    }

    // Tạo mật khẩu tạm ngẫu nhiên khi người dùng quên mật khẩu
    public static string RandomPassword()
    {
        return "Edu@" + Random.Shared.Next(100000, 999999) + Guid.NewGuid().ToString("N")[..4].ToUpper() + "x";
    }
}

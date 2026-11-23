using System.Text.Json;
using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Services;

// Xử lý hồ sơ cá nhân và đổi mật khẩu
public class ProfileService
{
    private readonly EduFlowDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public ProfileService(EduFlowDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // Lấy thông tin hồ sơ để hiển thị lên form
    public async Task<ProfileViewModel> GetProfileAsync(long userId)
    {
        var user = await GetUserAsync(userId);
        var links = new SocialLinks();
        if (!string.IsNullOrEmpty(user.SocialLinks))
            links = JsonSerializer.Deserialize<SocialLinks>(user.SocialLinks) ?? new SocialLinks();

        return new ProfileViewModel
        {
            Email = user.Email,
            EmailVerifiedAt = user.EmailVerifiedAt,
            AvatarUrl = user.AvatarUrl,
            Roles = user.RoleCodes,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            Facebook = links.Facebook,
            Github = links.Github,
            Linkedin = links.Linkedin
        };
    }

    // Lưu hồ sơ. newAvatarUrl = null nghĩa là giữ ảnh cũ.
    public async Task UpdateProfileAsync(long userId, ProfileViewModel model, string? newAvatarUrl)
    {
        var user = await GetUserAsync(userId);
        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber;
        user.Bio = model.Bio;
        if (newAvatarUrl != null) user.AvatarUrl = newAvatarUrl;

        var links = new SocialLinks { Facebook = model.Facebook, Github = model.Github, Linkedin = model.Linkedin };
        user.SocialLinks = JsonSerializer.Serialize(links);
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    // Đổi mật khẩu: trả về thông báo lỗi, null nếu thành công
    public async Task<string?> ChangePasswordAsync(long userId, string? currentPassword, string newPassword)
    {
        var user = await GetUserAsync(userId);
        // Đã có mật khẩu thì phải nhập đúng mật khẩu cũ. Tài khoản Google chưa có mật khẩu thì bỏ qua bước này.
        if (user.PasswordHash != null &&
            _hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword ?? "") == PasswordVerificationResult.Failed)
            return "Mật khẩu hiện tại không đúng.";

        var passwordError = AuthHelper.CheckPassword(newPassword);
        if (passwordError != null) return passwordError;

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    private async Task<User> GetUserAsync(long userId)
    {
        return await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == userId);
    }
}

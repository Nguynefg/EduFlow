using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Services;

// Xử lý đăng nhập, đăng ký, quên mật khẩu
public class AccountService
{
    private readonly EduFlowDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly SecurityThrottle _throttle;

    public AccountService(EduFlowDbContext db, IPasswordHasher<User> hasher, SecurityThrottle throttle)
    {
        _db = db;
        _hasher = hasher;
        _throttle = throttle;
    }

    // Hệ thống đã có quản trị viên nào chưa
    public async Task<bool> HasAdminAsync()
    {
        return await _db.UserRoles.AnyAsync(x => x.Role!.Code == AppRoles.Admin);
    }

    // Lấy user kèm vai trò và hồ sơ giảng viên
    public async Task<User?> GetUserAsync(long id)
    {
        return await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.InstructorProfile)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    // Đăng nhập: trả về user nếu đúng, ngược lại trả về thông báo lỗi
    public async Task<(User? User, string? Error)> LoginAsync(string email, string password)
    {
        if (!_throttle.Allow("login", email, 10, TimeSpan.FromMinutes(15)))
            return (null, "Bạn đã thử đăng nhập quá nhiều lần. Vui lòng thử lại sau 15 phút.");
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email.Trim() && u.DeletedAt == null);

        if (user == null || user.PasswordHash == null ||
            _hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
            return (null, "Email hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            return (null, "Tài khoản đang bị khóa. Vui lòng liên hệ quản trị viên.");

        user.LastLoginAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return (user, null);
    }

    // Đăng ký tài khoản mới với 1 vai trò (student hoặc admin khi cài đặt lần đầu)
    public async Task<(User? User, string? Error)> RegisterAsync(RegisterViewModel model, string roleCode)
    {
        var passwordError = AuthHelper.CheckPassword(model.Password);
        if (passwordError != null) return (null, passwordError);

        if (await _db.Users.AnyAsync(u => u.Email == model.Email.Trim()))
            return (null, "Email này đã được đăng ký.");

        await using var setupTransaction = roleCode == AppRoles.Admin
            ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable) : null;
        var role = roleCode == AppRoles.Admin
            ? await _db.Roles.FromSqlInterpolated($"SELECT * FROM roles WITH (UPDLOCK,HOLDLOCK) WHERE code={roleCode}").FirstAsync()
            : await _db.Roles.FirstAsync(r => r.Code == roleCode);
        if (roleCode == AppRoles.Admin && await HasAdminAsync())
            return (null, "Hệ thống đã có quản trị viên. Trang thiết lập đã đóng.");
        var user = new User
        {
            Email = model.Email.Trim(),
            FullName = model.FullName.Trim()
        };
        user.PasswordHash = _hasher.HashPassword(user, model.Password);
        user.UserRoles.Add(new UserRole { Role = role });

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        if (setupTransaction != null) await setupTransaction.CommitAsync();
        return (user, null);
    }

    // Đăng nhập bằng Google:
    // - Email đã có tài khoản -> đăng nhập vào tài khoản đó (Google đã xác thực email)
    // - Chưa có -> tạo tài khoản học viên mới, không có mật khẩu
    public async Task<(User? User, string? Error)> LoginWithGoogleAsync(string email, string fullName, string? avatarUrl)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user != null)
        {
            if (user.DeletedAt != null) return (null, "Tài khoản này đã bị xóa. Vui lòng liên hệ quản trị viên.");
            if (!user.IsActive) return (null, "Tài khoản đang bị khóa. Vui lòng liên hệ quản trị viên.");
        }
        else
        {
            var role = await _db.Roles.FirstAsync(r => r.Code == AppRoles.Student);
            user = new User
            {
                Email = email,
                FullName = string.IsNullOrWhiteSpace(fullName) ? email.Split('@')[0] : fullName,
                AvatarUrl = avatarUrl
            };
            user.UserRoles.Add(new UserRole { Role = role });
            _db.Users.Add(user);
        }

        user.EmailVerifiedAt ??= DateTime.Now;
        user.LastLoginAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return (user, null);
    }

    // Quên mật khẩu: đặt mật khẩu tạm mới. Trả về (user, mật khẩu tạm), hoặc null nếu email không hợp lệ.
    public async Task<(User User, string TempPassword)?> ResetPasswordAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Email == email.Trim() && u.DeletedAt == null && u.IsActive);
        if (user == null) return null;

        var tempPassword = AuthHelper.RandomPassword();
        user.PasswordHash = _hasher.HashPassword(user, tempPassword);
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return (user, tempPassword);
    }
}

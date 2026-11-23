using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Areas.Admin.Services;

// Admin quản lý người dùng: thống kê, tìm kiếm, thêm, sửa, phân quyền, khóa, xóa mềm, khôi phục
public class AdminUserService
{
    private readonly EduFlowDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public AdminUserService(EduFlowDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // ===== TỔNG QUAN =====
    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        var users = _db.Users.Where(u => u.DeletedAt == null);
        return new AdminDashboardViewModel
        {
            TotalUsers = await users.CountAsync(),
            Students = await users.CountAsync(u => u.UserRoles.Any(r => r.Role!.Code == AppRoles.Student)),
            Instructors = await users.CountAsync(u => u.UserRoles.Any(r => r.Role!.Code == AppRoles.Instructor)),
            Admins = await users.CountAsync(u => u.UserRoles.Any(r => r.Role!.Code == AppRoles.Admin)),
            Locked = await users.CountAsync(u => !u.IsActive),
            Deleted = await _db.Users.CountAsync(u => u.DeletedAt != null),
            NewThisWeek = await users.CountAsync(u => u.CreatedAt >= DateTime.Now.AddDays(-7)),
            PendingKyc = await _db.InstructorProfiles.CountAsync(p => p.KycStatus == KycStatus.Pending),
            RecentUsers = await users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Take(6)
                .ToListAsync()
        };
    }

    // ===== DANH SÁCH + TÌM KIẾM + PHÂN TRANG =====
    public async Task<AdminUsersViewModel> SearchAsync(string? q, string? role, string? status, bool deleted, int page)
    {
        var query = _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (deleted)
            query = query.Where(u => u.DeletedAt != null);
        else
            query = query.Where(u => u.DeletedAt == null);

        if (!string.IsNullOrEmpty(q))
            query = query.Where(u => u.FullName.Contains(q) || u.Email.Contains(q));
        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.UserRoles.Any(r => r.Role!.Code == role));
        if (status == "active")
            query = query.Where(u => u.IsActive);
        if (status == "locked")
            query = query.Where(u => !u.IsActive);

        var model = new AdminUsersViewModel
        {
            Query = q,
            Role = role,
            Status = status,
            Deleted = deleted,
            Page = page < 1 ? 1 : page,
            Total = await query.CountAsync()
        };
        model.Users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((model.Page - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync();
        return model;
    }

    // Lấy dữ liệu đổ vào form sửa, null nếu không tìm thấy
    public async Task<AdminUserFormViewModel?> GetFormAsync(long id)
    {
        var user = await FindAsync(id);
        if (user == null) return null;
        return new AdminUserFormViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            Roles = user.RoleCodes
        };
    }

    // ===== THÊM =====  Trả về thông báo lỗi, null nếu thành công
    public async Task<string?> CreateAsync(AdminUserFormViewModel model)
    {
        var error = AuthHelper.CheckPassword(model.Password) ?? await CheckFormAsync(model, null);
        if (error != null) return error;

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email.Trim(),
            PhoneNumber = model.PhoneNumber,
            Bio = model.Bio
        };
        user.PasswordHash = _hasher.HashPassword(user, model.Password!);
        await SetRolesAsync(user, model.Roles);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return null;
    }

    // ===== SỬA + PHÂN QUYỀN =====  Mật khẩu để trống = giữ nguyên
    public async Task<string?> UpdateAsync(long id, AdminUserFormViewModel model, long currentAdminId)
    {
        var user = await FindAsync(id);
        if (user == null) return "Không tìm thấy tài khoản.";

        if (!string.IsNullOrEmpty(model.Password))
        {
            var passwordError = AuthHelper.CheckPassword(model.Password);
            if (passwordError != null) return passwordError;
        }
        if (id == currentAdminId && !model.Roles.Contains(AppRoles.Admin))
            return "Không thể tự gỡ quyền quản trị của chính mình.";
        var error = await CheckFormAsync(model, id);
        if (error != null) return error;

        user.FullName = model.FullName;
        user.Email = model.Email.Trim();
        user.PhoneNumber = model.PhoneNumber;
        user.Bio = model.Bio;
        user.UpdatedAt = DateTime.Now;
        if (!string.IsNullOrEmpty(model.Password))
            user.PasswordHash = _hasher.HashPassword(user, model.Password);
        await SetRolesAsync(user, model.Roles);

        await _db.SaveChangesAsync();
        return null;
    }

    // ===== KHÓA / MỞ KHÓA =====
    public async Task<string?> SetActiveAsync(long id, bool active, long currentAdminId)
    {
        var user = await FindAsync(id);
        var error = CheckCanChange(user, currentAdminId);
        if (error != null) return error;

        user!.IsActive = active;
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    // ===== XÓA MỀM =====
    public async Task<string?> DeleteAsync(long id, long currentAdminId)
    {
        var user = await FindAsync(id);
        var error = CheckCanChange(user, currentAdminId);
        if (error != null) return error;

        user!.DeletedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    // ===== KHÔI PHỤC =====
    public async Task<string?> RestoreAsync(long id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt != null);
        if (user == null) return "Không tìm thấy tài khoản đã xóa.";

        user.DeletedAt = null;
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }

    // Tìm user chưa bị xóa, kèm vai trò
    private async Task<User?> FindAsync(long id)
    {
        return await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);
    }

    // Không cho khóa/xóa chính mình hoặc quản trị viên khác
    private static string? CheckCanChange(User? user, long currentAdminId)
    {
        if (user == null) return "Không tìm thấy tài khoản.";
        if (user.Id == currentAdminId) return "Không thể khóa hoặc xóa chính tài khoản của bạn.";
        if (user.RoleCodes.Contains(AppRoles.Admin)) return "Không thể khóa hoặc xóa quản trị viên. Hãy gỡ quyền quản trị trước.";
        return null;
    }

    private async Task<string?> CheckFormAsync(AdminUserFormViewModel model, long? id)
    {
        if (model.Roles.Count == 0) return "Chọn ít nhất một vai trò.";
        if (await _db.Users.AnyAsync(u => u.Email == model.Email.Trim() && u.Id != id))
            return "Email này đã thuộc tài khoản khác.";
        return null;
    }

    // Gán lại danh sách vai trò cho user
    private async Task SetRolesAsync(User user, List<string> roleCodes)
    {
        user.UserRoles.Clear();
        var roles = await _db.Roles.Where(r => roleCodes.Contains(r.Code)).ToListAsync();
        foreach (var role in roles)
            user.UserRoles.Add(new UserRole { RoleId = role.Id, Role = role });
    }
}

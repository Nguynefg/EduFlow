using EduFlow.Web.Models;
using EduFlow.Web.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Areas.Admin.Services;

// Admin duyệt / từ chối hồ sơ đăng ký làm giảng viên
public class AdminKycService
{
    private readonly EduFlowDbContext _db;

    public AdminKycService(EduFlowDbContext db)
    {
        _db = db;
    }

    // Số hồ sơ đang chờ duyệt (hiện trên menu admin)
    public async Task<int> CountPendingAsync()
    {
        return await _db.InstructorProfiles.CountAsync(p => p.KycStatus == KycStatus.Pending);
    }

    // Danh sách hồ sơ theo trạng thái (để trống = tất cả)
    public async Task<AdminKycViewModel> ListAsync(string? status)
    {
        var query = _db.InstructorProfiles
            .Include(p => p.User)
            .Where(p => p.User!.DeletedAt == null);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.KycStatus == status);

        return new AdminKycViewModel
        {
            Status = status,
            Profiles = await query.OrderByDescending(p => p.SubmittedAt).ToListAsync(),
            PendingCount = await CountPendingAsync()
        };
    }

    // Duyệt: đổi trạng thái và cấp thêm quyền giảng viên. Trả về tên người được duyệt, null nếu không tìm thấy.
    public async Task<string?> ApproveAsync(long userId)
    {
        var profile = await _db.InstructorProfiles
            .Include(p => p.User).ThenInclude(u => u!.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.KycStatus == KycStatus.Pending);
        if (profile == null) return null;

        profile.KycStatus = KycStatus.Approved;
        profile.KycRejectReason = null;

        // Vẫn giữ quyền học viên, chỉ thêm quyền giảng viên
        if (!profile.User!.RoleCodes.Contains(AppRoles.Instructor))
        {
            var role = await _db.Roles.FirstAsync(r => r.Code == AppRoles.Instructor);
            profile.User.UserRoles.Add(new UserRole { RoleId = role.Id });
        }
        await _db.SaveChangesAsync();
        return profile.User.FullName;
    }

    // Từ chối kèm lý do. Trả về tên người bị từ chối, null nếu không tìm thấy.
    public async Task<string?> RejectAsync(long userId, string reason)
    {
        var profile = await _db.InstructorProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.KycStatus == KycStatus.Pending);
        if (profile == null) return null;

        profile.KycStatus = KycStatus.Rejected;
        profile.KycRejectReason = reason;
        await _db.SaveChangesAsync();
        return profile.User!.FullName;
    }
}

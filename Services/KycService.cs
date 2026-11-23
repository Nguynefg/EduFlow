using EduFlow.Web.Models;
using EduFlow.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Services;

// Học viên gửi hồ sơ KYC để đăng ký làm giảng viên
public class KycService
{
    private readonly EduFlowDbContext _db;

    public KycService(EduFlowDbContext db)
    {
        _db = db;
    }

    // Lấy trang KYC: hồ sơ đã gửi (nếu có) và dữ liệu điền sẵn vào form
    public async Task<KycPageViewModel> GetPageAsync(long userId, bool isInstructor)
    {
        var profile = await _db.InstructorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        var model = new KycPageViewModel { Profile = profile, IsInstructor = isInstructor };
        if (profile != null)
        {
            model.Form.Headline = profile.Headline ?? "";
            model.Form.IdentityCard = profile.IdentityCard ?? "";
            model.Form.TaxId = profile.TaxId;
            model.Form.BankName = profile.BankName ?? "";
            model.Form.BankAccountNumber = profile.BankAccountNumber ?? "";
            model.Form.BankAccountName = profile.BankAccountName ?? "";
        }
        return model;
    }

    // Gửi hoặc gửi lại hồ sơ. Trả về thông báo lỗi, null nếu thành công.
    public async Task<string?> SubmitAsync(long userId, bool isInstructor, KycFormViewModel form)
    {
        var profile = await _db.InstructorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        // Chỉ được gửi khi chưa có hồ sơ hoặc hồ sơ bị từ chối
        if (isInstructor || (profile != null && profile.KycStatus != KycStatus.Rejected))
            return "Hồ sơ của bạn đang chờ duyệt hoặc đã được duyệt.";

        if (profile == null)
        {
            profile = new InstructorProfile { UserId = userId };
            _db.InstructorProfiles.Add(profile);
        }
        profile.Headline = form.Headline;
        profile.IdentityCard = form.IdentityCard;
        profile.TaxId = form.TaxId;
        profile.BankName = form.BankName;
        profile.BankAccountNumber = form.BankAccountNumber;
        profile.BankAccountName = form.BankAccountName.ToUpper();
        profile.KycStatus = KycStatus.Pending;
        profile.KycRejectReason = null;
        profile.SubmittedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return null;
    }
}

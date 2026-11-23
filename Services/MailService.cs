using System.Text.Json;
using System.Security.Cryptography;
using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace EduFlow.Web.Services;
public class MailService
{
    private readonly EduFlowDbContext _db;private readonly EmailSender _sender;private readonly IDataProtector _protector;private readonly IPasswordHasher<User> _hasher;private readonly SecurityThrottle _throttle;private readonly IConfiguration _config;private readonly IWebHostEnvironment _env;
    public MailService(EduFlowDbContext db,EmailSender sender,IDataProtectionProvider protection,IPasswordHasher<User> hasher,IConfiguration config,IWebHostEnvironment env,SecurityThrottle throttle){_throttle=throttle;_db=db;_sender=sender;_protector=protection.CreateProtector("EduFlow.MailTokens.v1");_hasher=hasher;_config=config;_env=env;}
    public string? PublicUrl
    {
        get {var raw=_config["Mail:PublicBaseUrl"];if(!Uri.TryCreate(raw,UriKind.Absolute,out var uri)||uri.UserInfo!=""||uri.Query!=""||uri.Fragment!=""||uri.AbsolutePath!="/"||(!_env.IsDevelopment()&&(uri.Scheme!="https"||uri.IsLoopback))||!new[]{"https","http"}.Contains(uri.Scheme))return null;return raw!.TrimEnd('/');}
    }
    private record Ticket(long Id,string Email,string Stamp,string Purpose,long Expires);
    private string Token(User user,string purpose)=>_protector.Protect(JsonSerializer.Serialize(new Ticket(user.Id,user.Email,AuthHelper.PasswordStamp(user),purpose,DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())));
    private Ticket? Decode(string? value,string purpose)
    {
        if(string.IsNullOrWhiteSpace(value)||value.Length>8192)return null;
        try{var ticket=JsonSerializer.Deserialize<Ticket>(_protector.Unprotect(value));return ticket?.Purpose==purpose&&ticket.Expires>DateTimeOffset.UtcNow.ToUnixTimeSeconds()?ticket:null;}catch(Exception e) when(e is CryptographicException or JsonException or FormatException){return null;}
    }
    public async Task<bool> VerifyRequestAsync(long id)
    {
        var user=await _db.Users.FirstOrDefaultAsync(u=>u.Id==id&&u.IsActive&&u.DeletedAt==null);if(user==null||user.EmailVerifiedAt!=null||PublicUrl==null)return false;
        if (!_throttle.Allow("verify-mail", user.Email, 1, TimeSpan.FromMinutes(1)) || !_throttle.Allow("verify-hour", user.Email, 5, TimeSpan.FromHours(1))) return false;
        return await _sender.SendAsync(user.Email,"Xác thực email EduFlow",$"Xin chào {user.FullName},\nMở liên kết để xác thực email (hiệu lực 1 giờ):\n{PublicUrl}/Account/VerifyEmail?token={Uri.EscapeDataString(Token(user,"verify"))}\nNếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.");
    }
    public async Task<bool> ConfirmAsync(string? token)
    {
        var ticket=Decode(token,"verify");if(ticket==null)return false;
        var user=await _db.Users.AsNoTracking().FirstOrDefaultAsync(u=>u.Id==ticket.Id&&u.Email==ticket.Email&&u.IsActive&&u.DeletedAt==null);
        if(user==null||ticket.Stamp!=AuthHelper.PasswordStamp(user))return false;
        return await _db.Users.Where(u=>u.Id==user.Id&&u.Email==ticket.Email&&u.EmailVerifiedAt==null&&u.IsActive&&u.DeletedAt==null).ExecuteUpdateAsync(set=>set.SetProperty(u=>u.EmailVerifiedAt,DateTime.Now))==1;
    }
    public async Task ResetRequestAsync(string email)
    {
        var user=await _db.Users.FirstOrDefaultAsync(u=>u.Email==email.Trim()&&u.IsActive&&u.DeletedAt==null&&u.PasswordHash!=null);
        if(user==null||PublicUrl==null)return;
        if (!_throttle.Allow("reset-mail", user.Email, 1, TimeSpan.FromMinutes(1)) || !_throttle.Allow("reset-hour", user.Email, 5, TimeSpan.FromHours(1))) return;
        await _sender.SendAsync(user.Email,"Đặt lại mật khẩu EduFlow",$"Xin chào {user.FullName},\nMở liên kết để đặt mật khẩu mới (hiệu lực 1 giờ):\n{PublicUrl}/Password/Reset?token={Uri.EscapeDataString(Token(user,"reset"))}\nMật khẩu hiện tại chưa thay đổi. Nếu không phải bạn yêu cầu, hãy bỏ qua email.");
    }
    public async Task<string?> ResetAsync(string? token,string password)
    {
        var error=AuthHelper.CheckPassword(password);if(error!=null)return error;
        var ticket=Decode(token,"reset");if(ticket==null)return "Liên kết không hợp lệ hoặc đã hết hạn.";
        var user=await _db.Users.AsNoTracking().FirstOrDefaultAsync(u=>u.Id==ticket.Id&&u.Email==ticket.Email&&u.IsActive&&u.DeletedAt==null);
        if(user==null||ticket.Stamp!=AuthHelper.PasswordStamp(user))return "Liên kết đã được dùng hoặc thông tin tài khoản đã thay đổi.";
        var hash=_hasher.HashPassword(user,password);
        var count=await _db.Users.Where(u=>u.Id==user.Id&&u.PasswordHash==user.PasswordHash&&u.Email==ticket.Email&&u.IsActive&&u.DeletedAt==null).ExecuteUpdateAsync(set=>set.SetProperty(u=>u.PasswordHash,hash).SetProperty(u=>u.UpdatedAt,DateTime.Now));
        return count==1?null:"Liên kết đã được dùng. Hãy yêu cầu liên kết mới.";
    }
    public async Task SendUserAsync(long userId,string subject,string body)
    {
        var user=await _db.Users.AsNoTracking().FirstOrDefaultAsync(u=>u.Id==userId&&u.IsActive&&u.DeletedAt==null);if(user==null)return;
        await _sender.SendAsync(user.Email,subject,$"Xin chào {user.FullName},\n\n{body}\n\nEduFlow");
    }
    public async Task PaidAsync(long orderId,bool simulated)
    {
        var order=await _db.Orders.AsNoTracking().Include(o=>o.Items).FirstOrDefaultAsync(o=>o.Id==orderId&&o.Status==OrderStatus.Paid);if(order==null)return;
        await SendUserAsync(order.UserId,simulated?"Thanh toán mô phỏng thành công":"Thanh toán thành công",$"Đơn #{order.Id} đã thanh toán: {UiText.Money(order.TotalPrice)}. Bạn đã có quyền học {order.Items.Count} khóa học."+(simulated?" Đây là giao dịch mô phỏng, không trừ tiền thật.":""));
    }
    public async Task CourseAsync(long id,string status,string? reason)
    {
        var course=await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c=>c.Id==id);if(course!=null)await SendUserAsync(course.InstructorId,"Kết quả xét duyệt khóa học",$"Khóa học {course.Title}: {status}.\n{reason}");
    }
}

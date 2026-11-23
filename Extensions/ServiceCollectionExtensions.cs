using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Areas.Admin.Services;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace EduFlow.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddEduFlow(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllersWithViews();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<SecurityThrottle>();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await context.HttpContext.Response.WriteAsync("Quá nhiều yêu cầu. Vui lòng thử lại sau một phút.", token);
            };
            options.AddPolicy("auth", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
                }));
        });

        // Kết nối SQL Server
        services.AddDbContext<EduFlowDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Tiện ích: băm mật khẩu, gửi email
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<EmailSender>();
        services.AddScoped<MailService>();

        // Tầng Service phía người dùng
        services.AddScoped<AccountService>();
        services.AddScoped<ProfileService>();
        services.AddScoped<KycService>();
        services.AddScoped<CourseService>();
        services.AddScoped<InstructorCourseService>();
        services.AddScoped<CurriculumService>();
        services.AddScoped<MediaLibraryService>();
        services.AddScoped<LearningService>();
        services.AddScoped<CommunityService>();
        services.AddScoped<CommerceService>();
        services.AddScoped<FinanceService>();
        services.AddScoped<ReportService>();
        services.AddScoped<ProtectedVideoService>();
        services.AddScoped<MediaOwnershipService>();
        services.AddScoped<StudyClockService>();
        services.AddScoped<VideoWatchService>();
        services.AddScoped<PaymentGatewayService>();
        services.AddHttpClient("payments", client => client.Timeout = TimeSpan.FromSeconds(35));

        // Tầng Service khu quản trị (Areas/Admin/Services)
        services.AddScoped<AdminUserService>();
        services.AddScoped<AdminKycService>();
        services.AddScoped<AdminCourseService>();
        services.AddScoped<AdminCategoryService>();

        // Đăng nhập bằng cookie
        var auth = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

        // Đăng nhập Google: chỉ bật khi đã cấu hình ClientId và ClientSecret
        // (đặt bằng: dotnet user-secrets set "Authentication:Google:ClientId" "...")
        var googleId = configuration["Authentication:Google:ClientId"];
        var googleSecret = configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrEmpty(googleId) && !string.IsNullOrEmpty(googleSecret))
        {
            // Cookie tạm giữ thông tin Google trả về, trước khi đăng nhập vào EduFlow
            auth.AddCookie(AuthHelper.ExternalScheme);
            auth.AddGoogle(options =>
            {
                options.ClientId = googleId;
                options.ClientSecret = googleSecret;
                options.SignInScheme = AuthHelper.ExternalScheme;
                options.ClaimActions.MapJsonKey("picture", "picture");   // lấy thêm ảnh đại diện
            });
        }

        auth.AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);

                // Mỗi lần tải trang: đọc lại user trong CSDL.
                // Bị khóa hoặc bị xóa thì đăng xuất, được cấp quyền mới thì cập nhật quyền ngay.
                options.Events.OnValidatePrincipal = async context =>
                {
                    var accountService = context.HttpContext.RequestServices.GetRequiredService<AccountService>();
                    var user = await accountService.GetUserAsync(AuthHelper.GetUserId(context.Principal!));

                    if (user == null || !user.IsActive || user.DeletedAt != null || context.Principal?.FindFirst("password_stamp")?.Value != AuthHelper.PasswordStamp(user))
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync();
                        return;
                    }
                    context.ReplacePrincipal(AuthHelper.CreatePrincipal(user));
                };
            });
    }
}

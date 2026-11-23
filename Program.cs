using EduFlow.Web.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEduFlow(builder.Configuration);
// Chỉ tin reverse proxy đã chỉ định, không tin X-Forwarded-For từ mọi địa chỉ.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    foreach (var value in builder.Configuration.GetSection("Hosting:TrustedProxies").Get<string[]>() ?? Array.Empty<string>())
    {
        if (!IPAddress.TryParse(value, out var address)) throw new InvalidOperationException("TrustedProxies phải là địa chỉ IP hợp lệ.");
        options.KnownProxies.Add(address);
    }
});

var app = builder.Build();
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context,next) =>
{
    var path=context.Request.Path.Value??"";
    if(EduFlow.Web.Helpers.VideoHelper.IsProtectedFile(path))
    {context.Response.StatusCode=404;return;}
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllerRoute(name: "admin", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "course-slug",pattern:"khoa-hoc/{slug}",defaults:new {controller="Courses",action="BySlug"});
app.MapControllerRoute(name: "category-slug",pattern:"danh-muc/{slug}",defaults:new {controller="Courses",action="Category"});

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

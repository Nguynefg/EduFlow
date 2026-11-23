using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EduFlow.Web.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles=AppRoles.Admin), AutoValidateAntiforgeryToken]
public class CommunityController:Controller
{
    private readonly EduFlowDbContext _db;
    private readonly CommunityService _community;
    public CommunityController(EduFlowDbContext db,CommunityService community){_db=db;_community=community;}
    public async Task<IActionResult> Index(bool? hidden,int page=1)
    {
        var id=AuthHelper.GetUserId(User);
        if(!await _db.Users.AnyAsync(u=>u.Id==id && u.IsActive && u.DeletedAt==null && u.UserRoles.Any(r=>r.Role!.Code==AppRoles.Admin))) return Forbid();
        var query=_db.CourseReviews.AsNoTracking().Where(r=>hidden==null || r.IsHidden==hidden);
        var count=await query.CountAsync();var pages=Math.Max(1,(int)Math.Ceiling(count/20d));page=Math.Clamp(page,1,pages);
        ViewBag.Hidden=hidden;ViewBag.Page=page;ViewBag.Pages=pages;
        return View(await query.Include(r=>r.User).Include(r=>r.Course).OrderByDescending(r=>r.CreatedAt).ThenBy(r=>r.CourseId).ThenBy(r=>r.UserId).Skip((page-1)*20).Take(20).ToListAsync());
    }
    [HttpPost]
    public async Task<IActionResult> Hide(long courseId,long reviewerId,bool hidden)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Dữ liệu kiểm duyệt không hợp lệ."; return RedirectToAction("Index"); }
        var error=await _community.HideReviewAsync(AuthHelper.GetUserId(User),courseId,reviewerId,hidden);
        TempData[error==null?"Success":"Error"]=error??"Đã cập nhật trạng thái đánh giá.";return RedirectToAction("Index");
    }
}

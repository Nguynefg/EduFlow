using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EduFlow.Web.Controllers;
[Authorize,ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class MailboxController:Controller
{
    private readonly EmailSender _sender;private readonly EduFlowDbContext _db;private readonly MailService _mail;
    public MailboxController(EmailSender sender,EduFlowDbContext db,MailService mail){_sender=sender;_db=db;_mail=mail;}
    public async Task<IActionResult> Index(bool all=false)
    {
        if(!_sender.IsSimulation)return NotFound();var user=await _db.Users.Include(u=>u.UserRoles).ThenInclude(r=>r.Role).FirstOrDefaultAsync(u=>u.Id==AuthHelper.GetUserId(User)&&u.IsActive&&u.DeletedAt==null);if(user==null)return Forbid();
        ViewBag.IsAdmin=user.RoleCodes.Contains(AppRoles.Admin);ViewBag.All=all&&ViewBag.IsAdmin;
        return View(await _sender.ListAsync(ViewBag.All?null:user.Email));
    }
    public async Task<IActionResult> Message(string id)
    {
        if(!_sender.IsSimulation)return NotFound();var user=await _db.Users.Include(u=>u.UserRoles).ThenInclude(r=>r.Role).FirstOrDefaultAsync(u=>u.Id==AuthHelper.GetUserId(User)&&u.IsActive&&u.DeletedAt==null);var mail=await _sender.ReadAsync(id);
        if(user==null||mail==null||(!user.RoleCodes.Contains(AppRoles.Admin)&&!string.Equals(mail.To,user.Email,StringComparison.OrdinalIgnoreCase)))return NotFound();
        var links=System.Text.RegularExpressions.Regex.Matches(mail.Body,@"https?://[^\s]+").Select(m=>m.Value).Where(url=>_mail.PublicUrl!=null&&(url.StartsWith(_mail.PublicUrl+"/Password/Reset?token=",StringComparison.Ordinal)||url.StartsWith(_mail.PublicUrl+"/Account/VerifyEmail?token=",StringComparison.Ordinal))).ToList();ViewBag.Links=links;return View(mail);
    }
}

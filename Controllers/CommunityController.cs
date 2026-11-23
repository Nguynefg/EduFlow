using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EduFlow.Web.Controllers;
[Authorize, AutoValidateAntiforgeryToken]
public class CommunityController : Controller
{
    private readonly CommunityService _community;
    private readonly EduFlowDbContext _db;
    public CommunityController(CommunityService community,EduFlowDbContext db){_community=community;_db=db;}
    private long UserId=>AuthHelper.GetUserId(User);
    private void Feedback(string? error)=>TempData[error==null?"Success":"Error"]=error??"Đã lưu thay đổi.";
    public async Task<IActionResult> Lesson(long id,string? q,int qaPage=1)
    {
        var model=await _community.DiscussionsAsync(UserId,id,q,qaPage);return model==null?NotFound():View(model);
    }
    [Authorize(Roles=AppRoles.Instructor+","+AppRoles.Admin)]
    public async Task<IActionResult> Manage()=>View(await _community.ManageAsync(UserId));
    [HttpPost]
    public async Task<IActionResult> Review(long courseId,int rating,string? comment)
    {
        Feedback(!ModelState.IsValid?"Dữ liệu đánh giá không hợp lệ.":await _community.ReviewAsync(UserId,courseId,rating,comment));
        return Redirect(Url.Action("Details","Courses",new{id=courseId})+"#reviews");
    }
    [HttpPost]
    public async Task<IActionResult> ReviewReply(long courseId,long reviewerId,string? reply)
    {
        Feedback(!ModelState.IsValid?"Dữ liệu phản hồi không hợp lệ.":await _community.ReviewReplyAsync(UserId,courseId,reviewerId,reply));
        return Redirect(Url.Action("Details","Courses",new{id=courseId})+"#reviews");
    }
    private async Task<IActionResult> Back(long lessonId,bool inLearn,string? q,int qaPage)
    {
        var courseId=await _db.Lessons.Where(l=>l.Id==lessonId).Select(l=>(long?)l.Section!.CourseId).FirstOrDefaultAsync();
        if(courseId==null)return NotFound();
        return Redirect((inLearn?Url.Action("Learn","Learning",new{id=courseId,lessonId,q,qaPage}):Url.Action("Lesson","Community",new{id=lessonId,q,qaPage}))+"#qa");
    }
    [HttpPost]
    public async Task<IActionResult> Post(long lessonId,string? title,string? content,long? parentId,int? timestamp,bool inLearn,string? q,int qaPage=1)
    {
        Feedback(!ModelState.IsValid?"Dữ liệu câu hỏi không hợp lệ.":await _community.PostAsync(UserId,lessonId,title,content,parentId,timestamp));return await Back(lessonId,inLearn,q,qaPage);
    }
    [HttpPost]
    public async Task<IActionResult> Vote(long lessonId,long id,bool inLearn,string? q,int qaPage=1)
    {
        Feedback(!ModelState.IsValid?"Dữ liệu bình chọn không hợp lệ.":await _community.VoteAsync(UserId,lessonId,id));return await Back(lessonId,inLearn,q,qaPage);
    }
    [HttpPost]
    public async Task<IActionResult> Moderate(long lessonId,long id,string action,bool inLearn,string? q,int qaPage=1)
    {
        Feedback(!ModelState.IsValid?"Dữ liệu thao tác không hợp lệ.":await _community.ModerateAsync(UserId,lessonId,id,action));return await Back(lessonId,inLearn,q,qaPage);
    }
}

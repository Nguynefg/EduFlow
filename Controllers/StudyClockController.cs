using EduFlow.Web.Helpers;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EduFlow.Web.Controllers;
[Authorize,AutoValidateAntiforgeryToken]
public class StudyClockController:Controller
{
    private readonly StudyClockService _clock;public StudyClockController(StudyClockService clock){_clock=clock;}
    [HttpPost]public async Task<IActionResult> Start(long lessonId){var id=await _clock.StartAsync(AuthHelper.GetUserId(User),lessonId);return id.HasValue?Json(new{session=id}):BadRequest();}
    [HttpPost]public async Task<IActionResult> Tick(Guid session,bool active){var seconds=await _clock.TickAsync(AuthHelper.GetUserId(User),session,active);return seconds.HasValue?Json(new{seconds}):BadRequest();}
}

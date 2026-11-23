using EduFlow.Web.Models;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Areas.Admin.Controllers;

// Dùng chung xử lý để form quản trị và giảng viên có cùng kiểm tra dữ liệu.
[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class InstructorController : EduFlow.Web.Controllers.InstructorController
{
    public InstructorController(InstructorCourseService courseService, IWebHostEnvironment env, MediaLibraryService media) : base(courseService, env, media) { }
}

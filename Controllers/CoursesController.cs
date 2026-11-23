using EduFlow.Web.Helpers;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;

// Trang khóa học cho khách và học viên
public class CoursesController : Controller
{
    private readonly CommunityService _community;
    private readonly CourseService _courseService;
    private readonly CommerceService _commerce;
    private readonly LearningService _learningService;

    public CoursesController(CourseService courseService, LearningService learningService, CommerceService commerce, CommunityService community)
    {
        _courseService = courseService;
        _learningService = learningService;
        _commerce = commerce;
        _community = community;
    }

    private long? CurrentUserId => User.Identity?.IsAuthenticated == true ? AuthHelper.GetUserId(User) : null;

    // Danh sách + tìm kiếm + lọc + sắp xếp + phân trang
    public async Task<IActionResult> Index(string? q, int? categoryId, List<string> level, string? price, string? sort, int page = 1, decimal? minRating = null)
    {
        return View("Index",await _courseService.SearchAsync(q, categoryId, level, price, sort, page, minRating));
    }

    public async Task<IActionResult> BySlug(string slug,int reviewPage=1){var id=await _courseService.IdBySlugAsync(slug);return id.HasValue?await Details(id.Value,reviewPage):NotFound();}
    public async Task<IActionResult> Category(string slug,string? q,List<string> level,string? price,string? sort,int page=1,decimal? minRating=null){var id=await _courseService.CategoryBySlugAsync(slug);return id.HasValue?await Index(q,id,level,price,sort,page,minRating):NotFound();}

    // Chi tiết khóa học (đã mua thì hiện tiến độ từng bài)
    public async Task<IActionResult> Details(long id, int reviewPage = 1)
    {
        var model = await _learningService.GetCourseDetailAsync(id, CurrentUserId);
        if (model == null) return NotFound();
        model.Reviews = await _community.ReviewsAsync(model.Course, CurrentUserId, reviewPage);
        return View("Details",model);
    }

    // Mua / đăng ký khóa học rồi vào học ngay
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(long id)
    {
        var error = await _commerce.AddAsync(AuthHelper.GetUserId(User), id);
        if (error != null) TempData["Error"] = error;
        return RedirectToAction("Index", "Cart");
    }

    // Xem thử bài học được đánh dấu "học thử miễn phí"
    public async Task<IActionResult> Preview(long id)
    {
        var model = await _courseService.GetPreviewLessonAsync(id);
        if (model == null) return NotFound();
        return View(model);
    }
}

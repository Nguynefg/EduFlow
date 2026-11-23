using EduFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Components;

// Menu "Khóa học" thả xuống (mega menu) trên thanh đầu trang
public class CategoryMenuViewComponent : ViewComponent
{
    private readonly CourseService _courseService;

    public CategoryMenuViewComponent(CourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View(await _courseService.GetMegaMenuAsync());
    }
}

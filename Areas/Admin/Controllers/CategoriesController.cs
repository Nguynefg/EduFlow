using EduFlow.Web.Areas.Admin.Models;
using EduFlow.Web.Areas.Admin.Services;
using EduFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Areas.Admin.Controllers;

// Admin quản lý danh mục khóa học
[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class CategoriesController : Controller
{
    private readonly AdminCategoryService _categoryService;

    public CategoriesController(AdminCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminCategoriesViewModel
        {
            Categories = await _categoryService.ListAsync(),
            CourseCounts = await _categoryService.CountCoursesAsync()
        };
        return View(model);
    }

    // Thêm (id rỗng) hoặc sửa danh mục
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int? id, string name, int? parentId, bool isActive = true)
    {
        var error = await _categoryService.SaveAsync(id, name, parentId, isActive);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = id == null ? "Đã thêm danh mục." : "Đã cập nhật danh mục.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var error = await _categoryService.DeleteAsync(id);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = "Đã xóa danh mục.";
        return RedirectToAction("Index");
    }
}

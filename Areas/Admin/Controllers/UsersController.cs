using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using EduFlow.Web.Areas.Admin.Models;
using EduFlow.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Areas.Admin.Controllers;

// Admin quản lý người dùng: xem, tìm kiếm, thêm, sửa, phân quyền, khóa, xóa mềm, khôi phục
[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class UsersController : Controller
{
    private readonly AdminUserService _adminUserService;

    public UsersController(AdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    // ===== DANH SÁCH =====
    public async Task<IActionResult> Index(string? q, string? role, string? status, bool deleted = false, int page = 1)
    {
        var model = await _adminUserService.SearchAsync(q, role, status, deleted, page);
        model.CurrentUserId = AuthHelper.GetUserId(User);
        return View(model);
    }

    // ===== THÊM =====
    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new AdminUserFormViewModel { Roles = new List<string> { AppRoles.Student } });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserFormViewModel model)
    {
        model.Id = null;
        if (!ModelState.IsValid) return View("Form", model);

        var error = await _adminUserService.CreateAsync(model);
        if (error != null)
        {
            ModelState.AddModelError("", error);
            return View("Form", model);
        }

        TempData["Success"] = "Đã tạo tài khoản " + model.Email;
        return RedirectToAction("Index");
    }

    // ===== SỬA =====
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var model = await _adminUserService.GetFormAsync(id);
        if (model == null) return NotFound();
        return View("Form", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, AdminUserFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid) return View("Form", model);

        var error = await _adminUserService.UpdateAsync(id, model, AuthHelper.GetUserId(User));
        if (error != null)
        {
            ModelState.AddModelError("", error);
            return View("Form", model);
        }

        TempData["Success"] = "Đã cập nhật tài khoản " + model.Email;
        return RedirectToAction("Index");
    }

    // ===== KHÓA / MỞ KHÓA =====
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(long id, bool active)
    {
        var error = await _adminUserService.SetActiveAsync(id, active, AuthHelper.GetUserId(User));
        ShowResult(error, active ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.");
        return RedirectToAction("Index");
    }

    // ===== XÓA MỀM =====
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var error = await _adminUserService.DeleteAsync(id, AuthHelper.GetUserId(User));
        ShowResult(error, "Đã xóa mềm tài khoản. Dữ liệu vẫn được giữ lại.");
        return RedirectToAction("Index");
    }

    // ===== KHÔI PHỤC =====
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(long id)
    {
        var error = await _adminUserService.RestoreAsync(id);
        ShowResult(error, "Đã khôi phục tài khoản.");
        return RedirectToAction("Index", new { deleted = true });
    }

    // Hiện thông báo thành công hoặc lỗi ở trang tiếp theo
    private void ShowResult(string? error, string successMessage)
    {
        if (error != null)
            TempData["Error"] = error;
        else
            TempData["Success"] = successMessage;
    }
}

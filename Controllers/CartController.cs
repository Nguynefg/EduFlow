using EduFlow.Web.Helpers;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;
[Authorize]
public class CartController : Controller
{
    private readonly CommerceService _commerce;
    public CartController(CommerceService commerce)
    {
        _commerce = commerce;
    }

    long UserId => AuthHelper.GetUserId(User);

    public async Task<IActionResult> Index(string? coupon, List<long>? selectedIds, bool selectionApplied = false)
    {
        return View(await _commerce.GetCartAsync(UserId, coupon, selectionApplied ? selectedIds ?? new List<long>() : null));
    }

    // Tính lại tổng tiền và coupon theo các khóa đã đánh dấu.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(string? coupon, List<long> selectedIds)
    {
        return View("Index", await _commerce.GetCartAsync(UserId, coupon, selectedIds));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(long id)
    {
        var error = await _commerce.AddAsync(UserId, id);
        if (error != null)
            TempData["Error"] = error;
        else
            TempData["Success"] = "Đã thêm khóa học vào giỏ.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(long id)
    {
        await _commerce.RemoveAsync(UserId, id);
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(string? coupon, List<long> selectedIds)
    {
        var(id, error) = await _commerce.CheckoutAsync(UserId, coupon, selectedIds);
        if (error != null)
        {
            TempData["Error"] = error;
            return View("Index", await _commerce.GetCartAsync(UserId, coupon, selectedIds));
        }

        var order = await _commerce.GetOrderAsync(UserId, id!.Value);
        if (order!.Order.Status == EduFlow.Web.Models.OrderStatus.Pending && order.DemoEnabled)
            return RedirectToAction("DemoQr", "Orders", new { id });
        return RedirectToAction("Details", "Orders", new { id });
    }
}

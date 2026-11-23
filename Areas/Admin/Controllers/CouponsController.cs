using EduFlow.Web.Models;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles = AppRoles.Admin)]
public class CouponsController : Controller
{
    private readonly EduFlowDbContext _db;
    private readonly FinanceService _finance;
    public CouponsController(EduFlowDbContext db, FinanceService finance)
    {
        _db = db;
        _finance = finance;
    }

    public async Task<IActionResult> Index() => View(await _db.Coupons.OrderByDescending(coupon => coupon.Id).ToListAsync());
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        var coupon = id == null ? new Coupon() : await _db.Coupons.FindAsync(id.Value);
        return coupon == null ? NotFound() : View(coupon);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Coupon model)
    {
        if (!ModelState.IsValid)
            return View(model);
        var error = await _finance.SaveCouponAsync(model);
        if (error != null)
        {
            ModelState.AddModelError("", error);
            return View(model);
        }

        TempData["Success"] = "Đã lưu mã giảm giá.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await _db.Coupons.Where(coupon => coupon.Id == id).ExecuteUpdateAsync(s => s.SetProperty(coupon => coupon.IsActive, coupon => !coupon.IsActive));
        return RedirectToAction("Index");
    }
}

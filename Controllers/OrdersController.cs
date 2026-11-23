using EduFlow.Web.Helpers;
using QRCoder;
using System.Globalization;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;
[Authorize]
public class OrdersController : Controller
{
    private readonly CommerceService _commerce;
    private readonly PaymentGatewayService _gateways;
    public OrdersController(CommerceService commerce, PaymentGatewayService gateways)
    {
        _commerce = commerce;
        _gateways = gateways;
    }

    long UserId => AuthHelper.GetUserId(User);

    public async Task<IActionResult> Index() => View(await _commerce.GetOrdersAsync(UserId));
    public async Task<IActionResult> Details(long id)
    {
        var model = await _commerce.GetOrderAsync(UserId, id);
        if (model == null) return NotFound();
        model.DemoEnabled = _gateways.DemoEnabled;
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id, bool returnToCart = false)
    {
        var error = await _commerce.CancelAsync(UserId, id);
        TempData[error == null ? "Success" : "Error"] = error ?? "Đã hủy đơn.";
        if (returnToCart) return RedirectToAction("Index", "Cart");
        return RedirectToAction("Details", new { id });
    }

    // Chỉ kiểm tra đơn của chính người đang đăng nhập.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckPayment(long id)
    {
        var model = await _commerce.GetOrderAsync(UserId, id);
        if (model == null) return NotFound();
        var payment = model.Order.Payments.FirstOrDefault(payment => payment.Status == "pending");
        if (payment == null) return RedirectToAction("Details", new { id });
        try
        {
            var result = await _gateways.CheckStatusAsync(payment);
            if (result == null)
            {
                TempData["Error"] = "Cổng thanh toán chưa có kết quả cuối cùng. Vui lòng kiểm tra lại sau.";
            }
            else
            {
                var error = await _commerce.ConfirmPaymentAsync(result);
                TempData[error == null ? "Success" : "Error"] = error == null ? "Đã đồng bộ trạng thái từ cổng thanh toán." : "Chưa đối soát được số tiền hoặc trạng thái. Vui lòng liên hệ hỗ trợ.";
            }
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException or InvalidOperationException or System.Text.Json.JsonException or KeyNotFoundException)
        {
            TempData["Error"] = "Không kết nối được cổng thanh toán. Đơn vẫn giữ nguyên, hãy thử lại sau.";
        }
        return RedirectToAction("Details", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoPay(long id)
    {
        if (!_gateways.DemoEnabled) return NotFound();
        var error = await _commerce.SimulatePaymentAsync(UserId, id);
        TempData[error == null ? "Success" : "Error"] = error ?? "Thanh toán mô phỏng thành công — không trừ tiền thật. Bạn đã có thể vào học.";
        return RedirectToAction("Details", new { id });
    }

    // Trang QR chỉ phục vụ mô phỏng, không chứa thông tin chuyển khoản ngân hàng.
    [HttpGet]
    public async Task<IActionResult> DemoQr(long id)
    {
        if (!_gateways.DemoEnabled) return NotFound();
        var model = await _commerce.GetOrderAsync(UserId, id);
        if (model == null) return NotFound();
        if (!model.CanPay || model.Order.Payments.Count > 0)
            return RedirectToAction("Details", new { id });
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DemoQrImage(long id)
    {
        if (!_gateways.DemoEnabled) return NotFound();
        var model = await _commerce.GetOrderAsync(UserId, id);
        if (model == null || !model.CanPay || model.Order.Payments.Count > 0) return NotFound();

        // Đây là văn bản demo, không phải VietQR hay đường dẫn thanh toán thật.
        var content = "EDUFLOW-DEMO|ORDER=" + model.Order.Id
            + "|AMOUNT=" + model.Order.TotalPrice.ToString("0", CultureInfo.InvariantCulture)
            + "|CURRENCY=VND|NO_REAL_PAYMENT";
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qr = new PngByteQRCode(data);
        Response.Headers.CacheControl = "no-store";
        return File(qr.GetGraphic(8), "image/png");
    }
}

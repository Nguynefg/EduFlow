using System.Text.Json;
using EduFlow.Web.Helpers;
using EduFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Web.Controllers;
public class PaymentController : Controller
{
    private readonly CommerceService _commerce;
    private readonly PaymentGatewayService _gateways;
    private readonly ILogger<PaymentController> _logger;
    public PaymentController(CommerceService commerce, PaymentGatewayService gateways, ILogger<PaymentController> logger)
    {
        _commerce = commerce;
        _gateways = gateways;
        _logger = logger;
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(long id, string gateway)
    {
        var(payment, error) = await _commerce.CreatePaymentAsync(AuthHelper.GetUserId(User), id, gateway);
        if (error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction("Details", "Orders", new { id });
        }

        try
        {
            var url = await _gateways.CreateUrlAsync(payment!, HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1");
            await _commerce.SavePaymentUrlAsync(payment!.Id, url);
            return Redirect(url);
        }
        catch (Exception ex)when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException or JsonException or KeyNotFoundException)
        {
            _logger.LogWarning("Payment initiation uncertain for order {OrderId}: {ErrorType}", id, ex.GetType().Name);
            TempData["Error"] = "Chưa nhận được liên kết thanh toán. Giao dịch vẫn chờ đối soát để tránh thu tiền hai lần. Liên hệ quản trị viên với mã đơn hàng.";
            return RedirectToAction("Details", "Orders", new { id });
        }
    }

    // Trang quay về chỉ hiển thị kết quả. Chỉ IPN có chữ ký hợp lệ mới được cấp quyền học.
    [HttpGet, ActionName("Return")]
    public IActionResult GatewayReturn()
    {
        TempData["Success"] = "Đã trở về từ cổng thanh toán. Trạng thái chỉ cập nhật khi hệ thống nhận xác nhận hợp lệ; tải lại chi tiết đơn để kiểm tra.";
        return RedirectToAction("Index", "Orders");
    }

    [HttpGet]
    public async Task<IActionResult> VnpayIpn()
    {
        var result = _gateways.VerifyVnp(Request.Query);
        if (result == null)
            return Json(new { RspCode = "97", Message = "Invalid signature" });
        try
        {
            var error = await _commerce.ConfirmPaymentAsync(result, reportDuplicate: true);
            var code = error switch
            {
                null => "00",
                "already_confirmed" => "02",
                "not_found" => "01",
                "invalid_amount" => "04",
                _ => "99"
            };
            Response.Headers.CacheControl = "no-store";
            return Json(new { RspCode = code, Message = error ?? "Confirmed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VNPay callback processing failed");
            return Json(new { RspCode = "99", Message = "Retry later" });
        }
    }

    [HttpPost, IgnoreAntiforgeryToken, RequestSizeLimit(65536)]
    public async Task<IActionResult> MomoIpn([FromBody] JsonElement body)
    {
        VerifiedPayment? result;
        try
        {
            result = _gateways.VerifyMomo(body);
        }
        catch (Exception error)when (error is JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
        {
            return BadRequest();
        }

        if (result == null)
            return BadRequest();
        try
        {
            var error = await _commerce.ConfirmPaymentAsync(result);
            return error == null ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoMo callback processing failed");
            return StatusCode(503);
        }
    }

    [HttpPost, IgnoreAntiforgeryToken, RequestSizeLimit(65536)]
    public async Task<IActionResult> ZaloIpn([FromBody] JsonElement body)
    {
        VerifiedPayment? result;
        try
        {
            result = _gateways.VerifyZalo(body);
        }
        catch (Exception error)when (error is JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
        {
            return Json(new { return_code = -1, return_message = "Invalid data" });
        }

        if (result == null)
            return Json(new { return_code = -1, return_message = "Invalid signature" });
        try
        {
            var error = await _commerce.ConfirmPaymentAsync(result);
            return Json(new { return_code = error == null ? 1 : 0, return_message = error ?? "Success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ZaloPay callback processing failed");
            return Json(new { return_code = 0, return_message = "Retry later" });
        }
    }
}

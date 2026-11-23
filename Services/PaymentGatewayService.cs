using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EduFlow.Web.Models;

namespace EduFlow.Web.Services;
public record VerifiedPayment(string Gateway, string Reference, decimal Amount, bool Success, bool IsFinal = true);
// Chỉ xử lý dữ liệu và chữ ký theo tài liệu của từng cổng thanh toán.
public class PaymentGatewayService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpClientFactory _clients;
    public PaymentGatewayService(IConfiguration config, IHttpClientFactory clients, IWebHostEnvironment environment)
    {
        _config = config;
        _environment = environment;
        _clients = clients;
    }

    string GetSetting(string gateway, string key) => _config[$"Payments:{gateway}:{key}"] ?? "";
    // Chỉ cho mô phỏng trên máy phát triển và phải được bật trong cấu hình.
    public bool IsLive => string.Equals(_config["Payments:Mode"], "Live", StringComparison.OrdinalIgnoreCase);
    public bool LiveEnabled => IsLive && _environment.IsProduction()
        && _config.GetValue<bool>("Payments:LiveEnabled")
        && _config.GetValue<bool>("Payments:LegacyDataReviewed");
    public bool DemoEnabled => !IsLive && _environment.IsDevelopment() && _config.GetValue<bool>("Payments:EnableDemo");
    public string PublicUrl => _config["Payments:PublicBaseUrl"]?.TrimEnd('/') ?? "";

    public List<string> GetAvailableGateways() => DemoEnabled ? new List<string>() : new[] { "vnpay", "momo", "zalopay" }.Where(g => IsConfigured(g)).ToList();

    private bool HasCredentials(string gateway)
    {
        var keys = gateway switch
        {
            "vnpay" => new[] { "TmnCode", "HashSecret" },
            "momo" => new[] { "PartnerCode", "AccessKey", "SecretKey" },
            "zalopay" => new[] { "AppId", "Key1", "Key2" },
            _ => Array.Empty<string>()
        };
        return keys.Length > 0 && keys.All(k => !string.IsNullOrWhiteSpace(GetSetting(gateway, k)));
    }

    private bool IsConfigured(string gateway, bool newPayment = true)
    {
        if (newPayment && DemoEnabled) return false;
        var mode = _config["Payments:Mode"];
        if (!string.Equals(mode, "Sandbox", StringComparison.OrdinalIgnoreCase) && !IsLive) return false;
        if (!HasCredentials(gateway)) return false;
        if (!Uri.TryCreate(PublicUrl, UriKind.Absolute, out var site) || site.Scheme != "https"
            || site.IsLoopback || site.UserInfo.Length > 0 || site.Query.Length > 0 || site.Fragment.Length > 0) return false;
        if (newPayment && (!_config.GetValue<bool>($"Payments:{gateway}:Enabled") || IsLive && !LiveEnabled)) return false;
        // Triển khai live VNPay trước. Hai cổng còn lại vẫn dùng sandbox.
        if (IsLive && gateway != "vnpay") return false;
        if (!Uri.TryCreate(GetSetting(gateway, "Endpoint"), UriKind.Absolute, out var endpoint) || endpoint.Scheme != "https") return false;
        if (gateway == "vnpay")
        {
            var expectedHost = IsLive ? "pay.vnpay.vn" : "sandbox.vnpayment.vn";
            if (!string.Equals(endpoint.Host, expectedHost, StringComparison.OrdinalIgnoreCase) || endpoint.UserInfo.Length > 0) return false;
            if (!Uri.TryCreate(GetSetting(gateway, "QueryEndpoint"), UriKind.Absolute, out var query)
                || query.Scheme != "https" || !string.Equals(query.Host, expectedHost, StringComparison.OrdinalIgnoreCase)
                || query.UserInfo.Length > 0) return false;
        }
        return true;
    }
    public static string Sign(string key, string data, bool sha512 = false) => Convert.ToHexString(sha512 ? HMACSHA512.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(data)) : HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    public static bool Equal(string expected, string received)
    {
        try
        {
            return expected.Length > 0 && CryptographicOperations.FixedTimeEquals(Convert.FromHexString(expected), Convert.FromHexString(received));
        }
        catch (FormatException)
        {
            return false;
        }
    }

    static string FormatAmount(decimal n) => n.ToString("0", CultureInfo.InvariantCulture);
    public static string VnpQuery(IEnumerable<KeyValuePair<string, string>> values) => string.Join("&", values.Where(x => !string.IsNullOrEmpty(x.Value)).OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
    // Mỗi cổng có định dạng yêu cầu riêng; không lưu secret vào đơn hàng.
    public async Task<string> CreateUrlAsync(Payment payment, string ip)
    {
        if (!IsConfigured(payment.Gateway))
            throw new InvalidOperationException("Cổng thanh toán chưa được cấu hình đầy đủ.");
        if (payment.Amount <= 0 || payment.Amount != decimal.Truncate(payment.Amount))
            throw new InvalidOperationException("Số tiền phải là số nguyên VND dương.");
        var gateway = payment.Gateway;
        var reference = payment.GatewayTxnId!;
        var amount = FormatAmount(payment.Amount);
        if (gateway == "vnpay")
        {
            var now = new DateTimeOffset(payment.CreatedAt).ToOffset(TimeSpan.FromHours(7));
            var fields = new Dictionary<string, string>
            {
                {
                    "vnp_Version",
                    "2.1.0"
                },
                {
                    "vnp_Command",
                    "pay"
                },
                {
                    "vnp_TmnCode",
                    GetSetting(gateway, "TmnCode")
                },
                {
                    "vnp_Amount",
                    FormatAmount(payment.Amount * 100)
                },
                {
                    "vnp_CreateDate",
                    now.ToString("yyyyMMddHHmmss")
                },
                {
                    "vnp_ExpireDate",
                    now.AddMinutes(15).ToString("yyyyMMddHHmmss")
                },
                {
                    "vnp_CurrCode",
                    "VND"
                },
                {
                    "vnp_IpAddr",
                    ip
                },
                {
                    "vnp_Locale",
                    "vn"
                },
                {
                    "vnp_OrderInfo",
                    "Thanh toan EduFlow " + payment.OrderId
                },
                {
                    "vnp_OrderType",
                    "other"
                },
                {
                    "vnp_ReturnUrl",
                    PublicUrl + "/Payment/Return"
                },
                {
                    "vnp_TxnRef",
                    reference
                }
            };
            var query = VnpQuery(fields);
            return GetSetting(gateway, "Endpoint") + "?" + query + "&vnp_SecureHash=" + Sign(GetSetting(gateway, "HashSecret"), query, true);
        }

        if (gateway == "momo")
        {
            var fields = new Dictionary<string, string>
            {
                {
                    "accessKey",
                    GetSetting(gateway, "AccessKey")
                },
                {
                    "amount",
                    amount
                },
                {
                    "extraData",
                    ""
                },
                {
                    "ipnUrl",
                    PublicUrl + "/Payment/MomoIpn"
                },
                {
                    "orderId",
                    reference
                },
                {
                    "orderInfo",
                    "Thanh toan EduFlow " + payment.OrderId
                },
                {
                    "partnerCode",
                    GetSetting(gateway, "PartnerCode")
                },
                {
                    "redirectUrl",
                    PublicUrl + "/Payment/Return"
                },
                {
                    "requestId",
                    reference
                },
                {
                    "requestType",
                    "captureWallet"
                }
            };
            var raw = string.Join("&", fields.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => x.Key + "=" + x.Value));
            var payload = new Dictionary<string, object>
            {
                {
                    "partnerCode",
                    fields["partnerCode"]
                },
                {
                    "amount",
                    (long)payment.Amount
                },
                {
                    "extraData",
                    ""
                },
                {
                    "ipnUrl",
                    fields["ipnUrl"]
                },
                {
                    "orderId",
                    reference
                },
                {
                    "orderInfo",
                    fields["orderInfo"]
                },
                {
                    "redirectUrl",
                    fields["redirectUrl"]
                },
                {
                    "requestId",
                    reference
                },
                {
                    "requestType",
                    "captureWallet"
                },
                {
                    "lang",
                    "vi"
                },
                {
                    "autoCapture",
                    true
                },
                {
                    "signature",
                    Sign(GetSetting(gateway, "SecretKey"), raw)
                }
            };
            using var response = await _clients.CreateClient("payments").PostAsJsonAsync(GetSetting(gateway, "Endpoint"), payload);
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (json.RootElement.GetProperty("resultCode").GetInt32() != 0)
                throw new InvalidOperationException("MoMo chưa chấp nhận giao dịch. Kiểm tra cấu hình merchant và nhật ký cổng thanh toán.");
            return SafeRedirect(json.RootElement.GetProperty("payUrl").GetString());
        }

        var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
        var embed = JsonSerializer.Serialize(new { redirecturl = PublicUrl + "/Payment/Return" });
        var user = "eduflow";
        var item = "[]";
        var mac = Sign(GetSetting(gateway, "Key1"), string.Join("|", GetSetting(gateway, "AppId"), reference, user, amount, time, embed, item));
        var form = new Dictionary<string, string>
        {
            {
                "app_id",
                GetSetting(gateway, "AppId")
            },
            {
                "app_user",
                user
            },
            {
                "app_trans_id",
                reference
            },
            {
                "app_time",
                time
            },
            {
                "amount",
                amount
            },
            {
                "item",
                item
            },
            {
                "embed_data",
                embed
            },
            {
                "description",
                "Thanh toan EduFlow " + payment.OrderId
            },
            {
                "bank_code",
                ""
            },
            {
                "callback_url",
                PublicUrl + "/Payment/ZaloIpn"
            },
            {
                "mac",
                mac
            },
            {
                "expire_duration_seconds",
                "900"
            }
        };
        using var zaloResponse = await _clients.CreateClient("payments").PostAsync(GetSetting(gateway, "Endpoint"), new FormUrlEncodedContent(form));
        zaloResponse.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await zaloResponse.Content.ReadAsStringAsync());
        if (document.RootElement.GetProperty("return_code").GetInt32() != 1)
            throw new InvalidOperationException("ZaloPay chưa chấp nhận giao dịch. Kiểm tra cấu hình merchant.");
        return SafeRedirect(document.RootElement.GetProperty("order_url").GetString());
    }

    static string SafeRedirect(string? url) => Uri.TryCreate(url, UriKind.Absolute, out var u) && u.Scheme == "https" ? url! : throw new InvalidOperationException("Cổng thanh toán trả về URL không hợp lệ.");
    // Không tin các tham số callback nếu chữ ký hoặc mã merchant không đúng.
    public VerifiedPayment? VerifyVnp(IQueryCollection query)
    {
        if (!HasCredentials("vnpay") || query.Any(x => x.Value.Count != 1))
            return null;
        var values = query.Where(x => x.Key.StartsWith("vnp_", StringComparison.Ordinal) && x.Key is not "vnp_SecureHash" and not "vnp_SecureHashType").ToDictionary(x => x.Key, x => x.Value.ToString());
        if (!Equal(Sign(GetSetting("vnpay", "HashSecret"), VnpQuery(values), true), query["vnp_SecureHash"].ToString()) || query["vnp_TmnCode"] != GetSetting("vnpay", "TmnCode"))
            return null;
        if (!decimal.TryParse(query["vnp_Amount"], NumberStyles.None, CultureInfo.InvariantCulture, out var amount))
            return null;
        var reference = query["vnp_TxnRef"].ToString();
        var response = query["vnp_ResponseCode"].ToString();
        var status = query["vnp_TransactionStatus"].ToString();
        if (string.IsNullOrWhiteSpace(reference) || amount <= 0 || amount % 100 != 0
            || response.Length != 2 || status.Length != 2) return null;
        // Chờ/xác minh thêm không được coi là thất bại cuối cùng.
        return new("vnpay", reference, amount / 100, response == "00" && status == "00", status is "00" or "02");
    }

    public VerifiedPayment? VerifyMomo(JsonElement body)
    {
        if (!HasCredentials("momo"))
            return null;
        string GetValue(string k) => body.TryGetProperty(k, out var v) ? v.ToString() : "";
        var keys = new[]
        {
            "amount",
            "extraData",
            "message",
            "orderId",
            "orderInfo",
            "orderType",
            "partnerCode",
            "payType",
            "requestId",
            "responseTime",
            "resultCode",
            "transId"
        };
        var raw = "accessKey=" + GetSetting("momo", "AccessKey") + "&" + string.Join("&", keys.Select(k => k + "=" + GetValue(k)));
        if (GetValue("partnerCode") != GetSetting("momo", "PartnerCode") || GetValue("requestId") != GetValue("orderId") || !Equal(Sign(GetSetting("momo", "SecretKey"), raw), GetValue("signature")))
            return null;
        if (!decimal.TryParse(GetValue("amount"), NumberStyles.None, CultureInfo.InvariantCulture, out var amount))
            return null;
        return new("momo", GetValue("orderId"), amount, GetValue("resultCode") == "0", GetValue("resultCode")is not ("1000" or "7000" or "7002" or "9000"));
    }

    public VerifiedPayment? VerifyZalo(JsonElement body)
    {
        if (!HasCredentials("zalopay") || !body.TryGetProperty("data", out var data) || !body.TryGetProperty("mac", out var mac))
            return null;
        var text = data.GetString() ?? "";
        if (!Equal(Sign(GetSetting("zalopay", "Key2"), text), mac.GetString() ?? ""))
            return null;
        using var parsed = JsonDocument.Parse(text);
        var callbackData = parsed.RootElement;
        if (callbackData.GetProperty("app_id").ToString() != GetSetting("zalopay", "AppId"))
            return null;
        return new("zalopay", callbackData.GetProperty("app_trans_id").GetString()!, callbackData.GetProperty("amount").GetDecimal(), true);
    }

    // Hỏi trực tiếp cổng thanh toán khi callback đến chậm hoặc người dùng đóng trang.
    // Không nhận trạng thái do trình duyệt tự gửi lên.
    public async Task<VerifiedPayment?> CheckStatusAsync(Payment payment)
    {
        if (DemoEnabled) return null;
        if (payment.IsTest == IsLive || !IsConfigured(payment.Gateway, false)) return null;
        var reference = payment.GatewayTxnId!;
        var gateway = payment.Gateway;
        if (gateway == "vnpay")
        {
            var fields = new Dictionary<string, string>
            {
                ["vnp_RequestId"] = Guid.NewGuid().ToString("N"),
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "querydr",
                ["vnp_TmnCode"] = GetSetting(gateway, "TmnCode"),
                ["vnp_TxnRef"] = reference,
                ["vnp_TransactionDate"] = new DateTimeOffset(payment.CreatedAt).ToOffset(TimeSpan.FromHours(7)).ToString("yyyyMMddHHmmss"),
                ["vnp_CreateDate"] = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).ToString("yyyyMMddHHmmss"),
                ["vnp_IpAddr"] = "127.0.0.1",
                ["vnp_OrderInfo"] = "Doi soat EduFlow " + payment.OrderId
            };
            fields["vnp_SecureHash"] = Sign(GetSetting(gateway, "HashSecret"), string.Join("|", fields.Values), true);
            using var response = await _clients.CreateClient("payments").PostAsJsonAsync(GetSetting(gateway, "QueryEndpoint"), fields);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            string Value(string key) => document.RootElement.TryGetProperty(key, out var value) ? value.ToString() : "";
            var signedKeys = new[] { "vnp_ResponseId", "vnp_Command", "vnp_ResponseCode", "vnp_Message", "vnp_TmnCode", "vnp_TxnRef", "vnp_Amount", "vnp_BankCode", "vnp_PayDate", "vnp_TransactionNo", "vnp_TransactionType", "vnp_TransactionStatus", "vnp_OrderInfo", "vnp_PromotionCode", "vnp_PromotionAmount" };
            var expected = Sign(GetSetting(gateway, "HashSecret"), string.Join("|", signedKeys.Select(Value)), true);
            if (!Equal(expected, Value("vnp_SecureHash")) || Value("vnp_TmnCode") != fields["vnp_TmnCode"] || Value("vnp_TxnRef") != reference || Value("vnp_Command") != "querydr") return null;
            if (Value("vnp_ResponseCode") != "00" || !decimal.TryParse(Value("vnp_Amount"), NumberStyles.None, CultureInfo.InvariantCulture, out var amount)) return null;
            var status = Value("vnp_TransactionStatus");
            return new(gateway, reference, amount / 100, status == "00", status is "00" or "02");
        }
        if (gateway == "momo")
        {
            var requestId = Guid.NewGuid().ToString("N");
            var partner = GetSetting(gateway, "PartnerCode");
            var raw = "accessKey=" + GetSetting(gateway, "AccessKey") + "&orderId=" + reference + "&partnerCode=" + partner + "&requestId=" + requestId;
            var payload = new { partnerCode = partner, requestId, orderId = reference, lang = "vi", signature = Sign(GetSetting(gateway, "SecretKey"), raw) };
            using var response = await _clients.CreateClient("payments").PostAsJsonAsync(GetSetting(gateway, "QueryEndpoint"), payload);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var result = document.RootElement;
            if (result.GetProperty("orderId").GetString() != reference || result.GetProperty("requestId").GetString() != requestId || result.GetProperty("partnerCode").GetString() != partner) return null;
            var status = result.GetProperty("resultCode").GetInt32();
            if (!result.TryGetProperty("amount", out var amount)) return null;
            return new(gateway, reference, amount.GetDecimal(), status == 0, status is not (1000 or 7000 or 7002 or 9000));
        }
        var data = GetSetting(gateway, "AppId") + "|" + reference + "|" + GetSetting(gateway, "Key1");
        var query = new Dictionary<string, string>
        {
            ["app_id"] = GetSetting(gateway, "AppId"),
            ["app_trans_id"] = reference,
            ["mac"] = Sign(GetSetting(gateway, "Key1"), data)
        };
        using var zaloResponse = await _clients.CreateClient("payments").PostAsync(GetSetting(gateway, "QueryEndpoint"), new FormUrlEncodedContent(query));
        zaloResponse.EnsureSuccessStatusCode();
        using var zaloDocument = JsonDocument.Parse(await zaloResponse.Content.ReadAsStringAsync());
        var zaloResult = zaloDocument.RootElement;
        var code = zaloResult.GetProperty("return_code").GetInt32();
        if (code == 1 && zaloResult.TryGetProperty("amount", out var paidAmount)) return new(gateway, reference, paidAmount.GetDecimal(), true);
        // Chưa biết kết quả: giữ đơn chờ, không tự suy luận đã thanh toán.
        return null;
    }
}

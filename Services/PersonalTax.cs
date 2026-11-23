namespace EduFlow.Web.Services;

// Chỉ áp dụng thù lao cá nhân cư trú, không HĐLĐ hoặc HĐLĐ dưới 3 tháng.
// Khoản 2 Điều 50 NĐ 253/2026/NĐ-CP, hiệu lực 01/07/2026.
public static class PersonalTax
{
    public const decimal Threshold = 5000000m;
    public const decimal Rate = 10m;
    public const string Rule = "ND253-2026-D50-K2";
    public static (decimal Rate, decimal Tax, decimal Net) Calculate(decimal gross, DateTime paidDate, bool voluntary)
    {
        if (gross <= 0 || gross > 9999999999999m || gross != decimal.Truncate(gross))
            throw new ArgumentException("Số tiền trước thuế phải là VND nguyên dương.");
        if (paidDate.Date < new DateTime(2026, 7, 1))
            throw new ArgumentException("Luồng này chỉ hỗ trợ ngày chi trả từ 01/07/2026.");
        var rate = gross >= Threshold || voluntary ? Rate : 0m;
        var tax = Math.Round(gross * rate / 100m, 0, MidpointRounding.AwayFromZero);
        return (rate, tax, gross - tax);
    }
}

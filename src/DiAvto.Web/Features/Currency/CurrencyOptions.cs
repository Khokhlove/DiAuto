namespace DiAvto.Web.Features.Currency;

public sealed class CurrencyOptions
{
    public const string SectionName = "Currency";

    public string CbrDailyUrl { get; set; } = "https://www.cbr.ru/scripts/XML_daily.asp";
    public string BaseCurrency { get; set; } = "CNY";
    public string QuoteCurrency { get; set; } = "RUB";
    public decimal FallbackRate { get; set; } = 12.15m;
    public List<string> Currencies { get; set; } = ["CNY", "EUR", "JPY", "KRW"];
    public Dictionary<string, decimal> FallbackRates { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CNY"] = 12.15m,
        ["EUR"] = 100m,
        ["JPY"] = 0.6m,
        ["KRW"] = 0.07m
    };
    public int MaxAgeHours { get; set; } = 12;
    public int RefreshIntervalHours { get; set; } = 6;
    public int HistoryDays { get; set; } = 365;
}

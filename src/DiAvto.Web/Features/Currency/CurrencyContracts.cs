namespace DiAvto.Web.Features.Currency;

public sealed record CurrencyQuote(
    string BaseCurrency,
    string QuoteCurrency,
    decimal Rate,
    DateTime EffectiveDate,
    DateTime FetchedAt,
    string Source,
    bool IsFallback)
{
    public decimal ConvertFrom(decimal amount) => Math.Round(amount * Rate, 2, MidpointRounding.AwayFromZero);
}

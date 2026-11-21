using DiAvto.Admin.Data;
using DiAvto.Domain;
using DiAvto.Application.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DiAvto.Admin.Features.Currency;

public sealed class CurrencyService
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    private readonly DiAvtoDbContext _db;
    private readonly CbrCurrencyClient _cbr;
    private readonly CurrencyOptions _options;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(
        DiAvtoDbContext db,
        CbrCurrencyClient cbr,
        IOptions<CurrencyOptions> options,
        ILogger<CurrencyService> logger)
    {
        _db = db;
        _cbr = cbr;
        _options = options.Value;
        _logger = logger;
    }

    public Task<CurrencyQuote> GetCurrentAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        return GetCurrentAsync(_options.BaseCurrency, forceRefresh, cancellationToken);
    }

    public async Task<CurrencyQuote> GetCurrentAsync(
        string baseCurrency,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var code = Normalize(baseCurrency);
        var latest = await FindLatestAsync(code, cancellationToken);
        var isFresh = latest is not null && IsFresh(latest);

        if (!forceRefresh && isFresh)
        {
            return Map(latest!);
        }

        try
        {
            await RefreshAllAsync(cancellationToken);
            latest = await FindLatestAsync(code, cancellationToken);
            return latest is not null ? Map(latest) : FallbackQuote(code);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Не удалось обновить курс ЦБ РФ. Используем последнее известное значение.");

            if (latest is not null)
            {
                return Map(latest);
            }

            return FallbackQuote(code);
        }
    }

    /// <summary>Курсы нескольких валют за один запрос к БД и одно обновление ЦБ.</summary>
    public async Task<IReadOnlyDictionary<string, CurrencyQuote>> GetRatesAsync(
        IEnumerable<string> currencies,
        CancellationToken cancellationToken = default)
    {
        var codes = currencies
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(Normalize)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var latest = await FindLatestManyAsync(codes, cancellationToken);
        if (codes.All(code => latest.TryGetValue(code, out var rate) && IsFresh(rate)))
        {
            return codes.ToDictionary(code => code, code => Map(latest[code]), StringComparer.Ordinal);
        }

        try
        {
            await RefreshAllAsync(cancellationToken);
            latest = await FindLatestManyAsync(codes, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Не удалось обновить курсы ЦБ РФ. Используем последние известные значения.");
        }

        return codes.ToDictionary(
            code => code,
            code => latest.TryGetValue(code, out var rate) ? Map(rate) : FallbackQuote(code),
            StringComparer.Ordinal);
    }

    public async Task<CurrencyQuote> RefreshAsync(CancellationToken cancellationToken = default)
    {
        var quotes = await RefreshAllAsync(cancellationToken);
        return quotes.TryGetValue(Normalize(_options.BaseCurrency), out var quote)
            ? quote
            : FallbackQuote(_options.BaseCurrency);
    }

    public async Task<IReadOnlyList<CurrencyRate>> GetHistoryAsync(int take = 60, CancellationToken cancellationToken = default)
    {
        return await _db.CurrencyRates
            .AsNoTracking()
            .Where(x => x.BaseCurrency == _options.BaseCurrency && x.QuoteCurrency == _options.QuoteCurrency)
            .OrderByDescending(x => x.EffectiveDate)
            .ThenByDescending(x => x.FetchedAt)
            .Take(Math.Clamp(take, 1, 365))
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetMarkupPercentAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _db.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == "PriceMarkupPercent", cancellationToken);

        return setting is not null &&
               decimal.TryParse(setting.Value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var markup)
            ? markup
            : 0m;
    }

    public async Task<decimal> ConvertCnyToRubAsync(decimal amountCny, CancellationToken cancellationToken = default)
    {
        var quote = await GetCurrentAsync(cancellationToken: cancellationToken);
        var markup = await GetMarkupPercentAsync(cancellationToken);
        return CalculateRub(amountCny, quote.Rate, markup);
    }

    public static decimal CalculateRub(decimal amountCny, decimal rate, decimal markupPercent)
    {
        return PricingCalculator.CnyToRub(amountCny, rate, markupPercent);
    }

    private async Task<Dictionary<string, CurrencyQuote>> RefreshAllAsync(CancellationToken cancellationToken)
    {
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            var codes = _options.Currencies
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(Normalize)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var quotes = await _cbr.GetRatesAsync(codes, cancellationToken);
            var result = new Dictionary<string, CurrencyQuote>(StringComparer.Ordinal);

            foreach (var (code, quote) in quotes)
            {
                var existing = await _db.CurrencyRates.FirstOrDefaultAsync(
                    x => x.Source == quote.Source &&
                         x.BaseCurrency == quote.BaseCurrency &&
                         x.QuoteCurrency == quote.QuoteCurrency &&
                         x.EffectiveDate == quote.EffectiveDate,
                    cancellationToken);

                if (existing is null)
                {
                    _db.CurrencyRates.Add(new CurrencyRate
                    {
                        BaseCurrency = quote.BaseCurrency,
                        QuoteCurrency = quote.QuoteCurrency,
                        Rate = quote.Rate,
                        Source = quote.Source,
                        EffectiveDate = quote.EffectiveDate,
                        FetchedAt = quote.FetchedAt,
                        IsFallback = quote.IsFallback
                    });
                }
                else
                {
                    existing.Rate = quote.Rate;
                    existing.FetchedAt = quote.FetchedAt;
                    existing.IsFallback = quote.IsFallback;
                }

                result[code] = quote;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private async Task<CurrencyRate?> FindLatestAsync(string baseCurrency, CancellationToken cancellationToken)
    {
        return await _db.CurrencyRates
            .AsNoTracking()
            .Where(x => x.BaseCurrency == baseCurrency && x.QuoteCurrency == _options.QuoteCurrency)
            .OrderByDescending(x => x.FetchedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Dictionary<string, CurrencyRate>> FindLatestManyAsync(
        IReadOnlyCollection<string> currencies,
        CancellationToken cancellationToken)
    {
        var rows = await _db.CurrencyRates
            .AsNoTracking()
            .Where(x => currencies.Contains(x.BaseCurrency) && x.QuoteCurrency == _options.QuoteCurrency)
            .OrderByDescending(x => x.FetchedAt)
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.BaseCurrency, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
    }

    private bool IsFresh(CurrencyRate rate)
    {
        return rate.FetchedAt > DateTime.UtcNow.AddHours(-Math.Max(1, _options.MaxAgeHours));
    }

    private CurrencyQuote FallbackQuote(string baseCurrency)
    {
        var rate = _options.FallbackRates.TryGetValue(baseCurrency, out var fallback)
            ? fallback
            : _options.FallbackRate;

        return new CurrencyQuote(
            baseCurrency,
            _options.QuoteCurrency,
            rate,
            DateTime.UtcNow.Date,
            DateTime.UtcNow,
            "fallback",
            true);
    }

    private static string Normalize(string currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "CNY"
            : currency.Trim().ToUpperInvariant();
    }

    private static CurrencyQuote Map(CurrencyRate rate)
    {
        return new CurrencyQuote(
            rate.BaseCurrency,
            rate.QuoteCurrency,
            rate.Rate,
            rate.EffectiveDate,
            rate.FetchedAt,
            rate.Source,
            rate.IsFallback);
    }
}



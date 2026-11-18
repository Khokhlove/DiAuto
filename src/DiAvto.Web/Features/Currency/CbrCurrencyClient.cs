using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;

namespace DiAvto.Web.Features.Currency;

public sealed class CbrCurrencyClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CurrencyOptions _options;
    private readonly ILogger<CbrCurrencyClient> _logger;

    static CbrCurrencyClient()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public CbrCurrencyClient(
        IHttpClientFactory httpClientFactory,
        IOptions<CurrencyOptions> options,
        ILogger<CbrCurrencyClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CurrencyQuote> GetCnyRubAsync(CancellationToken cancellationToken = default)
    {
        var rates = await GetRatesAsync([_options.BaseCurrency], cancellationToken);
        return rates[_options.BaseCurrency];
    }

    public async Task<IReadOnlyDictionary<string, CurrencyQuote>> GetRatesAsync(
        IEnumerable<string> currencies,
        CancellationToken cancellationToken = default)
    {
        var requested = currencies
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var client = _httpClientFactory.CreateClient("cbr");
        using var response = await client.GetAsync(_options.CbrDailyUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        var root = document.Root ?? throw new InvalidOperationException("ЦБ РФ вернул пустой XML.");
        var date = ParseDate((string?)root.Attribute("Date"));
        var result = new Dictionary<string, CurrencyQuote>(StringComparer.Ordinal);

        foreach (var code in requested)
        {
            var valute = root.Elements("Valute")
                .FirstOrDefault(x => string.Equals((string?)x.Element("CharCode"), code, StringComparison.OrdinalIgnoreCase));

            if (valute is null)
            {
                continue;
            }

            var nominal = ParseDecimal((string?)valute.Element("Nominal")) ?? 1m;
            var value = ParseDecimal((string?)valute.Element("Value"));
            if (value is null)
            {
                continue;
            }

            var rate = value.Value / nominal;
            if (rate <= 0)
            {
                continue;
            }

            result[code] = new CurrencyQuote(
                code,
                _options.QuoteCurrency,
                decimal.Round(rate, 6, MidpointRounding.AwayFromZero),
                date,
                DateTime.UtcNow,
                "CBR",
                false);

            _logger.LogInformation("Курс ЦБ РФ: 1 {Base} = {Rate} {Quote} на {Date:yyyy-MM-dd}",
                code, rate, _options.QuoteCurrency, date);
        }

        if (result.Count == 0)
        {
            throw new InvalidOperationException("В ответе ЦБ РФ нет запрошенных валют.");
        }

        return result;
    }

    private static DateTime ParseDate(string? value)
    {
        return DateTime.TryParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.Date
            : DateTime.UtcNow.Date;
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }
}

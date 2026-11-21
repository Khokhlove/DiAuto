using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace DiAvto.Admin.Features.Marketplaces.Che168;

public sealed class Che168ApiClient : IMarketplaceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Che168Options _options;
    private readonly ILogger<Che168ApiClient> _logger;

    public Che168ApiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<Che168Options> options,
        ILogger<Che168ApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public string Key => "che168";
    public string DisplayName => "Che168 / 二手车之家";

    public async Task<IReadOnlyList<MarketplaceBrand>> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        using var document = await GetJsonAsync("/api/v2/getbrands", [], cancellationToken);
        var result = GetResult(document.RootElement, "getbrands");

        var brands = new Dictionary<string, MarketplaceBrand>(StringComparer.Ordinal);
        var order = 0;

        if (result.TryGetProperty("hotbrand", out var hotBrand) && hotBrand.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in hotBrand.EnumerateArray())
            {
                var brand = ReadBrand(item, order++);
                if (brand is not null)
                {
                    brands[brand.ExternalId] = brand;
                }
            }
        }

        if (result.TryGetProperty("brands", out var groups) && groups.ValueKind == JsonValueKind.Array)
        {
            foreach (var group in groups.EnumerateArray())
            {
                if (!group.TryGetProperty("brand", out var groupBrands) || groupBrands.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var item in groupBrands.EnumerateArray())
                {
                    var brand = ReadBrand(item, order++);
                    if (brand is not null && !brands.ContainsKey(brand.ExternalId))
                    {
                        brands[brand.ExternalId] = brand;
                    }
                }
            }
        }

        return brands.Values
            .OrderBy(x => x.SortOrder)
            .ToList();
    }

    public async Task<IReadOnlyList<MarketplaceSeries>> GetSeriesAsync(
        string brandExternalId,
        CancellationToken cancellationToken = default)
    {
        using var document = await GetJsonAsync(
            "/api/v2/getseriesbybrandid",
            [new KeyValuePair<string, string?>("brandid", brandExternalId)],
            cancellationToken);

        var result = GetResult(document.RootElement, "getseriesbybrandid");
        var series = new Dictionary<string, MarketplaceSeries>(StringComparer.Ordinal);
        var order = 0;

        if (result.TryGetProperty("list", out var groups) && groups.ValueKind == JsonValueKind.Array)
        {
            foreach (var group in groups.EnumerateArray())
            {
                if (!group.TryGetProperty("item", out var items) || items.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var item in items.EnumerateArray())
                {
                    var id = GetString(item, "sid");
                    var name = GetString(item, "sname");

                    if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    series.TryAdd(id, new MarketplaceSeries(
                        id,
                        name,
                        GetString(item, "py"),
                        NormalizeUrl(GetString(item, "icon")),
                        GetString(item, "pricerange")));

                    order++;
                }
            }
        }

        return series.Values.ToList();
    }

    public async Task<MarketplaceSearchPage> SearchAsync(
        MarketplaceSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var parameters = BuildSearchParameters(query);

        using var document = await GetJsonAsync("/api/v11/search", parameters, cancellationToken);
        var result = GetResult(document.RootElement, "search");

        var page = new MarketplaceSearchPage
        {
            TotalCount = GetInt(result, "totalcount") ?? 0,
            PageIndex = GetInt(result, "pageindex") ?? query.PageIndex,
            PageCount = GetInt(result, "pagecount") ?? 0
        };

        if (result.TryGetProperty("carlist", out var cars) && cars.ValueKind == JsonValueKind.Array)
        {
            foreach (var car in cars.EnumerateArray())
            {
                var item = ReadSearchItem(car);
                if (item is not null)
                {
                    page.Items.Add(item);
                }
            }
        }

        if (page.PageCount <= 0)
        {
            page.PageCount = 1;
        }

        return page;
    }

    public async Task<MarketplaceCarDetails> GetCarDetailsAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        using var document = await GetJsonAsync(
            "/apic/v2/car/getcarinfo",
            [
                new KeyValuePair<string, string?>("infoid", externalId),
                new KeyValuePair<string, string?>("deviceid", _options.DeviceId)
            ],
            cancellationToken,
            useDetailHost: true);

        var result = GetResult(document.RootElement, "getcarinfo");
        var details = ReadDetails(result, externalId);
        details.RawJson = document.RootElement.GetRawText();
        return details;
    }

    public async Task<MarketplaceHealth> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var brands = await GetBrandsAsync(cancellationToken);
            return new MarketplaceHealth(true, $"API отвечает, брендов: {brands.Count}.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new MarketplaceHealth(false, ex.Message);
        }
    }

    private List<KeyValuePair<string, string?>> BuildSearchParameters(MarketplaceSearchQuery query)
    {
        var parameters = new List<KeyValuePair<string, string?>>
        {
            new("cityid", string.IsNullOrWhiteSpace(query.CityExternalId) ? "110100" : query.CityExternalId),
            new("brandid", query.BrandExternalId),
            new("pageindex", Math.Max(1, query.PageIndex).ToString(CultureInfo.InvariantCulture)),
            new("pagesize", Math.Clamp(query.PageSize, 1, 20).ToString(CultureInfo.InvariantCulture)),
            new("ssnew", "1"),
            new("sort", query.Sort switch
            {
                MarketplaceSort.PriceAscending => _options.PriceSortAscending.ToString(CultureInfo.InvariantCulture),
                MarketplaceSort.PriceDescending => _options.PriceSortDescending.ToString(CultureInfo.InvariantCulture),
                _ => "0"
            })
        };

        if (!string.IsNullOrWhiteSpace(query.SeriesExternalId))
        {
            parameters.Add(new("seriesid", query.SeriesExternalId));
        }

        if (query.PriceFromCny is not null || query.PriceToCny is not null)
        {
            var from = Che168RequestSigner.FormatWan(query.PriceFromCny ?? 0m);
            var to = Che168RequestSigner.FormatWan(query.PriceToCny ?? 999_000_000m);
            parameters.Add(new("price", $"{from}-{to}"));
        }

        if (query.MileageMaxKm is > 0)
        {
            var maxWan = (query.MileageMaxKm.Value / 10_000m).ToString("0.##", CultureInfo.InvariantCulture);
            parameters.Add(new("mileage", $"0-{maxWan}"));
        }

        if (query.YearFrom is not null || query.YearTo is not null)
        {
            var currentYear = DateTime.Now.Year;
            var newestYear = query.YearTo ?? currentYear;
            var oldestYear = query.YearFrom ?? newestYear - 30;
            var minAge = Math.Max(0, currentYear - newestYear);
            var maxAge = Math.Max(minAge, currentYear - oldestYear);
            parameters.Add(new("agerange", $"{minAge}-{maxAge}"));
        }

        if (query.DisplacementFromL is not null || query.DisplacementToL is not null)
        {
            var from = (query.DisplacementFromL ?? 0m).ToString("0.#", CultureInfo.InvariantCulture);
            var to = (query.DisplacementToL ?? 10m).ToString("0.#", CultureInfo.InvariantCulture);
            parameters.Add(new("displacement", $"{from}-{to}"));
        }

        AddFilter(parameters, "gearbox", query.Gearbox);
        AddFilter(parameters, "fueltype", query.FuelType);
        AddFilter(parameters, "powertrain", query.Powertrain);
        AddFilter(parameters, "classid", query.CategoryId);
        AddFilter(parameters, "structure", query.Structure);
        AddFilter(parameters, "environmental", query.Environmental);
        AddFilter(parameters, "setcount", query.Seats);

        return parameters;
    }

    private static void AddFilter(List<KeyValuePair<string, string?>> parameters, string key, int? value)
    {
        if (value is > 0)
        {
            parameters.Add(new(key, value.Value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    private async Task<JsonDocument> GetJsonAsync(
        string path,
        IEnumerable<KeyValuePair<string, string?>> parameters,
        CancellationToken cancellationToken,
        bool useDetailHost = false)
    {
        var baseUrl = useDetailHost ? _options.DetailApiBaseUrl : _options.ApiBaseUrl;
        var url = Che168RequestSigner.BuildUrl(
            baseUrl,
            path,
            parameters,
            _options.AppId,
            _options.AppVersion,
            _options.SubAppId);

        var client = _httpClientFactory.CreateClient("che168");
        Exception? lastError = null;

        for (var attempt = 1; attempt <= Math.Max(1, _options.MaxRetries); attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if ((int)response.StatusCode is 429 or >= 500)
                {
                    lastError = new HttpRequestException($"HTTP {(int)response.StatusCode} от che168");
                    if (attempt < _options.MaxRetries)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(350 * attempt), cancellationToken);
                        continue;
                    }
                }

                response.EnsureSuccessStatusCode();

                var text = await response.Content.ReadAsStringAsync(cancellationToken);
                text = text.TrimStart('\uFEFF');

                try
                {
                    return JsonDocument.Parse(text);
                }
                catch (JsonException ex)
                {
                    throw new Che168ApiException("che168 вернул некорректный JSON.", ex);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                lastError = ex;
                _logger.LogWarning(ex, "Запрос к che168 не удался (попытка {Attempt}/{MaxRetries})", attempt, _options.MaxRetries);

                if (attempt < _options.MaxRetries)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(350 * attempt), cancellationToken);
                }
            }
        }

        throw new Che168ApiException($"Не удалось получить данные от che168: {lastError?.Message}", lastError ?? new Exception());
    }

    private static JsonElement GetResult(JsonElement root, string operation)
    {
        if (!root.TryGetProperty("returncode", out var code) || code.ValueKind != JsonValueKind.Number)
        {
            throw new Che168ApiException($"che168: неожиданный ответ ({operation}).");
        }

        if (code.GetInt32() != 0)
        {
            var message = GetString(root, "message") ?? "неизвестная ошибка";
            throw new Che168ApiException($"che168 ({operation}): {message}");
        }

        if (!root.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Object)
        {
            throw new Che168ApiException($"che168 ({operation}): пустой result.");
        }

        return result;
    }

    private static MarketplaceBrand? ReadBrand(JsonElement item, int order)
    {
        var id = GetString(item, "bid");
        var name = GetString(item, "name");

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return new MarketplaceBrand(
            id,
            name,
            GetString(item, "py"),
            NormalizeUrl(GetString(item, "icon")),
            order);
    }

    private static MarketplaceSearchItem? ReadSearchItem(JsonElement car)
    {
        var id = GetString(car, "infoid");
        var title = GetString(car, "carname");

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var priceWan = GetDecimal(car, "price") ?? 0m;
        var mileageWan = GetDecimal(car, "mileage");
        var photoCount = GetInt(car, "photocount") ?? 0;

        return new MarketplaceSearchItem
        {
            ExternalId = id,
            Title = title,
            BrandName = null,
            SeriesName = null,
            PriceCny = decimal.Round(priceWan * 10_000m, 2, MidpointRounding.AwayFromZero),
            Year = ParseYear(GetString(car, "firstregyear")),
            MileageKm = mileageWan is null
                ? null
                : (int)Math.Round(mileageWan.Value * 10_000m, MidpointRounding.AwayFromZero),
            CityName = GetString(car, "cname"),
            ImageUrl = NormalizeUrl(GetString(car, "imageurl")),
            PhotoCount = photoCount,
            DealerId = GetString(car, "dealerid"),
            Displacement = GetString(car, "displacement"),
            EmissionStandard = GetString(car, "environmental"),
            IsNewEnergy = (GetInt(car, "isnewenergy") ?? 0) == 1
        };
    }

    private MarketplaceCarDetails ReadDetails(JsonElement car, string externalId)
    {
        var title = GetString(car, "carname") ?? $"Автомобиль {externalId}";
        var priceWan = GetDecimal(car, "price") ?? 0m;
        var guidanceWan = GetDecimal(car, "guidanceprice");
        var mileageWan = GetDecimal(car, "mileage");
        var reportedPhotoCount = GetInt(car, "photocount") ?? 0;

        var photos = new List<string>();
        if (car.TryGetProperty("piclist", out var picList) && picList.ValueKind == JsonValueKind.Array)
        {
            foreach (var photo in picList.EnumerateArray())
            {
                if (photo.ValueKind == JsonValueKind.String)
                {
                    var url = NormalizeUrl(photo.GetString());
                    if (!string.IsNullOrWhiteSpace(url) && !photos.Contains(url, StringComparer.Ordinal))
                    {
                        photos.Add(url);
                    }
                }
            }
        }

        var mainImage = NormalizeUrl(GetString(car, "imageurl"));
        if (!string.IsNullOrWhiteSpace(mainImage) && !photos.Contains(mainImage, StringComparer.Ordinal))
        {
            photos.Insert(0, mainImage);
        }

        var photoCount = photos.Count > 0 ? photos.Count : reportedPhotoCount;

        return new MarketplaceCarDetails
        {
            ExternalId = externalId,
            Title = title,
            BrandName = GetString(car, "brandname"),
            SeriesName = GetString(car, "seriesname"),
            SpecName = title,
            PriceCny = decimal.Round(priceWan * 10_000m, 2, MidpointRounding.AwayFromZero),
            GuidancePriceCny = guidanceWan is null
                ? null
                : decimal.Round(guidanceWan.Value * 10_000m, 2, MidpointRounding.AwayFromZero),
            Year = ParseYear(GetString(car, "firstregyear") ?? GetString(car, "firstregdate")),
            FirstRegistration = GetString(car, "firstregdate") ?? GetString(car, "firstregyear") ?? GetString(car, "firstregstr"),
            MileageKm = mileageWan is null
                ? null
                : (int)Math.Round(mileageWan.Value * 10_000m, MidpointRounding.AwayFromZero),
            CityName = GetString(car, "cname"),
            Displacement = GetString(car, "displacement"),
            Gearbox = GetString(car, "gearbox"),
            FuelType = GetString(car, "fuelname"),
            EmissionStandard = GetString(car, "environmental"),
            Color = GetString(car, "colorname"),
            TransferCount = GetInt(car, "transfercount"),
            BodyType = GetString(car, "levelname"),
            ImageUrl = mainImage,
            PhotoCount = photoCount,
            DealerId = GetString(car, "dealerid"),
            PublicUrl = BuildPublicUrl(externalId),
            Description = GetString(car, "remark") ?? GetString(car, "sellcarcontent"),
            IsNewEnergy = (GetInt(car, "isev") ?? GetInt(car, "isnewenergy") ?? 0) == 1,
            PublishedAtText = GetString(car, "publicdate"),
            Photos = photos
        };
    }

    private string BuildPublicUrl(string externalId)
    {
        return _options.DetailUrlTemplate.Replace("{infoid}", Uri.EscapeDataString(externalId), StringComparison.Ordinal);
    }

    private static string? GetString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "1",
            JsonValueKind.False => "0",
            _ => null
        };
    }

    private static int? GetInt(JsonElement element, string property)
    {
        var text = GetString(element, property);
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static decimal? GetDecimal(JsonElement element, string property)
    {
        var text = GetString(element, property);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static int? ParseYear(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).Take(4).ToArray());
        return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year)
            ? year
            : null;
    }

    private static string? NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        url = url.Trim();
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return "https://" + url[7..];
        }

        return url;
    }
}


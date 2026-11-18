namespace DiAvto.Web.Features.Marketplaces;

public enum MarketplaceSort
{
    Relevance = 0,
    PriceAscending = 1,
    PriceDescending = 2
}

public sealed record MarketplaceBrand(
    string ExternalId,
    string Name,
    string? Pinyin,
    string? IconUrl,
    int SortOrder);

public sealed record MarketplaceSeries(
    string ExternalId,
    string Name,
    string? Pinyin,
    string? IconUrl,
    string? PriceRange);

public sealed class MarketplaceSearchQuery
{
    public string BrandExternalId { get; set; } = string.Empty;
    public string? SeriesExternalId { get; set; }
    public string? CityExternalId { get; set; }

    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }

    /// <summary>Full CNY.</summary>
    public decimal? PriceFromCny { get; set; }

    /// <summary>Full CNY.</summary>
    public decimal? PriceToCny { get; set; }

    public int? MileageMaxKm { get; set; }

    public decimal? DisplacementFromL { get; set; }
    public decimal? DisplacementToL { get; set; }

    /// <summary>1 — механика, 2 — автомат.</summary>
    public int? Gearbox { get; set; }

    /// <summary>1 — бензин, 2 — дизель, 3 — гибрид, 5 — PHEV, 6 — EREV, 7 — электро.</summary>
    public int? FuelType { get; set; }

    /// <summary>1 — передний, 2 — задний, 3 — полный.</summary>
    public int? Powertrain { get; set; }

    /// <summary>che168 classid: класс автомобиля.</summary>
    public int? CategoryId { get; set; }

    /// <summary>che168 structure: тип кузова.</summary>
    public int? Structure { get; set; }

    /// <summary>che168 environmental: экологический класс.</summary>
    public int? Environmental { get; set; }

    /// <summary>che168 setcount: количество мест.</summary>
    public int? Seats { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public MarketplaceSort Sort { get; set; } = MarketplaceSort.Relevance;

    /// <summary>Копия запроса с другой страницей и сортировкой — фильтры остаются прежними.</summary>
    public MarketplaceSearchQuery CloneForPage(MarketplaceSort sort, int pageIndex, int pageSize)
    {
        return new MarketplaceSearchQuery
        {
            BrandExternalId = BrandExternalId,
            SeriesExternalId = SeriesExternalId,
            CityExternalId = CityExternalId,
            YearFrom = YearFrom,
            YearTo = YearTo,
            PriceFromCny = PriceFromCny,
            PriceToCny = PriceToCny,
            MileageMaxKm = MileageMaxKm,
            DisplacementFromL = DisplacementFromL,
            DisplacementToL = DisplacementToL,
            Gearbox = Gearbox,
            FuelType = FuelType,
            Powertrain = Powertrain,
            CategoryId = CategoryId,
            Structure = Structure,
            Environmental = Environmental,
            Seats = Seats,
            PageIndex = pageIndex,
            PageSize = pageSize,
            Sort = sort
        };
    }
}

public sealed class MarketplaceSearchItem
{
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? SeriesName { get; set; }
    public string? SpecName { get; set; }
    public decimal PriceCny { get; set; }
    public int? Year { get; set; }
    public string? FirstRegistration { get; set; }
    public int? MileageKm { get; set; }
    public string? CityName { get; set; }
    public string? ImageUrl { get; set; }
    public int PhotoCount { get; set; }
    public string? DealerId { get; set; }
    public string? DealerName { get; set; }
    public string? Displacement { get; set; }
    public string? EmissionStandard { get; set; }
    public string? PublicUrl { get; set; }
    public bool IsNewEnergy { get; set; }
}

public sealed class MarketplaceSearchPage
{
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageCount { get; set; }
    public List<MarketplaceSearchItem> Items { get; set; } = [];
}

public sealed class MarketplaceCarDetails
{
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? SeriesName { get; set; }
    public string? SpecName { get; set; }
    public decimal PriceCny { get; set; }
    public decimal? GuidancePriceCny { get; set; }
    public int? Year { get; set; }
    public string? FirstRegistration { get; set; }
    public int? MileageKm { get; set; }
    public string? CityName { get; set; }
    public string? Displacement { get; set; }
    public string? Gearbox { get; set; }
    public string? FuelType { get; set; }
    public string? EmissionStandard { get; set; }
    public string? Color { get; set; }
    public int? TransferCount { get; set; }
    public string? BodyType { get; set; }
    public string? ImageUrl { get; set; }
    public int PhotoCount { get; set; }
    public string? DealerId { get; set; }
    public string? DealerName { get; set; }
    public string? PublicUrl { get; set; }
    public string? Description { get; set; }
    public bool IsNewEnergy { get; set; }
    public string? PublishedAtText { get; set; }
    public List<string> Photos { get; set; } = [];
    public string? RawJson { get; set; }
}

public sealed record MarketplaceHealth(bool Ok, string Message);

public interface IMarketplaceClient
{
    string Key { get; }
    string DisplayName { get; }

    Task<IReadOnlyList<MarketplaceBrand>> GetBrandsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MarketplaceSeries>> GetSeriesAsync(
        string brandExternalId,
        CancellationToken cancellationToken = default);

    Task<MarketplaceSearchPage> SearchAsync(
        MarketplaceSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<MarketplaceCarDetails> GetCarDetailsAsync(
        string externalId,
        CancellationToken cancellationToken = default);

    Task<MarketplaceHealth> CheckHealthAsync(CancellationToken cancellationToken = default);
}

public interface IMarketplaceClientFactory
{
    IMarketplaceClient Get(string key);
    IReadOnlyList<IMarketplaceClient> All { get; }
}

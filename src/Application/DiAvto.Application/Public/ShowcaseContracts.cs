namespace DiAvto.Application.Public;

public sealed record ShowcaseCarDto(
    int Id,
    string Name,
    string Make,
    int Year,
    decimal PriceRub,
    string Country,
    string Fuel,
    string BodyType,
    string Transmission,
    string Drive,
    string Engine,
    int MileageKm,
    string Description,
    bool InStock,
    IReadOnlyList<string> ImageUrls)
{
    public string MainImageUrl => ImageUrls.Count > 0 ? ImageUrls[0] : "/images/car-placeholder.svg";
}

public sealed record ShowcaseCarFilter
{
    public string? Search { get; set; }
    public string? Country { get; set; }
    public string? Make { get; set; }
    public string? BodyType { get; set; }
    public string? Transmission { get; set; }
    public string? Fuel { get; set; }
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public string Sort { get; set; } = "default";
    public int Skip { get; set; }
    public int Take { get; set; } = 24;
}

public sealed record CatalogFacets(
    IReadOnlyList<string> Countries,
    IReadOnlyList<string> Makes,
    IReadOnlyList<string> BodyTypes,
    IReadOnlyList<string> Transmissions,
    IReadOnlyList<string> FuelTypes,
    decimal MinPrice,
    decimal MaxPrice,
    int MinYear,
    int MaxYear);

public interface IShowcaseCarService
{
    Task<IReadOnlyList<ShowcaseCarDto>> GetPublishedAsync(
        ShowcaseCarFilter filter,
        CancellationToken cancellationToken = default);

    Task<ShowcaseCarDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShowcaseCarDto>> GetFeaturedAsync(
        int take = 6,
        CancellationToken cancellationToken = default);

    Task<CatalogFacets> GetFacetsAsync(CancellationToken cancellationToken = default);
}


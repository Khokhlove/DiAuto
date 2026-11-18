namespace DiAvto.Web.Domain;

public class CarListing
{
    public int Id { get; set; }

    public int SourceSiteId { get; set; }
    public SourceSite SourceSite { get; set; } = null!;

    /// <summary>Listing id on the source site (che168 "infoid").</summary>
    public string ExternalId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? BrandName { get; set; }
    public string? SeriesName { get; set; }
    public string? SpecName { get; set; }

    public int? Year { get; set; }
    public string? FirstRegistration { get; set; }

    /// <summary>Price in full CNY (not in 10,000 CNY).</summary>
    public decimal PriceCny { get; set; }

    public decimal? GuidancePriceCny { get; set; }

    /// <summary>Mileage in kilometres.</summary>
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

    public DateTime LastFetchedAt { get; set; } = DateTime.UtcNow;

    public string? RawJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CarPhoto> Photos { get; set; } = new List<CarPhoto>();
    public ICollection<SearchResultItem> SearchResults { get; set; } = new List<SearchResultItem>();
    public ICollection<ClientCard> ClientCards { get; set; } = new List<ClientCard>();
}

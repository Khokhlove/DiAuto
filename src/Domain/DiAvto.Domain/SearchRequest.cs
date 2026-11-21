namespace DiAvto.Domain;

public class SearchRequest
{
    public int Id { get; set; }

    /// <summary>Public, non-sequential id used in URLs.</summary>
    public string PublicId { get; set; } = string.Empty;

    public int SourceSiteId { get; set; }
    public SourceSite SourceSite { get; set; } = null!;

    public string BrandName { get; set; } = string.Empty;
    public string? SeriesName { get; set; }

    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }

    /// <summary>Lower price bound in full CNY.</summary>
    public decimal? PriceFromCny { get; set; }

    /// <summary>Upper price bound in full CNY.</summary>
    public decimal? PriceToCny { get; set; }

    public int? MileageMaxKm { get; set; }

    public string? CityName { get; set; }

    public SearchStatus Status { get; set; } = SearchStatus.Pending;

    public string? ErrorMessage { get; set; }

    public int ResultCount { get; set; }

    /// <summary>CNY -> RUB rate captured when the search was executed.</summary>
    public decimal ExchangeRate { get; set; }

    public decimal MarkupPercent { get; set; }

    public string? CriteriaJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ICollection<SearchResultItem> Items { get; set; } = new List<SearchResultItem>();
}


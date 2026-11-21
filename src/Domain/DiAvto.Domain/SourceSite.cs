namespace DiAvto.Domain;

/// <summary>
/// A marketplace / data source (che168.com today, more sources later).
/// </summary>
public class SourceSite
{
    public int Id { get; set; }

    /// <summary>Stable technical key, for example "che168".</summary>
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Template for a public car page. Supported token: {infoid}.</summary>
    public string? DetailUrlTemplate { get; set; }

    public string? LogoUrl { get; set; }

    /// <summary>Attribution shown under cars and in the footer.</summary>
    public string AttributionText { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastHealthCheckAt { get; set; }

    public bool? LastHealthCheckOk { get; set; }

    public string? LastHealthCheckMessage { get; set; }

    public ICollection<Brand> Brands { get; set; } = new List<Brand>();
    public ICollection<CarListing> CarListings { get; set; } = new List<CarListing>();
    public ICollection<SearchRequest> SearchRequests { get; set; } = new List<SearchRequest>();
}


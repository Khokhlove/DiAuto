namespace DiAvto.Domain;

public class Brand
{
    public int Id { get; set; }

    public int SourceSiteId { get; set; }
    public SourceSite SourceSite { get; set; } = null!;

    /// <summary>Brand id on the source site (che168 "bid").</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Name as returned by the source (Chinese).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Latin/pinyin name, useful for search and fallback display.</summary>
    public string? NameLatin { get; set; }

    /// <summary>Optional Russian/English display name.</summary>
    public string? NameRu { get; set; }

    public string? IconUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CarSeries> Series { get; set; } = new List<CarSeries>();

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(NameRu) ? NameRu! :
        !string.IsNullOrWhiteSpace(NameLatin) ? $"{NameLatin} / {Name}" :
        Name;
}


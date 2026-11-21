namespace DiAvto.Domain;

public class CarSeries
{
    public int Id { get; set; }

    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    /// <summary>Series id on the source site (che168 "sid").</summary>
    public string ExternalId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? NameRu { get; set; }

    public string? Pinyin { get; set; }

    public string? IconUrl { get; set; }

    public string? PriceRange { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(NameRu) ? NameRu! :
        !string.IsNullOrWhiteSpace(Pinyin) ? $"{Name} ({Pinyin})" :
        Name;
}


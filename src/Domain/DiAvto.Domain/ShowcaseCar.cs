namespace DiAvto.Domain;

/// <summary>Автомобиль на публичной витрине сайта.</summary>
public class ShowcaseCar
{
    public int Id { get; set; }

    /// <summary>ID автомобиля в исходной витрине/источнике.</summary>
    public string? ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public int Year { get; set; }

    /// <summary>Публичная цена в рублях.</summary>
    public decimal PriceRub { get; set; }

    public string Country { get; set; } = "Китай";

    public string Fuel { get; set; } = string.Empty;

    public string BodyType { get; set; } = string.Empty;

    public string Transmission { get; set; } = string.Empty;

    public string Drive { get; set; } = string.Empty;

    public string Engine { get; set; } = string.Empty;

    public int MileageKm { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool InStock { get; set; } = true;

    public bool IsPublished { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ShowcaseCarPhoto> Photos { get; set; } = new List<ShowcaseCarPhoto>();
}

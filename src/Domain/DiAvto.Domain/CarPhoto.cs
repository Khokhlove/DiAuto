namespace DiAvto.Domain;

public class CarPhoto
{
    public int Id { get; set; }

    public int CarListingId { get; set; }
    public CarListing CarListing { get; set; } = null!;

    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}


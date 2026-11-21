namespace DiAvto.Domain;

public class Office
{
    public int Id { get; set; }

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Phone2 { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

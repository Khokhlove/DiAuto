namespace DiAvto.Domain;

public class SearchResultItem
{
    public int Id { get; set; }

    public int SearchRequestId { get; set; }
    public SearchRequest SearchRequest { get; set; } = null!;

    public int CarListingId { get; set; }
    public CarListing CarListing { get; set; } = null!;

    public ResultTier Tier { get; set; }

    /// <summary>1..3 within the tier.</summary>
    public int Rank { get; set; }

    public decimal PriceCny { get; set; }
    public decimal PriceRub { get; set; }
}


namespace DiAvto.Web.Domain;

public class CurrencyRate
{
    public int Id { get; set; }

    public string BaseCurrency { get; set; } = "CNY";
    public string QuoteCurrency { get; set; } = "RUB";

    /// <summary>How many RUB for one unit of the base currency.</summary>
    public decimal Rate { get; set; }

    public string Source { get; set; } = "CBR";

    /// <summary>Date the rate is effective for (CBR publishes a daily rate).</summary>
    public DateTime EffectiveDate { get; set; }

    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;

    public bool IsFallback { get; set; }
}

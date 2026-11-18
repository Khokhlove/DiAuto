namespace DiAvto.Web.Domain;

/// <summary>
/// Публичная карточка автомобиля для клиента. Все суммы фиксируются на момент
/// формирования, чтобы изменение тарифов не меняло уже отправленное предложение.
/// </summary>
public class ClientCard
{
    public int Id { get; set; }

    /// <summary>Публичный токен для ссылки вида /card/{PublicId}.</summary>
    public string PublicId { get; set; } = string.Empty;

    public int CarListingId { get; set; }
    public CarListing CarListing { get; set; } = null!;

    public string? CustomerName { get; set; }

    public string CountryName { get; set; } = "Китай";

    public string DestinationCity { get; set; } = string.Empty;

    public string? DeliveryToVladivostokTitle { get; set; }

    public string? DeliveryFromVladivostokTitle { get; set; }

    public string? InspectionTitle { get; set; }

    public string? CustomsClearanceTitle { get; set; }

    public decimal CarPriceCny { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal MarkupPercent { get; set; }

    public decimal CarPriceRub { get; set; }

    public decimal DeliveryToVladivostokRub { get; set; }

    public decimal DeliveryFromVladivostokRub { get; set; }

    public decimal InspectionRub { get; set; }

    public decimal CustomsClearanceRub { get; set; }

    /// <summary>Итоговая стоимость под ключ, которую видит клиент.</summary>
    public decimal TotalRub { get; set; }

    /// <summary>Комментарий менеджера: контакты, условия, сроки.</summary>
    public string? Note { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;
}

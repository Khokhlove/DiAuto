namespace DiAvto.Domain;

public enum ServiceTariffKind
{
    /// <summary>Доставка автомобиля из города покупки до Владивостока.</summary>
    DeliveryToVladivostok = 0,

    /// <summary>Доставка из Владивостока до города клиента.</summary>
    DeliveryFromVladivostok = 1,

    /// <summary>Осмотр автомобиля в регионе покупки.</summary>
    Inspection = 2,

    /// <summary>Таможенное оформление (растаможка) автомобиля.</summary>
    CustomsClearance = 3
}

public class ServiceTariff
{
    public int Id { get; set; }

    public ServiceTariffKind Kind { get; set; }

    /// <summary>Название тарифа: регион покупки или город назначения.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Стоимость услуги в рублях.</summary>
    public decimal AmountRub { get; set; }

    /// <summary>Тариф по умолчанию для своего типа.</summary>
    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


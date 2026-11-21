using System.Text.Json.Serialization;
using DiAvto.Application.Pricing;

namespace DiAvto.Application.Search;

public enum PriceCurrency
{
    Cny = 0,
    Rub = 1
}

public sealed class SearchCriteria
{
    public int BrandId { get; set; }
    public int? SeriesId { get; set; }

    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }

    /// <summary>Price bounds exactly as entered by the user in <see cref="PriceCurrency"/>.</summary>
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }

    /// <summary>Currency selected in the search form.</summary>
    public PriceCurrency PriceCurrency { get; set; } = PriceCurrency.Cny;

    /// <summary>Maximum mileage in kilometers exactly as entered by the user.</summary>
    public int? MileageMaxKm { get; set; }

    public decimal? DisplacementFromL { get; set; }
    public decimal? DisplacementToL { get; set; }

    /// <summary>1 — механика, 2 — автомат.</summary>
    public int? Gearbox { get; set; }

    /// <summary>1 — бензин, 2 — дизель, 3 — гибрид, 5 — PHEV, 6 — EREV, 7 — электро.</summary>
    public int? FuelType { get; set; }

    /// <summary>1 — передний, 2 — задний, 3 — полный.</summary>
    public int? Powertrain { get; set; }

    public int? CategoryId { get; set; }
    public int? Structure { get; set; }
    public int? Environmental { get; set; }
    public int? Seats { get; set; }

    /// <summary>Выбранный тариф доставки из Владивостока до города клиента.</summary>
    public int? DeliveryFromVladivostokTariffId { get; set; }

    /// <summary>Доставка до Владивостока в валюте источника (CNY/JPY/KRW).</summary>
    public decimal DeliveryToVladivostokAmount { get; set; }
    public string DeliveryToVladivostokCurrency { get; set; } = "CNY";

    /// <summary>Параметры автомобиля для расчёта таможни.</summary>
    public int EngineCc { get; set; } = 2000;
    public int PowerHp { get; set; } = 150;
    public int VehicleYear { get; set; } = DateTime.UtcNow.Year - 3;
    public bool IsDiesel { get; set; }
    public bool IsElectric { get; set; }
    public bool IsLegalEntity { get; set; }
    public bool UseResaleUtilization { get; set; }

    /// <summary>Учитывать доставку до города клиента в бюджете.</summary>
    public bool IncludeDeliveryToCity { get; set; }

    /// <summary>CNY/RUB rate captured for this search. Set by the search service.</summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>Markup percent captured for this search. Set by the search service.</summary>
    public decimal MarkupPercent { get; set; }

    public string? CityExternalId { get; set; }
    public string? CityName { get; set; }

    /// <summary>
    /// Снимок бюджета и расходов. Заполняется сервисом поиска до запроса к площадке,
    /// чтобы в результатах можно было показать, на что ушли деньги.
    /// </summary>
    public SearchBudgetPlan? BudgetPlan { get; set; }

    /// <summary>
    /// Цена для площадки в юанях. Для рублёвого бюджета это остаток после вычета
    /// всех расходов; для поиска в юанях — исходная цена автомобиля.
    /// </summary>
    [JsonIgnore]
    public decimal? PriceFromCny => BudgetPlan is { HasBudgetFrom: true } plan
        ? plan.CarBudgetFromCny ?? throw RateUnavailable()
        : ToCny(PriceFrom);

    /// <inheritdoc cref="PriceFromCny"/>
    [JsonIgnore]
    public decimal? PriceToCny => BudgetPlan is { HasBudgetTo: true } plan
        ? plan.CarBudgetToCny ?? throw RateUnavailable()
        : ToCny(PriceTo);

    public ServiceTariffSelection ToTariffSelection() => new(
        null,
        DeliveryFromVladivostokTariffId,
        null,
        null);

    private decimal? ToCny(decimal? amount)
    {
        if (amount is null)
        {
            return null;
        }

        if (PriceCurrency == PriceCurrency.Cny)
        {
            return amount;
        }

        var rate = ExchangeRate ?? 0m;
        if (rate <= 0m)
        {
            throw RateUnavailable();
        }

        return PricingCalculator.TryRubToCny(amount.Value, rate, MarkupPercent);
    }

    private static InvalidOperationException RateUnavailable()
    {
        return new InvalidOperationException("Курс CNY/RUB недоступен для пересчёта цены.");
    }
}



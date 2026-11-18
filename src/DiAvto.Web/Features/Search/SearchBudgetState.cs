using DiAvto.Web.Domain;
using DiAvto.Web.Features.Pricing;

namespace DiAvto.Web.Features.Search;

/// <summary>
/// Состояние бюджета, общее для левой панели и формы поиска.
/// Живёт на странице поиска, чтобы расчёт и критерии использовали одни и те же значения.
/// </summary>
public sealed class SearchBudgetState
{
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public PriceCurrency Currency { get; set; } = PriceCurrency.Cny;

    public decimal DeliveryToVladivostokAmount { get; set; }
    public string DeliveryToVladivostokCurrency { get; set; } = "CNY";

    public int EngineCc { get; set; } = 2000;
    public int PowerHp { get; set; } = 150;
    public int VehicleYear { get; set; } = DateTime.UtcNow.Year - 3;
    public string FuelKind { get; set; } = "petrol";
    public bool IsLegalEntity { get; set; }
    public bool UseResaleUtilization { get; set; }

    public bool IsDiesel => FuelKind is "diesel" or "diesel-hybrid";
    public bool IsElectric => FuelKind == "electric";

    public bool IncludeDeliveryToCity { get; set; }
    public int DeliveryFromVladivostokTariffId { get; set; }

    public List<ServiceTariff> ActiveTariffs { get; set; } = [];
    public ImportCurrencyRates Rates { get; set; } = new();
    public bool Loading { get; set; } = true;
    public string? Error { get; set; }

    public SearchBudgetPlan? BuildPlan()
    {
        if (Loading)
        {
            return null;
        }

        var cityTariff = ActiveTariffs.FirstOrDefault(x => x.Id == DeliveryFromVladivostokTariffId)
                         ?? ServiceTariffPicker.PickDefault(ForKind(ServiceTariffKind.DeliveryFromVladivostok));

        var template = new ImportPricingParameters
        {
            DeliveryToVladivostokAmount = DeliveryToVladivostokAmount,
            DeliveryToVladivostokCurrency = DeliveryToVladivostokCurrency,
            DeliveryToCityRub = cityTariff?.AmountRub ?? 0m,
            IncludeDeliveryToCity = IncludeDeliveryToCity,
            EngineCc = EngineCc,
            PowerHp = PowerHp,
            VehicleYear = VehicleYear,
            IsDiesel = IsDiesel,
            IsElectric = IsElectric,
            IsLegalEntity = IsLegalEntity,
            UseResaleUtilization = UseResaleUtilization,
            Rates = Rates
        };

        return SearchBudgetPlanner.Build(PriceFrom, PriceTo, Currency, template);
    }

    public void ApplyDefaultTariffs()
    {
        DeliveryFromVladivostokTariffId = PickDefaultId(ServiceTariffKind.DeliveryFromVladivostok);
    }

    public List<ServiceTariff> ForKind(ServiceTariffKind kind)
    {
        return ActiveTariffs.Where(x => x.Kind == kind).ToList();
    }

    private int PickDefaultId(ServiceTariffKind kind)
    {
        return ServiceTariffPicker.PickDefault(ForKind(kind))?.Id ?? 0;
    }
}

using DiAvto.Application.Pricing;
using DiAvto.Domain;

namespace DiAvto.Tests;

public class ClientCardPricingTests
{
    [Fact]
    public void Calculate_AddsMarkupDeliveryInspectionAndCustoms()
    {
        var totals = PricingCalculator.CalculateClientCard(
            priceCny: 100_000m,
            exchangeRate: 12m,
            markupPercent: 10m,
            deliveryToVladivostokRub: 150_000m,
            deliveryFromVladivostokRub: 30_000m,
            inspectionRub: 20_000m,
            customsClearanceRub: 100_000m);

        Assert.Equal(1_320_000m, totals.CarPriceRub);
        Assert.Equal(1_620_000m, totals.TotalRub);
    }

    [Fact]
    public void Calculate_RoundsToKopecks()
    {
        var totals = PricingCalculator.CalculateClientCard(
            priceCny: 10_000m,
            exchangeRate: 12.345m,
            markupPercent: 0m,
            deliveryToVladivostokRub: 0m,
            deliveryFromVladivostokRub: 0m,
            inspectionRub: 0m,
            customsClearanceRub: 0m);

        Assert.Equal(123_450m, totals.CarPriceRub);
        Assert.Equal(123_450m, totals.TotalRub);
    }

    [Fact]
    public void PickBestMatch_PrefersCityOverDefault()
    {
        var tariffs = new List<ServiceTariff>
        {
            new()
            {
                Id = 1,
                Title = "Китай (базовый)",
                IsDefault = true,
                AmountRub = 100_000m
            },
            new()
            {
                Id = 2,
                Title = "Гуанчжоу",
                AmountRub = 140_000m
            }
        };

        var match = ServiceTariffPicker.PickBestMatch(tariffs, "广州");

        Assert.NotNull(match);
        Assert.Equal(2, match.Id);
    }

    [Fact]
    public void PickBestMatch_FallsBackToDefault()
    {
        var tariffs = new List<ServiceTariff>
        {
            new()
            {
                Id = 1,
                Title = "Шанхай",
                AmountRub = 100_000m
            },
            new()
            {
                Id = 2,
                Title = "Китай (базовый)",
                IsDefault = true,
                AmountRub = 120_000m
            }
        };

        var match = ServiceTariffPicker.PickBestMatch(tariffs, "Гуанчжоу");

        Assert.NotNull(match);
        Assert.Equal(2, match.Id);
    }
}

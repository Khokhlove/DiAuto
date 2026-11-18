using DiAvto.Web.Features.Pricing;

namespace DiAvto.Tests;

public class CustomsCalculatorTests
{
    [Fact]
    public void Calculate_PhysicalEts_MatchesInspectionExample()
    {
        var result = CustomsCalculator.Calculate(
            customsValueRub: 2_000_000m,
            engineCc: 2000,
            powerHp: 160,
            vehicleYear: DateTime.UtcNow.Year,
            isDiesel: false,
            isElectric: false,
            isLegalEntity: false,
            useResaleUtilization: false,
            eurRubRate: 90.8136m);

        Assert.Equal(998_949.6m, result.DutyRub);
        Assert.Equal(13_541m, result.CustomsFeeRub);
        Assert.Equal(3_400m, result.PersonalUtilizationRub);
        Assert.Equal(800_800m, result.ResaleUtilizationRub);
        Assert.Equal(1_015_890.6m, result.TotalPersonalRub);
        Assert.Equal(1_813_290.6m, result.TotalResaleRub);
        Assert.Equal(1_015_890.6m, result.TotalRub);
        Assert.Equal(0m, result.ExciseRub);
        Assert.Equal(0m, result.VatRub);
    }

    [Fact]
    public void Calculate_PhysicalEts_UsesResaleUtilizationWhenSelected()
    {
        var result = CustomsCalculator.Calculate(
            customsValueRub: 2_000_000m,
            engineCc: 2000,
            powerHp: 160,
            vehicleYear: DateTime.UtcNow.Year,
            isDiesel: false,
            isElectric: false,
            isLegalEntity: false,
            useResaleUtilization: true,
            eurRubRate: 90.8136m);

        Assert.Equal(1_813_290.6m, result.TotalRub);
        Assert.Equal(800_800m, result.UtilizationFeeRub);
    }

    [Fact]
    public void Calculate_LegalEntity_UsesStpDutyExciseAndVat()
    {
        var result = CustomsCalculator.Calculate(
            customsValueRub: 2_000_000m,
            engineCc: 2000,
            powerHp: 150,
            vehicleYear: DateTime.UtcNow.Year - 3,
            isDiesel: false,
            isElectric: false,
            isLegalEntity: true,
            useResaleUtilization: false,
            eurRubRate: 90.8136m);

        Assert.Equal(400_000m, result.DutyRub);
        Assert.Equal(9_600m, result.ExciseRub);
        Assert.Equal(530_112m, result.VatRub);
        Assert.Equal(1_408_800m, result.UtilizationFeeRub);
        Assert.Equal(13_541m, result.CustomsFeeRub);
        Assert.Equal(2_362_053m, result.TotalRub);
    }

    [Fact]
    public void Calculate_PhysicalEts_AgeThreeUsesPerCcDuty()
    {
        var result = CustomsCalculator.Calculate(
            customsValueRub: 2_000_000m,
            engineCc: 2000,
            powerHp: 150,
            vehicleYear: DateTime.UtcNow.Year - 4,
            isDiesel: false,
            isElectric: false,
            isLegalEntity: false,
            useResaleUtilization: false,
            eurRubRate: 90.8136m);

        Assert.Equal(490_393.44m, result.DutyRub);
        Assert.Equal(5_200m, result.PersonalUtilizationRub);
        Assert.Equal(509_134.44m, result.TotalRub);
    }

    [Theory]
    [InlineData(3_000_000, 150_000)]
    [InlineData(3_000_001, 160_000)]
    [InlineData(3_100_000, 160_000)]
    [InlineData(3_100_001, 170_000)]
    public void Commission_RoundsStepsUp(decimal carPriceRub, decimal expected)
    {
        var parameters = new ImportPricingParameters();
        Assert.Equal(expected, ImportPricingCalculator.CalculateCommission(carPriceRub, parameters));
    }

    [Fact]
    public void Calculate_BankCommissionIsPercentOfFullSubtotal()
    {
        var breakdown = ImportPricingCalculator.Calculate(new ImportPricingParameters
        {
            CarPrice = 1_000_000m,
            CarCurrency = "RUB",
            DeliveryToVladivostokAmount = 0m,
            EngineCc = 0,
            PowerHp = 0,
            InspectionCny = 900m,
            BrokerLaboratoryRub = 140_000m,
            CommissionBaseRub = 150_000m,
            Rates = new ImportCurrencyRates { CnyRub = 10m, EurRub = 100m }
        });

        Assert.Equal(9_000m, breakdown.InspectionRub);
        Assert.Equal(150_000m, breakdown.OurCommissionRub);
        Assert.Equal(32_475m, breakdown.BankCommissionRub);
        Assert.Equal(1_331_475m, breakdown.TotalRub);
    }

    [Fact]
    public void Solver_FindsMaximumCarPrice()
    {
        var template = new ImportPricingParameters
        {
            EngineCc = 2000,
            PowerHp = 150,
            VehicleYear = DateTime.UtcNow.Year - 1,
            Rates = new ImportCurrencyRates { CnyRub = 10m, EurRub = 100m }
        };

        var breakdown = ImportBudgetSolver.SolveMaxCarPriceRub(2_000_000m, template);

        Assert.True(breakdown.CarPriceRub > 0m);
        Assert.True(breakdown.TotalRub <= 2_000_000m);
        Assert.True(
            ImportPricingCalculator.CalculateForCarPriceRub(
                template, breakdown.CarPriceRub + 1_000m).TotalRub > 2_000_000m);
    }
}

using DiAvto.Web.Features.Pricing;
using DiAvto.Web.Features.Search;

namespace DiAvto.Tests;

public class SearchBudgetPlannerTests
{
    [Fact]
    public void Build_FindsCarBudgetWithinBudget()
    {
        var plan = SearchBudgetPlanner.Build(
            budgetFrom: null,
            budgetTo: 2_000_000m,
            PriceCurrency.Rub,
            Template());

        Assert.True(plan.HasBudget);
        Assert.NotNull(plan.CarBudgetToRub);
        Assert.True(plan.CarBudgetToRub > 0m);
        Assert.True(plan.CarBudgetToCny > 0m);
        Assert.NotEmpty(plan.Lines);
        Assert.Contains(plan.Lines, x => x.Title == "Наша комиссия");
        Assert.Contains(plan.Lines, x => x.Title == "Банковская комиссия 2,5%");
        Assert.DoesNotContain(plan.Lines, x => x.Title == "Автомобиль");

        var check = ImportPricingCalculator.CalculateForCarPriceRub(
            Template(), plan.CarBudgetToRub!.Value);
        Assert.True(check.TotalRub <= 2_000_000m);
    }

    [Fact]
    public void Build_FlagsBudgetBelowFixedExpenses()
    {
        var plan = SearchBudgetPlanner.Build(
            budgetFrom: null,
            budgetTo: 100_000m,
            PriceCurrency.Rub,
            Template());

        Assert.True(plan.IsTooLow);
        Assert.Equal(0m, plan.CarBudgetToRub);
    }

    [Fact]
    public void Build_ConvertsCnyBudgetToCnyCarLimit()
    {
        var plan = SearchBudgetPlanner.Build(
            budgetFrom: null,
            budgetTo: 100_000m,
            PriceCurrency.Cny,
            Template());

        Assert.True(plan.HasBudget);
        Assert.Equal(100_000m, plan.BudgetToCny);
        Assert.Equal(PriceCurrency.Cny, plan.Currency);
        Assert.NotEmpty(plan.Lines);
    }

    [Fact]
    public void Build_KeepsExpenseSnapshotInCriteriaJson()
    {
        var plan = SearchBudgetPlanner.Build(
            budgetFrom: null,
            budgetTo: 200_000m,
            PriceCurrency.Rub,
            Template());

        var criteria = new SearchCriteria
        {
            BrandId = 1,
            PriceCurrency = PriceCurrency.Rub,
            PriceTo = 200_000m,
            BudgetPlan = plan
        };

        var json = SearchCriteriaSerializer.Serialize(criteria);
        var restored = SearchCriteriaSerializer.TryDeserialize(json);

        Assert.NotNull(restored?.BudgetPlan);
        Assert.True(restored!.BudgetPlan!.HasBudget);
        Assert.NotEmpty(restored.BudgetPlan.Lines);
    }

    private static ImportPricingParameters Template()
    {
        return new ImportPricingParameters
        {
            EngineCc = 2000,
            PowerHp = 150,
            VehicleYear = DateTime.UtcNow.Year - 3,
            IsDiesel = false,
            DeliveryToVladivostokAmount = 0m,
            DeliveryToVladivostokCurrency = "CNY",
            BrokerLaboratoryRub = 140_000m,
            InspectionCny = 900m,
            Rates = new ImportCurrencyRates
            {
                CnyRub = 10m,
                EurRub = 100m,
                JpyRub = 0.6m,
                KrwRub = 0.07m
            }
        };
    }
}

using System.Text.Json.Serialization;
using DiAvto.Application.Pricing;

namespace DiAvto.Application.Search;

/// <summary>
/// Снимок бюджета поиска: сколько клиент готов потратить, сколько уходит на
/// сопутствующие расходы и сколько остаётся на автомобиль.
/// Сохраняется в <c>SearchRequest.CriteriaJson</c>, чтобы результаты можно было
/// показать даже после изменения тарифов.
/// </summary>
public sealed class SearchBudgetPlan
{
    public PriceCurrency Currency { get; set; }

    public decimal? BudgetFromRub { get; set; }
    public decimal? BudgetToRub { get; set; }
    public decimal? BudgetFromCny { get; set; }
    public decimal? BudgetToCny { get; set; }

    public decimal ExpensesRub { get; set; }
    public decimal? ExpensesCny { get; set; }

    public decimal? CarBudgetFromRub { get; set; }
    public decimal? CarBudgetToRub { get; set; }
    public decimal? CarBudgetFromCny { get; set; }
    public decimal? CarBudgetToCny { get; set; }

    public decimal ExchangeRate { get; set; }
    public decimal MarkupPercent { get; set; }

    public List<ImportCostLine> Lines { get; set; } = [];

    [JsonIgnore]
    public bool HasBudgetFrom => BudgetFromRub is not null || BudgetFromCny is not null;

    [JsonIgnore]
    public bool HasBudgetTo => BudgetToRub is not null || BudgetToCny is not null;

    [JsonIgnore]
    public bool HasBudget => HasBudgetFrom || HasBudgetTo;

    [JsonIgnore]
    public bool HasBudgetRange => HasBudgetFrom && HasBudgetTo;

    [JsonIgnore]
    public bool IsTooLow =>
        (BudgetToRub is not null && (CarBudgetToRub ?? 0m) <= 0m) ||
        (BudgetToCny is not null && (CarBudgetToCny ?? 0m) <= 0m);
}

public static class SearchBudgetPlanner
{
    /// <summary>
    /// Считает бюджет «под ключ» по актуальной модели расходов.
    /// Стоимость автомобиля находится обратным расчётом из бюджета.
    /// </summary>
    public static SearchBudgetPlan Build(
        decimal? budgetFrom,
        decimal? budgetTo,
        PriceCurrency currency,
        ImportPricingParameters template)
    {
        var rate = currency == PriceCurrency.Rub ? 1m : template.Rates.CnyRub;
        if (rate <= 0m)
        {
            throw new InvalidOperationException("Курс CNY/RUB недоступен для пересчёта бюджета.");
        }

        decimal? budgetFromRub = budgetFrom is null ? null : budgetFrom.Value * rate;
        decimal? budgetToRub = budgetTo is null ? null : budgetTo.Value * rate;

        var plan = new SearchBudgetPlan
        {
            Currency = currency,
            ExchangeRate = template.Rates.CnyRub,
            MarkupPercent = 0m
        };

        if (currency == PriceCurrency.Rub)
        {
            plan.BudgetFromRub = budgetFromRub;
            plan.BudgetToRub = budgetToRub;
            plan.BudgetFromCny = budgetFromRub is null
                ? null
                : PricingCalculator.TryRubToCny(budgetFromRub.Value, template.Rates.CnyRub, 0m);
            plan.BudgetToCny = budgetToRub is null
                ? null
                : PricingCalculator.TryRubToCny(budgetToRub.Value, template.Rates.CnyRub, 0m);
        }
        else
        {
            plan.BudgetFromCny = budgetFrom;
            plan.BudgetToCny = budgetTo;
        }

        var breakdownFrom = budgetFromRub is null
            ? null
            : ImportBudgetSolver.SolveMaxCarPriceRub(budgetFromRub.Value, template);

        var breakdownTo = budgetToRub is null
            ? null
            : ImportBudgetSolver.SolveMaxCarPriceRub(budgetToRub.Value, template);

        var display = breakdownTo ?? breakdownFrom;
        if (display is not null)
        {
            plan.Lines = display.Lines
                .Where(x => !string.Equals(x.Title, "Автомобиль", StringComparison.Ordinal))
                .ToList();
            plan.ExpensesRub = display.ExpensesRub;
            plan.ExpensesCny = PricingCalculator.TryRubToCny(display.ExpensesRub, template.Rates.CnyRub, 0m);
        }

        if (breakdownFrom is not null)
        {
            plan.CarBudgetFromRub = breakdownFrom.CarPriceRub;
            plan.CarBudgetFromCny = PricingCalculator.TryRubToCny(breakdownFrom.CarPriceRub, template.Rates.CnyRub, 0m);
        }

        if (breakdownTo is not null)
        {
            plan.CarBudgetToRub = breakdownTo.CarPriceRub;
            plan.CarBudgetToCny = PricingCalculator.TryRubToCny(breakdownTo.CarPriceRub, template.Rates.CnyRub, 0m);
        }

        return plan;
    }
}



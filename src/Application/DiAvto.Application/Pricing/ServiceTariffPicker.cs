using DiAvto.Application.Formatting;
using DiAvto.Domain;


namespace DiAvto.Application.Pricing;

/// <summary>Выбранные пользователем тарифы для расчёта расходов.</summary>
public readonly record struct ServiceTariffSelection(
    int? DeliveryToVladivostok,
    int? DeliveryFromVladivostok,
    int? Inspection,
    int? CustomsClearance)
{
    public int? For(ServiceTariffKind kind) => kind switch
    {
        ServiceTariffKind.DeliveryToVladivostok => DeliveryToVladivostok,
        ServiceTariffKind.DeliveryFromVladivostok => DeliveryFromVladivostok,
        ServiceTariffKind.Inspection => Inspection,
        ServiceTariffKind.CustomsClearance => CustomsClearance,
        _ => null
    };
}

public sealed record ServiceExpenseSnapshot(
    IReadOnlyList<ServiceExpense> Expenses,
    IReadOnlyList<ServiceTariffKind> MissingKinds);

/// <summary>Статья расходов, подготовленная для показа и расчёта бюджета.</summary>
public sealed class ServiceExpense
{
    public ServiceTariffKind Kind { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal AmountRub { get; set; }
}

public static class ServiceTariffPicker
{
    /// <summary>
    /// Подбирает тариф по городу автомобиля, затем тариф по умолчанию,
    /// затем первый доступный. Возвращает null, если список пуст.
    /// </summary>
    public static ServiceTariff? PickBestMatch(IEnumerable<ServiceTariff> tariffs, string? carCity)
    {
        var list = tariffs
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToList();

        if (list.Count == 0)
        {
            return null;
        }

        var translatedCity = CarSpecTranslator.Translate(carCity);
        var candidates = new[] { translatedCity, carCity }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in candidates)
        {
            var match = list.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x.Title) &&
                (x.Title.Contains(candidate!, StringComparison.OrdinalIgnoreCase) ||
                 candidate!.Contains(x.Title, StringComparison.OrdinalIgnoreCase)));

            if (match is not null)
            {
                return match;
            }
        }

        return PickDefault(list);
    }

    /// <summary>Тариф по умолчанию, иначе первый в отсортированном списке.</summary>
    public static ServiceTariff? PickDefault(IEnumerable<ServiceTariff> tariffs)
    {
        var list = tariffs
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToList();

        return list.FirstOrDefault(x => x.IsDefault) ?? list.FirstOrDefault();
    }

    /// <summary>
    /// Разворачивает выбор тарифов в снимок расходов. Если пользователь не выбрал
    /// тариф, берётся тариф по умолчанию, а при его отсутствии — первый доступный.
    /// </summary>
    public static ServiceExpenseSnapshot BuildExpenses(
        IEnumerable<ServiceTariff> activeTariffs,
        ServiceTariffSelection selection)
    {
        var tariffs = activeTariffs.ToList();
        var expenses = new List<ServiceExpense>(ServiceTariffKinds.All.Count);
        var missing = new List<ServiceTariffKind>();

        foreach (var kind in ServiceTariffKinds.All)
        {
            var tariff = Resolve(tariffs, kind, selection.For(kind));
            if (tariff is null)
            {
                missing.Add(kind);
                continue;
            }

            expenses.Add(new ServiceExpense
            {
                Kind = kind,
                Title = tariff.Title,
                AmountRub = tariff.AmountRub
            });
        }

        return new ServiceExpenseSnapshot(expenses, missing);
    }

    private static ServiceTariff? Resolve(
        IReadOnlyList<ServiceTariff> tariffs,
        ServiceTariffKind kind,
        int? selectedId)
    {
        var byKind = tariffs
            .Where(x => x.Kind == kind)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToList();

        if (byKind.Count == 0)
        {
            return null;
        }

        if (selectedId is > 0)
        {
            var selected = byKind.FirstOrDefault(x => x.Id == selectedId.Value);
            if (selected is not null)
            {
                return selected;
            }
        }

        return PickDefault(byKind);
    }
}




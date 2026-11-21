using System.Text.Json.Serialization;

namespace DiAvto.Application.Pricing;

public sealed class ImportCurrencyRates
{
    public decimal CnyRub { get; set; }
    public decimal EurRub { get; set; }
    public decimal JpyRub { get; set; }
    public decimal KrwRub { get; set; }

    public decimal RateFor(string currency) => currency.Trim().ToUpperInvariant() switch
    {
        "RUB" => 1m,
        "CNY" => CnyRub,
        "EUR" => EurRub,
        "JPY" => JpyRub,
        "KRW" => KrwRub,
        _ => 0m
    };
}

public sealed class ImportPricingParameters
{
    public decimal CarPrice { get; set; }
    public string CarCurrency { get; set; } = "CNY";

    public decimal DeliveryToVladivostokAmount { get; set; }
    public string DeliveryToVladivostokCurrency { get; set; } = "CNY";

    public decimal DeliveryToCityRub { get; set; }
    public bool IncludeDeliveryToCity { get; set; }

    public int EngineCc { get; set; } = 2000;
    public int PowerHp { get; set; } = 150;
    public int VehicleYear { get; set; } = DateTime.UtcNow.Year - 3;
    public bool IsDiesel { get; set; }
    public bool IsElectric { get; set; }
    public bool IsLegalEntity { get; set; }
    public bool UseResaleUtilization { get; set; }

    public decimal InspectionCny { get; set; } = 900m;
    public decimal BrokerLaboratoryRub { get; set; } = 140_000m;
    public decimal CommissionBaseRub { get; set; } = 150_000m;
    public decimal CommissionThresholdRub { get; set; } = 3_000_000m;
    public decimal CommissionStepRub { get; set; } = 100_000m;
    public decimal CommissionStepAmountRub { get; set; } = 10_000m;
    public decimal BankCommissionPercent { get; set; } = 2.5m;

    public ImportCurrencyRates Rates { get; set; } = new();
}

public sealed class ImportCostLine
{
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal AmountRub { get; set; }
}

public sealed class ImportCostBreakdown
{
    public decimal CarPriceRub { get; set; }
    public decimal InspectionRub { get; set; }
    public decimal DeliveryToVladivostokRub { get; set; }
    public decimal DeliveryToCityRub { get; set; }
    public CustomsCalculation Customs { get; set; } = new();
    public decimal BrokerLaboratoryRub { get; set; }
    public decimal OurCommissionRub { get; set; }
    public decimal BankCommissionRub { get; set; }
    public decimal TotalRub { get; set; }
    public List<ImportCostLine> Lines { get; set; } = [];

    [JsonIgnore]
    public decimal ExpensesRub => TotalRub - CarPriceRub;
}

public static class ImportPricingCalculator
{
    public static ImportCostBreakdown Calculate(ImportPricingParameters p)
    {
        var carPriceRub = ConvertToRub(p.CarPrice, p.CarCurrency, p.Rates);
        var deliveryRub = ConvertToRub(p.DeliveryToVladivostokAmount, p.DeliveryToVladivostokCurrency, p.Rates);
        var inspectionRub = ConvertToRub(p.InspectionCny, "CNY", p.Rates);
        var customs = CustomsCalculator.Calculate(
            carPriceRub,
            p.EngineCc,
            p.PowerHp,
            p.VehicleYear,
            p.IsDiesel,
            p.IsElectric,
            p.IsLegalEntity,
            p.UseResaleUtilization,
            p.Rates.EurRub);
        var broker = Round(p.BrokerLaboratoryRub);
        var commission = CalculateCommission(carPriceRub, p);
        var deliveryToCity = p.IncludeDeliveryToCity ? Round(p.DeliveryToCityRub) : 0m;

        var subtotal = carPriceRub + inspectionRub + deliveryRub + customs.TotalRub +
                       broker + commission + deliveryToCity;
        var bank = Round(subtotal * p.BankCommissionPercent / 100m);
        var total = Round(subtotal + bank);

        var breakdown = new ImportCostBreakdown
        {
            CarPriceRub = carPriceRub,
            InspectionRub = inspectionRub,
            DeliveryToVladivostokRub = deliveryRub,
            DeliveryToCityRub = deliveryToCity,
            Customs = customs,
            BrokerLaboratoryRub = broker,
            OurCommissionRub = commission,
            BankCommissionRub = bank,
            TotalRub = total
        };

        breakdown.Lines =
        [
            new ImportCostLine { Title = "Автомобиль", Note = FormatCarNote(p, carPriceRub), AmountRub = carPriceRub },
            new ImportCostLine { Title = "Осмотр", Note = $"{p.InspectionCny:N0} ¥", AmountRub = inspectionRub },
            new ImportCostLine { Title = "Доставка до Владивостока", AmountRub = deliveryRub },
            new ImportCostLine
            {
                Title = p.IsLegalEntity ? "Таможенная пошлина (СТП)" : "Таможенная пошлина (ЕТС)",
                AmountRub = customs.DutyRub
            }
        ];

        if (customs.ExciseRub > 0)
        {
            breakdown.Lines.Add(new ImportCostLine
            {
                Title = "Акциз",
                Note = $"{p.PowerHp:N0} л.с.",
                AmountRub = customs.ExciseRub
            });
        }

        if (customs.VatRub > 0)
        {
            breakdown.Lines.Add(new ImportCostLine { Title = "НДС 22%", AmountRub = customs.VatRub });
        }

        breakdown.Lines.Add(new ImportCostLine
        {
            Title = "Утилизационный сбор",
            Note = p.IsLegalEntity
                ? "коммерческий"
                : p.UseResaleUtilization
                    ? "перепродажа"
                    : $"для себя; перепродажа: {customs.ResaleUtilizationRub:N0} ₽",
            AmountRub = customs.UtilizationFeeRub
        });
        breakdown.Lines.Add(new ImportCostLine { Title = "Таможенный сбор", AmountRub = customs.CustomsFeeRub });
        breakdown.Lines.Add(new ImportCostLine { Title = "Брокер + лаборатория", AmountRub = broker });
        breakdown.Lines.Add(new ImportCostLine { Title = "Наша комиссия", AmountRub = commission });
        breakdown.Lines.Add(new ImportCostLine { Title = "Банковская комиссия 2,5%", AmountRub = bank });

        if (deliveryToCity > 0)
        {
            breakdown.Lines.Add(new ImportCostLine
            {
                Title = "Доставка до города клиента",
                AmountRub = deliveryToCity
            });
        }

        return breakdown;
    }

    public static ImportCostBreakdown CalculateForCarPriceRub(ImportPricingParameters template, decimal carPriceRub)
    {
        var copy = new ImportPricingParameters
        {
            CarPrice = carPriceRub,
            CarCurrency = "RUB",
            DeliveryToVladivostokAmount = template.DeliveryToVladivostokAmount,
            DeliveryToVladivostokCurrency = template.DeliveryToVladivostokCurrency,
            DeliveryToCityRub = template.DeliveryToCityRub,
            IncludeDeliveryToCity = template.IncludeDeliveryToCity,
            EngineCc = template.EngineCc,
            PowerHp = template.PowerHp,
            VehicleYear = template.VehicleYear,
            IsDiesel = template.IsDiesel,
            IsElectric = template.IsElectric,
            IsLegalEntity = template.IsLegalEntity,
            UseResaleUtilization = template.UseResaleUtilization,
            InspectionCny = template.InspectionCny,
            BrokerLaboratoryRub = template.BrokerLaboratoryRub,
            CommissionBaseRub = template.CommissionBaseRub,
            CommissionThresholdRub = template.CommissionThresholdRub,
            CommissionStepRub = template.CommissionStepRub,
            CommissionStepAmountRub = template.CommissionStepAmountRub,
            BankCommissionPercent = template.BankCommissionPercent,
            Rates = template.Rates
        };

        return Calculate(copy);
    }

    public static decimal CalculateCommission(decimal carPriceRub, ImportPricingParameters p)
    {
        if (carPriceRub <= p.CommissionThresholdRub)
        {
            return Round(p.CommissionBaseRub);
        }

        var steps = Math.Ceiling(
            (carPriceRub - p.CommissionThresholdRub) / p.CommissionStepRub);

        return Round(p.CommissionBaseRub + steps * p.CommissionStepAmountRub);
    }

    public static decimal ConvertToRub(decimal amount, string currency, ImportCurrencyRates rates)
    {
        var rate = rates.RateFor(currency);
        if (rate <= 0m)
        {
            throw new InvalidOperationException($"Курс {currency}/RUB недоступен.");
        }

        return Round(amount * rate);
    }

    private static string? FormatCarNote(ImportPricingParameters p, decimal carPriceRub)
    {
        if (string.Equals(p.CarCurrency, "RUB", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return $"{p.CarPrice:N0} {p.CarCurrency}";
    }

    private static decimal Round(decimal value)
    {
        return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}

public static class ImportBudgetSolver
{
    /// <summary>
    /// Находит максимальную стоимость автомобиля в рублях, при которой итог
    /// «под ключ» не превышает бюджет. Все платежи монотонно растут вместе
    /// со стоимостью авто, поэтому используем бинарный поиск.
    /// </summary>
    public static ImportCostBreakdown SolveMaxCarPriceRub(
        decimal budgetRub,
        ImportPricingParameters template)
    {
        if (budgetRub <= 0)
        {
            return ImportPricingCalculator.CalculateForCarPriceRub(template, 0m);
        }

        var high = Math.Max(100_000m, budgetRub);
        var guard = 0;
        while (ImportPricingCalculator.CalculateForCarPriceRub(template, high).TotalRub <= budgetRub &&
               high < 200_000_000m && guard++ < 20)
        {
            high *= 2m;
        }

        var low = 0m;
        for (var i = 0; i < 70; i++)
        {
            var middle = (low + high) / 2m;
            var total = ImportPricingCalculator.CalculateForCarPriceRub(template, middle).TotalRub;
            if (total <= budgetRub)
            {
                low = middle;
            }
            else
            {
                high = middle;
            }
        }

        return ImportPricingCalculator.CalculateForCarPriceRub(template, low);
    }
}



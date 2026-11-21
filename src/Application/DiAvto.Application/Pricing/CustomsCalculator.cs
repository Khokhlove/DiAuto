using System.Text.Json.Serialization;

namespace DiAvto.Application.Pricing;

public sealed class CustomsCalculation
{
    public decimal CustomsValueRub { get; set; }
    public decimal DutyRub { get; set; }
    public decimal ExciseRub { get; set; }
    public decimal VatRub { get; set; }
    public decimal UtilizationFeeRub { get; set; }
    public decimal CustomsFeeRub { get; set; }

    /// <summary>Утильсбор для личного пользования (если применим).</summary>
    public decimal PersonalUtilizationRub { get; set; }

    /// <summary>Утильсбор для перепродажи (коммерческая шкала).</summary>
    public decimal ResaleUtilizationRub { get; set; }

    public decimal TotalPersonalRub { get; set; }
    public decimal TotalResaleRub { get; set; }

    public bool IsLegalEntity { get; set; }
    public bool UseResaleUtilization { get; set; }

    [JsonIgnore]
    public decimal TotalRub => IsLegalEntity
        ? TotalResaleRub
        : UseResaleUtilization
            ? TotalResaleRub
            : TotalPersonalRub;
}

/// <summary>
/// Таможенные платежи 2026 года.
/// Физлицо: единая таможенная ставка (ЕТС), без отдельного акциза и НДС.
/// Юрлицо: пошлина СТП + акциз + НДС 22% + коммерческий утильсбор.
/// </summary>
public static class CustomsCalculator
{
    private const decimal UtilizationBaseRub = 20_000m;
    private const decimal VatPercent = 22m;

    // Границы мощности для коммерческого утильсбора, л.с.
    private static readonly decimal[] PowerBands =
    [
        160m, 187.26m, 216.8m, 246.39m, 275.95m, 305.52m,
        335.09m, 364.65m, 394.22m, 423.78m, 453.36m, 492.78m
    ];

    private static readonly decimal[][] UtilizationPetrolUnder3 =
    [
        [14.88m, 15.36m, 15.84m, 16.2m, 17.28m, 17.28m, 17.28m, 17.28m, 17.28m, 17.28m, 17.28m, 17.28m, 17.28m],
        [40.04m, 45m, 47.64m, 50.52m, 57.12m, 64.56m, 72.96m, 83.16m, 94.8m, 108m, 123.24m, 140.4m, 160.08m],
        [112.52m, 115.34m, 118.2m, 120.12m, 126m, 131.04m, 136.32m, 141.72m, 147.48m, 153.36m, 159.48m, 165.84m, 172.44m],
        [129.2m, 131.76m, 134.4m, 137.16m, 140.52m, 144m, 151.92m, 160.32m, 169.2m, 178.44m, 188.28m, 198.6m, 209.52m],
        [164.53m, 167.28m, 170.16m, 173.04m, 176.52m, 180m, 186.36m, 192.88m, 199.68m, 206.64m, 213.84m, 221.28m, 229.08m]
    ];

    private static readonly decimal[][] UtilizationPetrolOver3 =
    [
        [27.6m, 28.43m, 29.28m, 30.12m, 30.12m, 30.12m, 30.12m, 30.12m, 30.12m, 30.12m, 30.12m, 30.12m, 30.12m],
        [70.44m, 74.64m, 79.2m, 83.88m, 91.92m, 100.56m, 110.16m, 120.6m, 132m, 144.6m, 158.4m, 173.4m, 189.84m],
        [170.36m, 172.8m, 175.08m, 177.6m, 183m, 188.52m, 193.68m, 199.08m, 204.72m, 210.48m, 216.36m, 222.36m, 228.6m],
        [197.81m, 200.04m, 202.2m, 204.36m, 207.24m, 212.4m, 217.8m, 224.28m, 231m, 237.96m, 245.04m, 252.48m, 260.04m],
        [216.29m, 219.48m, 222.84m, 226.2m, 231.36m, 236.64m, 249.6m, 263.4m, 277.92m, 293.16m, 309.36m, 326.4m, 344.28m]
    ];

    public static CustomsCalculation Calculate(
        decimal customsValueRub,
        int engineCc,
        int powerHp,
        int vehicleYear,
        bool isDiesel,
        bool isElectric,
        bool isLegalEntity,
        bool useResaleUtilization,
        decimal eurRubRate)
    {
        if (customsValueRub <= 0 || engineCc <= 0 || powerHp <= 0 || eurRubRate <= 0)
        {
            return new CustomsCalculation { CustomsValueRub = Math.Max(0m, customsValueRub) };
        }

        var age = Math.Max(0, DateTime.UtcNow.Year - vehicleYear);
        var fee = CalculateCustomsFee(customsValueRub);
        var personalUtilization = CalculatePersonalUtilization(engineCc, powerHp, age, isElectric);
        var resaleUtilization = isElectric
            ? CalculateElectricUtilization(powerHp)
            : CalculateUtilization(engineCc, powerHp, age);

        if (isLegalEntity)
        {
            var duty = CalculateLegalDuty(customsValueRub, engineCc, age, isDiesel, eurRubRate);
            var excise = CalculateExcise(powerHp);
            var vat = Round((customsValueRub + duty + excise) * VatPercent / 100m);
            var total = Round(duty + excise + vat + resaleUtilization + fee);

            return new CustomsCalculation
            {
                CustomsValueRub = Round(customsValueRub),
                DutyRub = duty,
                ExciseRub = excise,
                VatRub = vat,
                UtilizationFeeRub = resaleUtilization,
                CustomsFeeRub = fee,
                PersonalUtilizationRub = personalUtilization,
                ResaleUtilizationRub = resaleUtilization,
                TotalPersonalRub = total,
                TotalResaleRub = total,
                IsLegalEntity = true,
                UseResaleUtilization = true
            };
        }

        var etsDuty = CalculateEtsDuty(customsValueRub, engineCc, age, eurRubRate);
        var totalPersonal = Round(etsDuty + personalUtilization + fee);
        var totalResale = Round(etsDuty + resaleUtilization + fee);

        return new CustomsCalculation
        {
            CustomsValueRub = Round(customsValueRub),
            DutyRub = etsDuty,
            ExciseRub = 0m,
            VatRub = 0m,
            UtilizationFeeRub = useResaleUtilization ? resaleUtilization : personalUtilization,
            CustomsFeeRub = fee,
            PersonalUtilizationRub = personalUtilization,
            ResaleUtilizationRub = resaleUtilization,
            TotalPersonalRub = totalPersonal,
            TotalResaleRub = totalResale,
            IsLegalEntity = false,
            UseResaleUtilization = useResaleUtilization
        };
    }

    public static decimal CalculateEtsDuty(
        decimal customsValueRub,
        int engineCc,
        int age,
        decimal eurRubRate)
    {
        if (age < 3)
        {
            var valueEur = customsValueRub / eurRubRate;
            var (percent, minPerCc) = valueEur switch
            {
                <= 8_500m => (54m, 2.5m),
                <= 16_700m => (48m, 3.5m),
                <= 42_300m => (48m, 5.5m),
                <= 84_500m => (48m, 7.5m),
                <= 169_000m => (48m, 15m),
                _ => (48m, 20m)
            };

            return Round(Math.Max(
                customsValueRub * percent / 100m,
                engineCc * minPerCc * eurRubRate));
        }

        var perCc = age < 5
            ? engineCc <= 1000 ? 1.5m
                : engineCc <= 1500 ? 1.7m
                : engineCc <= 1800 ? 2.5m
                : engineCc <= 2300 ? 2.7m
                : engineCc <= 3000 ? 3.0m
                : 3.6m
            : engineCc <= 1000 ? 3.0m
                : engineCc <= 1500 ? 3.2m
                : engineCc <= 1800 ? 3.5m
                : engineCc <= 2300 ? 4.8m
                : engineCc <= 3000 ? 5.0m
                : 5.7m;

        return Round(engineCc * perCc * eurRubRate);
    }

    public static decimal CalculateLegalDuty(
        decimal customsValueRub,
        int engineCc,
        int age,
        bool isDiesel,
        decimal eurRubRate)
    {
        if (age < 3)
        {
            var percent = isDiesel || engineCc <= 2800 ? 15m : 12.5m;
            return Round(customsValueRub * percent / 100m);
        }

        if (age < 7)
        {
            var minimumPerCc = isDiesel
                ? engineCc <= 1500 ? 0.32m : engineCc <= 2500 ? 0.40m : 0.80m
                : engineCc <= 1000 ? 0.36m
                    : engineCc <= 1500 ? 0.40m
                    : engineCc <= 1800 ? 0.36m
                    : engineCc <= 3000 ? 0.44m
                    : 0.80m;

            var byValue = customsValueRub * 20m / 100m;
            var byEngine = engineCc * minimumPerCc * eurRubRate;
            return Round(Math.Max(byValue, byEngine));
        }

        var perCc = isDiesel
            ? engineCc <= 1500 ? 1.50m : engineCc <= 2500 ? 2.20m : 3.20m
            : engineCc <= 1000 ? 1.40m
                : engineCc <= 1500 ? 1.50m
                : engineCc <= 1800 ? 1.60m
                : engineCc <= 3000 ? 2.20m
                : 3.20m;

        return Round(engineCc * perCc * eurRubRate);
    }

    public static decimal CalculateExcise(int powerHp)
    {
        var rate = powerHp <= 90 ? 0m
            : powerHp <= 150 ? 64m
            : powerHp <= 200 ? 613m
            : powerHp <= 300 ? 1004m
            : powerHp <= 400 ? 1711m
            : powerHp <= 500 ? 1771m
            : 1829m;

        return Round(powerHp * rate);
    }

    public static decimal CalculatePersonalUtilization(int engineCc, int powerHp, int age, bool isElectric)
    {
        if (isElectric)
        {
            return powerHp <= 80
                ? (age < 3 ? 3_400m : 5_200m)
                : CalculateElectricUtilization(powerHp);
        }

        if (engineCc <= 3000 && powerHp <= 160)
        {
            return age < 3 ? 3_400m : 5_200m;
        }

        return CalculateUtilization(engineCc, powerHp, age);
    }

    public static decimal CalculateUtilization(int engineCc, int powerHp, int age)
    {
        var volumeIndex = engineCc <= 1000 ? 0
            : engineCc <= 2000 ? 1
            : engineCc <= 3000 ? 2
            : engineCc <= 3500 ? 3
            : 4;

        var powerIndex = 0;
        while (powerIndex < PowerBands.Length && powerHp > PowerBands[powerIndex])
        {
            powerIndex++;
        }

        powerIndex = Math.Min(powerIndex, PowerBands.Length);
        var coefficients = age < 3 ? UtilizationPetrolUnder3 : UtilizationPetrolOver3;
        return Round(UtilizationBaseRub * coefficients[volumeIndex][powerIndex]);
    }

    public static decimal CalculateCustomsFee(decimal customsValueRub)
    {
        return customsValueRub switch
        {
            <= 200_000m => 1_231m,
            <= 450_000m => 2_462m,
            <= 1_200_000m => 4_924m,
            <= 2_700_000m => 13_541m,
            <= 4_200_000m => 18_465m,
            <= 5_500_000m => 21_344m,
            <= 10_000_000m => 49_240m,
            _ => 73_860m
        };
    }

    private static decimal CalculateElectricUtilization(int powerHp)
    {
        return powerHp <= 80 ? 667_400m
            : powerHp <= 100 ? 667_400m
            : powerHp <= 130 ? 1_098_000m
            : powerHp <= 160 ? 1_300_000m
            : powerHp <= 190 ? 1_540_000m
            : powerHp <= 220 ? 1_828_000m
            : powerHp <= 250 ? 2_166_000m
            : powerHp <= 280 ? 2_566_000m
            : 3_040_000m;
    }

    private static decimal Round(decimal value)
    {
        return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}



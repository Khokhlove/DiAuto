using System.Globalization;
using System.Text.RegularExpressions;

namespace DiAvto.Web.Features.Formatting;

/// <summary>Извлекает из китайских строк объём, мощность и тип топлива.</summary>
public static partial class CarSpecsParser
{
    public static int ParseEngineCc(string? displacement)
    {
        if (string.IsNullOrWhiteSpace(displacement))
        {
            return 2000;
        }

        var match = NumberRegex().Match(displacement);
        if (!match.Success ||
            !decimal.TryParse(match.Value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            return 2000;
        }

        return value < 20m
            ? (int)Math.Round(value * 1000m, MidpointRounding.AwayFromZero)
            : (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    public static int ParsePowerHp(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var match = PowerRegex().Match(text);
        return match.Success && int.TryParse(match.Groups[1].Value, out var hp)
            ? hp
            : 0;
    }

    public static (bool IsDiesel, bool IsElectric) DetectFuel(string? fuelType, bool isNewEnergy)
    {
        if (string.IsNullOrWhiteSpace(fuelType))
        {
            return (false, false);
        }

        var diesel = fuelType.Contains("柴油", StringComparison.Ordinal);
        var electric = fuelType.Contains("电动", StringComparison.Ordinal) ||
                       fuelType.Contains("纯电", StringComparison.Ordinal);
        return (diesel, electric);
    }

    [GeneratedRegex(@"\d+(?:[.,]\d+)?", RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();

    [GeneratedRegex(@"(\d{2,4})\s*(?:马力|匹|hp|HP|ps|PS)", RegexOptions.CultureInvariant)]
    private static partial Regex PowerRegex();
}

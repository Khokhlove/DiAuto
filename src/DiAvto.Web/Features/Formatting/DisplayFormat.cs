using System.Globalization;

namespace DiAvto.Web.Features.Formatting;

public static class DisplayFormat
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

    public static string Cny(decimal amount)
    {
        return amount.ToString("N0", Ru) + " ¥";
    }

    public static string Rub(decimal amount)
    {
        return "≈ " + amount.ToString("N0", Ru) + " ₽";
    }

    public static string RubExact(decimal amount)
    {
        return amount.ToString("N0", Ru) + " ₽";
    }

    public static string RubRate(decimal rate)
    {
        return rate.ToString("N3", Ru) + " ₽";
    }

    public static string Mileage(int? km)
    {
        return km is null ? "—" : km.Value.ToString("N0", Ru) + " км";
    }

    public static string MileageShort(int? km)
    {
        if (km is null)
        {
            return "—";
        }

        return km.Value < 10_000
            ? km.Value.ToString("N0", Ru) + " км"
            : (km.Value / 10_000m).ToString("0.#", Ru) + " тыс. км";
    }

    public static string Number(decimal value, string format = "N0")
    {
        return value.ToString(format, Ru);
    }

    public static string Date(DateTimeOffset value)
    {
        return value.ToLocalTime().ToString("dd.MM.yyyy HH:mm", Ru);
    }

    public static string Date(DateTime value)
    {
        return value.ToString("dd.MM.yyyy", Ru);
    }

    /// <summary>Дата в локальном часовом поясе сервера для значений, сохранённых в UTC.</summary>
    public static string DateLocal(DateTime utcValue)
    {
        return DateTime.SpecifyKind(utcValue, DateTimeKind.Utc)
            .ToLocalTime()
            .ToString("dd.MM.yyyy", Ru);
    }

    public static string Percent(decimal value)
    {
        return value.ToString("0.##", Ru) + "%";
    }

    public static string PriceRange(decimal? fromCny, decimal? toCny)
    {
        if (fromCny is null && toCny is null)
        {
            return "Любая цена";
        }

        if (fromCny is not null && toCny is not null)
        {
            return $"{fromCny.Value.ToString("N0", Ru)} – {toCny.Value.ToString("N0", Ru)} ¥";
        }

        return fromCny is not null
            ? $"от {fromCny.Value.ToString("N0", Ru)} ¥"
            : $"до {toCny!.Value.ToString("N0", Ru)} ¥";
    }

    public static string StatusRu(Domain.SearchStatus status) => status switch
    {
        Domain.SearchStatus.Pending => "В очереди",
        Domain.SearchStatus.Running => "Выполняется",
        Domain.SearchStatus.Completed => "Готово",
        Domain.SearchStatus.Failed => "Ошибка",
        _ => status.ToString()
    };

    public static string TierRu(Domain.ResultTier tier) => tier switch
    {
        Domain.ResultTier.Cheapest => "Самые дешёвые",
        Domain.ResultTier.MostExpensive => "Самые дорогие",
        _ => tier.ToString()
    };
}

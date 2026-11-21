namespace DiAvto.UI.Formatting;

public static class SiteFormat
{
    public static string Money(decimal amount) => amount.ToString("N0") + " ₽";

    public static string Mileage(int km) => km <= 0 ? "0 км" : km.ToString("N0") + " км";

    public static string Short(string? value, int length = 120) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Length <= length
                ? value
                : value[..length].TrimEnd() + "…";
}


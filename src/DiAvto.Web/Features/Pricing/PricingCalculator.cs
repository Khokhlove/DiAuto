namespace DiAvto.Web.Features.Pricing;

public readonly record struct ClientCardTotals(decimal CarPriceRub, decimal TotalRub);

/// <summary>Общие формулы пересчёта валют и стоимости автомобиля под ключ.</summary>
public static class PricingCalculator
{
    /// <summary>Курс с наценкой: сколько рублей стоит один юань для клиента.</summary>
    public static decimal EffectiveRate(decimal exchangeRate, decimal markupPercent)
    {
        return decimal.Round(
            exchangeRate * (1m + markupPercent / 100m),
            4,
            MidpointRounding.AwayFromZero);
    }

    public static decimal CnyToRub(decimal priceCny, decimal exchangeRate, decimal markupPercent)
    {
        return decimal.Round(
            priceCny * exchangeRate * (1m + markupPercent / 100m),
            2,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>Переводит рубли в юани по курсу с наценкой. null, если курс не задан.</summary>
    public static decimal? TryRubToCny(decimal amountRub, decimal exchangeRate, decimal markupPercent)
    {
        if (exchangeRate <= 0m)
        {
            return null;
        }

        return decimal.Round(
            amountRub / (exchangeRate * (1m + markupPercent / 100m)),
            2,
            MidpointRounding.AwayFromZero);
    }

    public static ClientCardTotals CalculateClientCard(
        decimal priceCny,
        decimal exchangeRate,
        decimal markupPercent,
        decimal deliveryToVladivostokRub,
        decimal deliveryFromVladivostokRub,
        decimal inspectionRub,
        decimal customsClearanceRub)
    {
        var carPriceRub = CnyToRub(priceCny, exchangeRate, markupPercent);

        var totalRub = decimal.Round(
            carPriceRub +
            deliveryToVladivostokRub +
            deliveryFromVladivostokRub +
            inspectionRub +
            customsClearanceRub,
            2,
            MidpointRounding.AwayFromZero);

        return new ClientCardTotals(carPriceRub, totalRub);
    }
}

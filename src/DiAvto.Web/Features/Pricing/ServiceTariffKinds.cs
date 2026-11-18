using DiAvto.Web.Domain;

namespace DiAvto.Web.Features.Pricing;

/// <summary>
/// Единый справочник типов сервисных тарифов: подписи, подсказки и порядок вывода.
/// Используется и в админке, и в расчётах, чтобы формулировки не расходились.
/// </summary>
public static class ServiceTariffKinds
{
    public static IReadOnlyList<ServiceTariffKind> All { get; } =
    [
        ServiceTariffKind.DeliveryToVladivostok,
        ServiceTariffKind.DeliveryFromVladivostok,
        ServiceTariffKind.Inspection,
        ServiceTariffKind.CustomsClearance
    ];

    public static string Title(ServiceTariffKind kind) => kind switch
    {
        ServiceTariffKind.DeliveryToVladivostok => "Доставка до Владивостока",
        ServiceTariffKind.DeliveryFromVladivostok => "Доставка из Владивостока",
        ServiceTariffKind.Inspection => "Осмотр автомобиля",
        ServiceTariffKind.CustomsClearance => "Растаможка автомобиля",
        _ => kind.ToString()
    };

    public static string GroupHint(ServiceTariffKind kind) => kind switch
    {
        ServiceTariffKind.DeliveryToVladivostok => "Из региона покупки",
        ServiceTariffKind.DeliveryFromVladivostok => "До города клиента",
        ServiceTariffKind.Inspection => "В регионе покупки",
        ServiceTariffKind.CustomsClearance => "Таможенное оформление",
        _ => "Услуга"
    };

    public static string TitlePlaceholder(ServiceTariffKind kind) => kind switch
    {
        ServiceTariffKind.DeliveryToVladivostok => "Например: Гуанчжоу",
        ServiceTariffKind.DeliveryFromVladivostok => "Например: Москва",
        ServiceTariffKind.Inspection => "Например: Гуанчжоу",
        ServiceTariffKind.CustomsClearance => "Например: Базовая растаможка",
        _ => "Название"
    };

    public static string TitleHelp(ServiceTariffKind kind) => kind switch
    {
        ServiceTariffKind.DeliveryToVladivostok =>
            "Укажите город или регион покупки в Китае. Для общих условий используйте «Китай (базовый)».",
        ServiceTariffKind.DeliveryFromVladivostok =>
            "Укажите город, в который доставляем автомобиль из Владивостока.",
        ServiceTariffKind.Inspection =>
            "Укажите регион осмотра. Тариф подбирается по городу автомобиля, если названия совпадают.",
        ServiceTariffKind.CustomsClearance =>
            "Стоимость таможенного оформления. Можно завести один базовый тариф или несколько под разные категории автомобилей.",
        _ => string.Empty
    };
}

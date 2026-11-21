namespace DiAvto.Application.Content;

public sealed record SiteNavLink(string Label, string Url);

public sealed record SiteStat(string Value, string Label);

public sealed record HeroSlide(string ImageUrl, string Title, string Subtitle);

public sealed record PartnerPlatform(string Name, string LogoUrl, string Url);

public sealed record CommissionCategory(string Title, IReadOnlyList<string> Items);

public sealed record ClientBenefit(string Title, string Description);

public sealed record TrustBadge(string Text);

public sealed record CtaBanner(string Slug, string Headline, string Description, string ButtonText);

/// <summary>
/// Статический контент визитки, перенесённый из исходного сайта.
/// Динамические сущности (автомобили, отзывы, офисы, настройки) живут в БД.
/// </summary>
public static class SiteContent
{
    public const string CompanyName = "ДИ.авто";
    public const string CompanyTagline = "импорт автомобилей из Азии";
    public const string CompanyDescription = "Импорт автомобилей из Японии, Кореи и Китая. Надёжно, прозрачно, выгодно.";
    public const string Phone = "+7 (923) 317-77-79";
    public const string Phone2 = "+7 (923) 370-38-77";
    public const string Email = "zakaz@diautogo.ru";
    public const string WhatsAppUrl = "https://wa.me/79233177779";
    public const string TelegramUrl = "https://t.me/+79233177779";
    public const string VkUrl = "https://vk.ru/diauto1";
    public const string MaxUrl = "https://max.ru/diauto";
    public const string LegalEntity = "ИП Кулик А.С.";
    public const string CarActionLabel = "Оставить заявку";
    public const string CarPriceDisclaimer = "Автомобиль ориентировочно. Финальная цена является предварительной.";
    public const string InStockLabel = "В наличии";
    public const string OrderLabel = "Под заказ";
    public const string FormResponseTime = "Ответим в течение 15 минут в рабочее время";

    public static IReadOnlyList<SiteNavLink> NavLinks { get; } =
    [
        new("Каталог", "/catalog"),
        new("Этапы работы", "/stages-of-work"),
        new("Автокредит", "/auto-credit"),
        new("Отзывы", "/reviews"),
        new("О нас", "/about"),
        new("Контакты", "/contacts")
    ];

    public static IReadOnlyList<SiteStat> Stats { get; } =
    [
        new("450+", "Автомобилей доставлено"),
        new("5", "Лет на рынке"),
        new("190+", "Довольных клиентов"),
        new("3", "Офиса в РФ")
    ];

    public static IReadOnlyList<HeroSlide> HeroSlides { get; } =
    [
        new("/images/hero/hero-1.webp", "Автомобили из Японии, Кореи и Китая",
            "Привозим машины под ключ. Полная проверка, выкуп, доставка и оформление."),
        new("/images/hero/hero-2.webp", "Импорт автомобилей из Азии",
            "Честная цена, проверенная история и полное сопровождение на всём пути."),
        new("/images/hero/hero-3.webp", "Под ключ — от выбора до номеров",
            "Подберём, проверим, выкупим, доставим и поставим на учёт. Вам останется только сесть за руль.")
    ];

    public static IReadOnlyList<string> Countries { get; } = ["Япония", "Корея", "Китай"];

    public static IReadOnlyList<string> BodyTypes { get; } =
        ["Седан", "Кроссовер", "Хэтчбек", "Универсал", "Минивэн", "Купе", "Кабриолет", "Пикап", "Кроссовер", "Спорткар"];

    public static IReadOnlyList<string> Transmissions { get; } = ["Автомат", "Робот", "Вариатор", "Механика"];

    public static IReadOnlyList<string> FuelTypes { get; } = ["Бензин", "Дизель", "Гибрид", "Электро"];

    public static IReadOnlyList<string> CarMakes { get; } =
    [
        "Chevrolet", "Dongfeng", "Ford", "Geely", "Haval", "Honda", "Hyundai", "Jetour", "Jetta", "Kia",
        "Land Rover", "Lexus", "Mazda", "Mercedes-Benz", "MG", "Mini", "Nissan", "Peugeot", "Renault",
        "Skoda", "Toyota", "Volkswagen"
    ];

    public static decimal PriceMin { get; } = 500_000m;
    public static decimal PriceMax { get; } = 10_000_000m;
    public static decimal PriceStep { get; } = 50_000m;
    public static int YearMin { get; } = 2010;

    public static IReadOnlyList<string> TurnkeyServices { get; } =
    [
        "Подбор авто на проверенных площадках Кореи, Японии и Китая",
        "Участие в аукционах и покупка у дилеров",
        "Полный осмотр и техническая диагностика",
        "Доставка морем и автовозом в любой город РФ",
        "Организация постановки на учёт с гос. номерами",
        "Комплексное ТО и антикоррозийная обработка"
    ];

    public static IReadOnlyList<ClientBenefit> ClientBenefits { get; } =
    [
        new("Прозрачность", "Фото- и видеоотчёт на каждом этапе. Вы видите весь процесс."),
        new("Экономия", "Контролируем все расходы — итоговая цена «под ключ» ниже рынка."),
        new("Гарантия", "Глубокая диагностика и выкуп авто при несоответствии заявленному состоянию."),
        new("Удобство", "Весь процесс «под ключ» — от подбора до получения готового автомобиля.")
    ];

    public static IReadOnlyList<CommissionCategory> CommissionServices { get; } =
    [
        new("Основные услуги",
        [
            "Комплексный подбор автомобиля на территории другой страны",
            "Проверка состояния автомобиля",
            "Снятие его с учёта",
            "Подготовка экспортной документации в РФ",
            "Логистика до РФ",
            "Подготовка всех документов",
            "Прохождение лаборатории",
            "Получение документов ЭПТС, СБКТС, таможенной декларации",
            "Транспортировка автомобиля к нам на стоянку",
            "Транспортировка автомобиля на автовоз",
            "Бесплатная стоянка до 30 дней"
        ]),
        new("Организация услуг",
        [
            "Проведение комплексного ТО в РФ",
            "Проведение комплекса услуг шиномонтажа",
            "Подготовка автомобиля к перегону (оклейка зон риска)",
            "Замена комплектации в Китае, перешивка салона под нужный клиенту, замена руля, сидений, добавление опций"
        ]),
        new("Дополнительные услуги",
        [
            "Постановка на учёт в ГИБДД",
            "Замена государственного номера"
        ])
    ];

    public static IReadOnlyList<PartnerPlatform> PartnerPlatforms { get; } =
    [
        new("Encar", "/images/platforms/encar.webp", "https://www.encar.com/index.do"),
        new("KB Cha Cha Cha", "/images/platforms/kbchachacha.webp", "https://www.kbchachacha.com/"),
        new("Che168", "/images/platforms/che168.webp", "https://www.che168.com/shangqiu/"),
        new("Dongchedi", "/images/platforms/dongchedi.webp", "https://www.dongchedi.com/"),
        new("K-Car", "/images/platforms/kcar.webp", "https://www.kcar.com/")
    ];

    public static IReadOnlyList<TrustBadge> TrustBadges { get; } =
    [
        new("Бесплатная консультация"),
        new("Без предоплаты")
    ];

    public static IReadOnlyList<CtaBanner> CtaBanners { get; } =
    [
        new("cta_consultation", "Бесплатная консультация", "Ответим на все вопросы и поможем определиться с выбором", "Получить консультацию"),
        new("cta_calculation", "Расчёт стоимости за 5 минут", "Рассчитаем полную стоимость «под ключ» с доставкой и таможней", "Рассчитать стоимость"),
        new("cta_refund", "100% возврат предоплаты", "Гарантируем полный возврат предоплаты при отказе от покупки", "Узнать подробнее"),
        new("cta_selection", "Бесплатный подбор авто", "Подберём автомобиль по вашим параметрам — бесплатно и без обязательств", "Получить подборку")
    ];

    public static IReadOnlyList<(string Label, string Value)> WorkingHours { get; } =
    [
        ("Пн–Пт", "09:00–19:00"),
        ("Сб–Вс", "выходной")
    ];
}

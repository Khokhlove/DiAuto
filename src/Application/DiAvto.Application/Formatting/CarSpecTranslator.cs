using DiAvto.Application.Catalog;

namespace DiAvto.Application.Formatting;

/// <summary>
/// Переводит распространённые китайские значения характеристик автомобиля на русский.
/// Неизвестные значения возвращает без изменений.
/// </summary>
public static class CarSpecTranslator
{
    private static readonly Dictionary<string, string> Values = new(StringComparer.Ordinal)
    {
        // Топливо
        ["汽油"] = "Бензин",
        ["柴油"] = "Дизель",
        ["油电混合"] = "Гибрид",
        ["混合动力"] = "Гибрид",
        ["插电式混合动力"] = "Подключаемый гибрид",
        ["插电混动"] = "Подключаемый гибрид",
        ["纯电动"] = "Электро",
        ["电动"] = "Электро",
        ["新能源"] = "Электро/гибрид",
        ["天然气"] = "Газ",
        ["液化石油气"] = "Газ (LPG)",
        ["氢燃料"] = "Водород",
        ["甲醇"] = "Метанол",

        // Коробка передач
        ["自动"] = "Автомат",
        ["自动挡"] = "Автомат",
        ["手动"] = "Механика",
        ["手动挡"] = "Механика",
        ["无级变速"] = "Вариатор",
        ["双离合"] = "Робот (DCT)",
        ["手自一体"] = "Автомат (Tiptronic)",
        ["CVT"] = "Вариатор",
        ["AT"] = "Автомат",
        ["MT"] = "Механика",
        ["DCT"] = "Робот (DCT)",

        // Кузов
        ["微型车"] = "Мини-класс",
        ["小型车"] = "Малый класс",
        ["紧凑型车"] = "Компактный класс",
        ["中型车"] = "Средний класс",
        ["中大型车"] = "Бизнес-класс",
        ["大型车"] = "Большой класс",
        ["两厢车"] = "Хэтчбек",
        ["三厢车"] = "Седан",
        ["掀背车"] = "Лифтбек",
        ["旅行车"] = "Универсал",
        ["跨界车"] = "Кроссовер",
        ["SUV"] = "Кроссовер",
        ["MPV"] = "Минивэн",
        ["跑车"] = "Спорткар",
        ["皮卡"] = "Пикап",
        ["面包车"] = "Минивэн",
        ["客车"] = "Автобус",
        ["货车"] = "Грузовик",

        // Экологический класс
        ["欧I"] = "Евро-1",
        ["欧II"] = "Евро-2",
        ["欧III"] = "Евро-3",
        ["欧IV"] = "Евро-4",
        ["欧V"] = "Евро-5",
        ["欧VI"] = "Евро-6",
        ["国I"] = "Евро-1",
        ["国II"] = "Евро-2",
        ["国III"] = "Евро-3",
        ["国IV"] = "Евро-4",
        ["国V"] = "Евро-5",
        ["国VI"] = "Евро-6",

        // Цвет
        ["黑色"] = "Чёрный",
        ["白色"] = "Белый",
        ["银色"] = "Серебристый",
        ["银灰"] = "Серебристо-серый",
        ["灰色"] = "Серый",
        ["红色"] = "Красный",
        ["蓝色"] = "Синий",
        ["棕色"] = "Коричневый",
        ["香槟"] = "Шампань",
        ["香槟色"] = "Шампань",
        ["金色"] = "Золотой",
        ["绿色"] = "Зелёный",
        ["黄色"] = "Жёлтый",
        ["橙色"] = "Оранжевый",
        ["紫色"] = "Фиолетовый",
        ["米色"] = "Бежевый",
        ["咖啡色"] = "Кофейный",
        ["酒红色"] = "Бордовый",
        ["珍珠白"] = "Жемчужно-белый",
        ["金属漆"] = "Металлик",
        ["其他"] = "Другой",

        // Города
        ["北京"] = "Пекин",
        ["上海"] = "Шанхай",
        ["广州"] = "Гуанчжоу",
        ["深圳"] = "Шэньчжэнь",
        ["天津"] = "Тяньцзинь",
        ["重庆"] = "Чунцин",
        ["杭州"] = "Ханчжоу",
        ["南京"] = "Нанкин",
        ["苏州"] = "Сучжоу",
        ["宁波"] = "Нинбо",
        ["温州"] = "Вэньчжоу",
        ["成都"] = "Чэнду",
        ["武汉"] = "Ухань",
        ["西安"] = "Сиань",
        ["长沙"] = "Чанша",
        ["郑州"] = "Чжэнчжоу",
        ["青岛"] = "Циндао",
        ["厦门"] = "Сямынь",
        ["福州"] = "Фучжоу",
        ["合肥"] = "Хэфэй",
        ["昆明"] = "Куньмин",
        ["济南"] = "Цзинань",
        ["沈阳"] = "Шэньян",
        ["大连"] = "Далянь",
        ["哈尔滨"] = "Харбин",
        ["长春"] = "Чанчунь",
        ["石家庄"] = "Шицзячжуан",
        ["太原"] = "Тайюань",
        ["南昌"] = "Наньчан",
        ["贵阳"] = "Гуйян",
        ["南宁"] = "Наньнин",
        ["海口"] = "Хайкоу",
        ["兰州"] = "Ланьчжоу",
        ["银川"] = "Иньчуань",
        ["西宁"] = "Синин",
        ["乌鲁木齐"] = "Урумчи",
        ["呼和浩特"] = "Хух-Хото",
        ["拉萨"] = "Лхаса",
        ["无锡"] = "Уси",
        ["常州"] = "Чанчжоу",
        ["南通"] = "Наньтун",
        ["徐州"] = "Сюйчжоу",
        ["扬州"] = "Янчжоу",
        ["绍兴"] = "Шаосин",
        ["嘉兴"] = "Цзясин",
        ["金华"] = "Цзиньхуа",
        ["台州"] = "Тайчжоу",
        ["泉州"] = "Цюаньчжоу",
        ["珠海"] = "Чжухай",
        ["中山"] = "Чжуншань",
        ["东莞"] = "Дунгуань",
        ["惠州"] = "Хуэйчжоу",
        ["汕头"] = "Шаньтоу",
        ["桂林"] = "Гуйлинь",
        ["洛阳"] = "Лоян",
        ["唐山"] = "Таншань",
        ["保定"] = "Баодин",
        ["廊坊"] = "Ланфан"
    };

    public static string Translate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? "";
        }

        var trimmed = value.Trim();

        if (Values.TryGetValue(trimmed, out var exact))
        {
            return exact;
        }

        if (trimmed.Contains('/'))
        {
            var parts = trimmed
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => Values.GetValueOrDefault(part, part));

            return string.Join("/", parts);
        }

        return trimmed;
    }

    public static string BuildTitle(
        string? brandName,
        string? seriesName,
        int? year,
        string? displacement,
        string? gearbox,
        string? fallbackTitle)
    {
        var parts = new List<string>();
        AddPart(parts, ResolveBrand(brandName));
        AddPart(parts, ResolveSeries(seriesName));

        if (year is not null)
        {
            parts.Add(year.Value.ToString());
        }

        AddPart(parts, displacement);
        AddPart(parts, Translate(gearbox));

        if (parts.Count == 0)
        {
            return string.IsNullOrWhiteSpace(fallbackTitle) ? "Автомобиль" : fallbackTitle.Trim();
        }

        return string.Join(" ", parts);
    }

    public static string TranslateBrand(string? value) => ResolveBrand(value) ?? "";

    public static string TranslateSeries(string? value) => ResolveSeries(value) ?? "";

    private static string? ResolveBrand(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!ContainsChinese(value))
        {
            return value;
        }

        return BrandLocalization.GetRussianName(value) ?? value;
    }

    private static string? ResolveSeries(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var localized = ModelLocalization.GetName(value);
        if (!string.IsNullOrWhiteSpace(localized))
        {
            return localized;
        }

        if (!ContainsChinese(value))
        {
            return value;
        }

        var latin = new string(value.Where(ch => ch is < '\u4e00' or > '\u9fff').ToArray()).Trim();
        return string.IsNullOrWhiteSpace(latin) ? null : latin;
    }

    private static void AddPart(List<string> parts, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || ContainsChinese(value))
        {
            return;
        }

        parts.Add(value.Trim());
    }

    private static bool ContainsChinese(string value)
    {
        return value.Any(ch => ch is >= '\u4e00' and <= '\u9fff');
    }
}



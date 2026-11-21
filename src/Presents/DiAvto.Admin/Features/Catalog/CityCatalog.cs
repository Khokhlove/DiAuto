namespace DiAvto.Admin.Features.Catalog;

public sealed record CityOption(string ExternalId, string NameRu, string NameCn);

public static class CityCatalog
{
    public static IReadOnlyList<CityOption> All { get; } =
    [
        new("110100", "Пекин", "北京"),
        new("310100", "Шанхай", "上海"),
        new("440100", "Гуанчжоу", "广州"),
        new("440300", "Шэньчжэнь", "深圳"),
        new("510100", "Чэнду", "成都"),
        new("330100", "Ханчжоу", "杭州"),
        new("500100", "Чунцин", "重庆"),
        new("420100", "Ухань", "武汉"),
        new("610100", "Сиань", "西安"),
        new("320500", "Сучжоу", "苏州"),
        new("120100", "Тяньцзинь", "天津"),
        new("370200", "Циндао", "青岛"),
        new("210100", "Шэньян", "沈阳"),
        new("430100", "Чанша", "长沙"),
        new("410100", "Чжэнчжоу", "郑州"),
        new("340100", "Хэфэй", "合肥"),
        new("350200", "Сямэнь", "厦门"),
        new("460100", "Хайкоу", "海口"),
        new("530100", "Куньмин", "昆明"),
        new("650100", "Урумчи", "乌鲁木齐")
    ];

    public static CityOption Default => All[0];
}


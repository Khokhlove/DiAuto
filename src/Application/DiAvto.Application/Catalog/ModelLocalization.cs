namespace DiAvto.Application.Catalog;

/// <summary>
/// Переводит названия моделей, которые che168 отдаёт на китайском.
/// Неизвестные названия возвращает как null, чтобы UI показал исходное имя и pinyin.
/// </summary>
public static class ModelLocalization
{
    private static readonly Dictionary<string, string> Names = new(StringComparer.Ordinal)
    {
        // Honda
        ["雅阁"] = "Accord",
        ["雅阁新能源"] = "Accord PHEV",
        ["雅阁(海外)"] = "Accord (за рубежом)",
        ["思域"] = "Civic",
        ["思域(进口)"] = "Civic (импорт)",
        ["飞度"] = "Fit",
        ["飞度(进口)"] = "Fit (импорт)",
        ["飞度新能源(进口)"] = "Fit EV (импорт)",
        ["锋范"] = "City",
        ["锋范(海外)"] = "City (за рубежом)",
        ["锋范经典"] = "City Classic",
        ["凌派"] = "Crider",
        ["缤智"] = "Vezel",
        ["缤智(海外)"] = "Vezel (за рубежом)",
        ["冠道"] = "Avancier",
        ["皓影"] = "Breeze",
        ["皓影新能源"] = "Breeze PHEV",
        ["型格"] = "Integra",
        ["奥德赛"] = "Odyssey",
        ["奥德赛(进口)"] = "Odyssey (импорт)",
        ["艾力绅"] = "Elysion",
        ["歌诗图"] = "Crosstour",
        ["歌诗图(海外)"] = "Crosstour (за рубежом)",
        ["杰德"] = "Jade",
        ["杰德(海外)"] = "Jade (за рубежом)",
        ["英仕派"] = "Inspire",
        ["英仕派新能源"] = "Inspire PHEV",
        ["享域"] = "Envix",
        ["竞瑞"] = "Gienia",
        ["哥瑞"] = "Greiz",
        ["思铂睿"] = "Spirior",
        ["思迪"] = "City",
        ["时韵"] = "Stream",
        ["里程"] = "Legend",
        ["元素"] = "Element",
        ["猎光e:NS2"] = "e:NS2",
        ["ZR-V 致在"] = "ZR-V",
        ["e:NP1 极湃1"] = "e:NP1",
        ["e:NP2 极湃2"] = "e:NP2",
        ["东风本田GT"] = "Dongfeng Honda GT",
        ["东风本田S7"] = "Dongfeng Honda S7",
        ["广汽本田GT"] = "GAC Honda GT",
        ["广汽本田P7"] = "GAC Honda P7",
        ["本田0 SUV"] = "Honda 0 SUV",
        ["本田0 Saloon"] = "Honda 0 Saloon",

        // Toyota
        ["卡罗拉"] = "Corolla",
        ["凯美瑞"] = "Camry",
        ["汉兰达"] = "Highlander",
        ["RAV4荣放"] = "RAV4",
        ["皇冠"] = "Crown",
        ["普拉多"] = "Prado",
        ["兰德酷路泽"] = "Land Cruiser",
        ["雷凌"] = "Levin",
        ["亚洲龙"] = "Avalon",
        ["威驰"] = "Vios",
        ["致炫"] = "Yaris L",
        ["赛那"] = "Sienna",
        ["埃尔法"] = "Alphard",
        ["威尔法"] = "Vellfire",
        ["锋兰达"] = "Frontlander",
        ["凌尚"] = "Levin GT",
        ["奕泽"] = "C-HR",
        ["卡罗拉锐放"] = "Corolla Cross",
        ["铂智4X"] = "bZ4X",

        // Volkswagen
        ["朗逸"] = "Lavida",
        ["速腾"] = "Sagitar",
        ["迈腾"] = "Magotan",
        ["帕萨特"] = "Passat",
        ["宝来"] = "Bora",
        ["高尔夫"] = "Golf",
        ["途观"] = "Tiguan",
        ["途观L"] = "Tiguan L",
        ["途昂"] = "Teramont",
        ["探岳"] = "Tayron",
        ["桑塔纳"] = "Santana",
        ["捷达"] = "Jetta",
        ["凌渡"] = "Lamando",
        ["途岳"] = "Tharu",
        ["揽境"] = "Talagon",
        ["揽巡"] = "Tavendor",

        // Nissan
        ["轩逸"] = "Sylphy",
        ["天籁"] = "Teana",
        ["奇骏"] = "X-Trail",
        ["逍客"] = "Qashqai",
        ["途达"] = "Terra",
        ["楼兰"] = "Murano",
        ["蓝鸟"] = "Bluebird",
        ["骐达"] = "Tiida",

        // BMW / Mercedes / Audi
        ["3系"] = "3 Series",
        ["5系"] = "5 Series",
        ["7系"] = "7 Series",
        ["C级"] = "C-Class",
        ["E级"] = "E-Class",
        ["S级"] = "S-Class",
        ["A4L"] = "A4L",
        ["A6L"] = "A6L",
        ["Q5L"] = "Q5L",

        // Hyundai / Kia
        ["伊兰特"] = "Elantra",
        ["索纳塔"] = "Sonata",
        ["途胜"] = "Tucson",
        ["智跑"] = "Sportage",
        ["狮跑"] = "Sportage",

        // BYD
        ["秦"] = "Qin",
        ["汉"] = "Han",
        ["唐"] = "Tang",
        ["宋"] = "Song",
        ["元"] = "Yuan",
        ["海豹"] = "Seal",
        ["海豚"] = "Dolphin"
    };

    public static string? GetName(string? sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            return null;
        }

        var name = sourceName.Trim();
        var suffixes = new List<string>();

        for (var i = 0; i < 3; i++)
        {
            if (name.EndsWith("(进口)", StringComparison.Ordinal))
            {
                suffixes.Insert(0, " (импорт)");
                name = name[..^4].Trim();
            }
            else if (name.EndsWith("(海外)", StringComparison.Ordinal))
            {
                suffixes.Insert(0, " (за рубежом)");
                name = name[..^4].Trim();
            }
            else if (name.EndsWith("新能源", StringComparison.Ordinal))
            {
                suffixes.Insert(0, " PHEV");
                name = name[..^3].Trim();
            }
            else
            {
                break;
            }
        }

        string? translated = null;

        if (Names.TryGetValue(name, out var exact))
        {
            translated = exact;
        }
        else if (name.StartsWith("本田", StringComparison.Ordinal))
        {
            var rest = name[2..].Trim();
            if (!string.IsNullOrWhiteSpace(rest) && !ContainsChinese(rest))
            {
                translated = rest;
            }
        }

        return translated is null ? null : translated + string.Concat(suffixes);
    }

    private static bool ContainsChinese(string value)
    {
        return value.Any(ch => ch is >= '\u4e00' and <= '\u9fff');
    }
}



namespace DiAvto.Application.Catalog;

public static class BrandLocalization
{
    private static readonly Dictionary<string, string> RussianNames = new(StringComparer.Ordinal)
    {
        ["宝马"] = "BMW",
        ["奥迪"] = "Audi",
        ["奔驰"] = "Mercedes-Benz",
        ["梅赛德斯-奔驰"] = "Mercedes-Benz",
        ["丰田"] = "Toyota",
        ["本田"] = "Honda",
        ["大众"] = "Volkswagen",
        ["特斯拉"] = "Tesla",
        ["比亚迪"] = "BYD",
        ["蔚来"] = "NIO",
        ["理想"] = "Li Auto",
        ["小鹏"] = "XPeng",
        ["吉利"] = "Geely",
        ["哈弗"] = "Haval",
        ["长城"] = "Great Wall",
        ["奇瑞"] = "Chery",
        ["长安"] = "Changan",
        ["五菱"] = "Wuling",
        ["宝骏"] = "Baojun",
        ["红旗"] = "Hongqi",
        ["领克"] = "Lynk & Co",
        ["极氪"] = "Zeekr",
        ["问界"] = "Aito",
        ["小米"] = "Xiaomi",
        ["零跑"] = "Leapmotor",
        ["哪吒"] = "Neta",
        ["广汽传祺"] = "GAC Trumpchi",
        ["荣威"] = "Roewe",
        ["名爵"] = "MG",
        ["保时捷"] = "Porsche",
        ["路虎"] = "Land Rover",
        ["捷豹"] = "Jaguar",
        ["沃尔沃"] = "Volvo",
        ["雷克萨斯"] = "Lexus",
        ["日产"] = "Nissan",
        ["马自达"] = "Mazda",
        ["三菱"] = "Mitsubishi",
        ["现代"] = "Hyundai",
        ["起亚"] = "Kia",
        ["别克"] = "Buick",
        ["雪佛兰"] = "Chevrolet",
        ["凯迪拉克"] = "Cadillac",
        ["福特"] = "Ford",
        ["林肯"] = "Lincoln",
        ["法拉利"] = "Ferrari",
        ["兰博基尼"] = "Lamborghini",
        ["玛莎拉蒂"] = "Maserati",
        ["宾利"] = "Bentley",
        ["劳斯莱斯"] = "Rolls-Royce",
        ["迈凯伦"] = "McLaren",
        ["阿斯顿·马丁"] = "Aston Martin",
        ["阿尔法·罗密欧"] = "Alfa Romeo",
        ["标致"] = "Peugeot",
        ["雪铁龙"] = "Citroen",
        ["雷诺"] = "Renault",
        ["斯柯达"] = "Skoda",
        ["迷你"] = "MINI",
        ["捷尼赛思"] = "Genesis",
        ["英菲尼迪"] = "Infiniti",
        ["讴歌"] = "Acura",
        ["铃木"] = "Suzuki",
        ["斯巴鲁"] = "Subaru",
        ["五十铃"] = "Isuzu",
        ["依维柯"] = "Iveco",
        ["东风"] = "Dongfeng",
        ["江淮"] = "JAC",
        ["福田"] = "Foton",
        ["金杯"] = "Jinbei"
    };

    public static string? GetRussianName(string? chineseName)
    {
        return chineseName is not null && RussianNames.TryGetValue(chineseName, out var name) ? name : null;
    }

    public static string BuildDisplayName(string chineseName, string? pinyin, string? russianName)
    {
        if (!string.IsNullOrWhiteSpace(russianName))
        {
            return russianName;
        }

        if (!string.IsNullOrWhiteSpace(pinyin))
        {
            return $"{pinyin} · {chineseName}";
        }

        return chineseName;
    }
}



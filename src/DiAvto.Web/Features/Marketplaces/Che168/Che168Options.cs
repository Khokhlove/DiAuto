namespace DiAvto.Web.Features.Marketplaces.Che168;

public sealed class Che168Options
{
    public const string SectionName = "Che168";

    public string ApiBaseUrl { get; set; } = "https://api2scsou.che168.com";
    public string DetailApiBaseUrl { get; set; } = "https://apiuscdt.che168.com";
    public string AppId { get; set; } = "2sc.m";
    public string AppVersion { get; set; } = "11.41.5";
    public string SubAppId { get; set; } = "2sc.pc";
    public string DeviceId { get; set; } = "diavto-web-001";
    public string DetailUrlTemplate { get; set; } = "https://m.che168.com/cardetail/index?infoid={infoid}";
    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
    public string Referer { get; set; } = "https://m.che168.com/";
    public int TimeoutSeconds { get; set; } = 35;
    public int PriceSortAscending { get; set; } = 2;
    public int PriceSortDescending { get; set; } = 1;
    public int MaxRetries { get; set; } = 3;
}

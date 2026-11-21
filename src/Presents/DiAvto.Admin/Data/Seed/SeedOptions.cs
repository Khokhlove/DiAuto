namespace DiAvto.Admin.Data.Seed;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public bool Enabled { get; set; } = true;
    public string AdminUserName { get; set; } = "admin";
    public string AdminEmail { get; set; } = "admin";
    public string AdminPassword { get; set; } = "admin";
    public string AdminDisplayName { get; set; } = "Администратор";
    public string Che168DetailUrlTemplate { get; set; } = "https://m.che168.com/cardetail/index?infoid={infoid}";
}


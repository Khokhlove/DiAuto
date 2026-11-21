namespace DiAvto.Domain;

/// <summary>Публичные настройки сайта: название, телефоны, ссылки на мессенджеры.</summary>
public class SiteSetting
{
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public string? Description { get; set; }
}

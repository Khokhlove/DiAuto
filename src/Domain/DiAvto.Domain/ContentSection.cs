namespace DiAvto.Domain;

/// <summary>Настраиваемая секция публичной страницы.</summary>
public class ContentSection
{
    public int Id { get; set; }

    public string Page { get; set; } = "index";

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; } = true;
}

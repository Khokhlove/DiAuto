namespace DiAvto.Domain;

/// <summary>Карточка внутри настраиваемой секции публичной страницы.</summary>
public class ContentSectionItem
{
    public int Id { get; set; }

    public string Section { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? IconName { get; set; }

    public string? Url { get; set; }

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; } = true;
}

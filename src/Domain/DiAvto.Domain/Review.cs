namespace DiAvto.Domain;

public class Review
{
    public int Id { get; set; }

    /// <summary>text, screenshot или video.</summary>
    public string Type { get; set; } = "text";

    public string? AuthorName { get; set; }

    public string? City { get; set; }

    public string? Text { get; set; }

    public int? Rating { get; set; }

    public string? Source { get; set; }

    public string? ScreenshotUrl { get; set; }

    public string? VideoUrl { get; set; }

    public string? VideoTitle { get; set; }

    public bool IsVisible { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

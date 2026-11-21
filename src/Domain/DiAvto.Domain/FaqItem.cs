namespace DiAvto.Domain;

public class FaqItem
{
    public int Id { get; set; }

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; } = true;
}

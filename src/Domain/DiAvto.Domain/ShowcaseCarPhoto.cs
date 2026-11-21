namespace DiAvto.Domain;

public class ShowcaseCarPhoto
{
    public int Id { get; set; }

    public int ShowcaseCarId { get; set; }
    public ShowcaseCar ShowcaseCar { get; set; } = null!;

    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

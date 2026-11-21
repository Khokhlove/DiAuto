namespace DiAvto.Domain;

/// <summary>Заявка с публичного сайта.</summary>
public class Lead
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string Source { get; set; } = "site";

    public bool IsProcessed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public string? ManagerComment { get; set; }
}

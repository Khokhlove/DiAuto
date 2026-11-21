namespace DiAvto.Admin.Features.Search;

public sealed class SearchOptions
{
    public const string SectionName = "Search";

    public int PageSize { get; set; } = 20;
    public int MaxPagesPerTier { get; set; } = 4;
    public int DetailsConcurrency { get; set; } = 3;
    public int ResultsPerTier { get; set; } = 3;
    public int HistoryRetentionDays { get; set; } = 365;
}


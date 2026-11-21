namespace DiAvto.Application.Public;

public sealed record ContentSectionDto(string Slug, string Title, string? Subtitle, int SortOrder);

public sealed record ContentSectionItemDto(
    string Section,
    string Title,
    string? Description,
    string? ImageUrl,
    string? IconName,
    string? Url,
    int SortOrder);

public interface IContentSectionService
{
    Task<IReadOnlyList<ContentSectionDto>> GetSectionsAsync(
        string page,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentSectionItemDto>> GetItemsAsync(
        string section,
        CancellationToken cancellationToken = default);
}

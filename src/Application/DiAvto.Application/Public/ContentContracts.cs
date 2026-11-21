namespace DiAvto.Application.Public;

public sealed record ReviewDto(
    int Id,
    string Type,
    string? AuthorName,
    string? City,
    string? Text,
    int? Rating,
    string? Source,
    string? ScreenshotUrl,
    string? VideoUrl,
    string? VideoTitle);

public sealed record OfficeDto(
    int Id,
    string City,
    string Address,
    string? Phone,
    string? Phone2,
    decimal? Latitude,
    decimal? Longitude);

public sealed record FaqItemDto(int Id, string Question, string Answer);

public sealed record SiteSettingsDto(
    string CompanyName,
    string CompanyTagline,
    string Phone,
    string Phone2,
    string Email,
    string WhatsAppUrl,
    string TelegramUrl,
    string VkUrl,
    string MaxUrl,
    string PrivacyPolicyUrl = "/privacy-policy")
{
    public string PhoneHref => BuildPhoneHref(Phone);
    public string Phone2Href => BuildPhoneHref(Phone2);
    public string EmailHref => $"mailto:{Email}";

    private static string BuildPhoneHref(string? phone)
    {
        var digits = new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? "#" : $"tel:+{digits}";
    }
}

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetVisibleAsync(CancellationToken cancellationToken = default);
}

public interface IOfficeService
{
    Task<IReadOnlyList<OfficeDto>> GetActiveAsync(CancellationToken cancellationToken = default);
}

public interface IFaqService
{
    Task<IReadOnlyList<FaqItemDto>> GetVisibleAsync(CancellationToken cancellationToken = default);
}

public interface ISiteSettingsService
{
    Task<SiteSettingsDto> GetAsync(CancellationToken cancellationToken = default);
}

public interface ILeadService
{
    Task SubmitAsync(LeadRequest request, CancellationToken cancellationToken = default);
}

public sealed record LeadRequest(string Name, string Phone, string? Message, string Source);

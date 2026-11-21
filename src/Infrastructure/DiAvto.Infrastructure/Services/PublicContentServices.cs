using DiAvto.Application.Content;
using DiAvto.Application.Public;
using DiAvto.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Infrastructure.Services;

public sealed class ReviewService : IReviewService
{
    private readonly DiAvtoDbContext _db;

    public ReviewService(DiAvtoDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ReviewDto>> GetVisibleAsync(CancellationToken cancellationToken = default)
    {
        var reviews = await _db.Reviews
            .AsNoTracking()
            .Where(x => x.IsVisible)
            .OrderBy(x => x.SortOrder)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(x => new ReviewDto(
            x.Id, x.Type, x.AuthorName, x.City, x.Text, x.Rating, x.Source,
            x.ScreenshotUrl, x.VideoUrl, x.VideoTitle)).ToList();
    }
}

public sealed class OfficeService : IOfficeService
{
    private readonly DiAvtoDbContext _db;

    public OfficeService(DiAvtoDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OfficeDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var offices = await _db.Offices
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return offices.Select(x => new OfficeDto(
            x.Id, x.City, x.Address, x.Phone, x.Phone2, x.Latitude, x.Longitude)).ToList();
    }
}

public sealed class FaqService : IFaqService
{
    private readonly DiAvtoDbContext _db;

    public FaqService(DiAvtoDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<FaqItemDto>> GetVisibleAsync(CancellationToken cancellationToken = default)
    {
        return await _db.FaqItems
            .AsNoTracking()
            .Where(x => x.IsVisible)
            .OrderBy(x => x.SortOrder)
            .Select(x => new FaqItemDto(x.Id, x.Question, x.Answer))
            .ToListAsync(cancellationToken);
    }
}

public sealed class SiteSettingsService : ISiteSettingsService
{
    private readonly DiAvtoDbContext _db;

    public SiteSettingsService(DiAvtoDbContext db)
    {
        _db = db;
    }

    public async Task<SiteSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var values = await _db.SiteSettings
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase, cancellationToken);

        string Get(string key, string fallback) =>
            values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;

        return new SiteSettingsDto(
            Get("company_name", SiteContent.CompanyName),
            Get("company_tagline", SiteContent.CompanyTagline),
            Get("phone", SiteContent.Phone),
            Get("phone2", SiteContent.Phone2),
            Get("email", SiteContent.Email),
            Get("social_whatsapp", SiteContent.WhatsAppUrl),
            Get("social_telegram", SiteContent.TelegramUrl),
            Get("social_vk", SiteContent.VkUrl),
            Get("social_max", SiteContent.MaxUrl));
    }
}

public sealed class LeadService : ILeadService
{
    private readonly DiAvtoDbContext _db;

    public LeadService(DiAvtoDbContext db)
    {
        _db = db;
    }

    public async Task SubmitAsync(LeadRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new ArgumentException("Укажите имя и телефон.");
        }

        _db.Leads.Add(new Domain.Lead
        {
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
            Source = string.IsNullOrWhiteSpace(request.Source) ? "site" : request.Source.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ContentSectionService : IContentSectionService
{
    private readonly DiAvtoDbContext _db;

    public ContentSectionService(DiAvtoDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ContentSectionDto>> GetSectionsAsync(
        string page,
        CancellationToken cancellationToken = default)
    {
        return await _db.ContentSections
            .AsNoTracking()
            .Where(x => x.Page == page && x.IsVisible)
            .OrderBy(x => x.SortOrder)
            .Select(x => new ContentSectionDto(x.Slug, x.Title, x.Subtitle, x.SortOrder))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentSectionItemDto>> GetItemsAsync(
        string section,
        CancellationToken cancellationToken = default)
    {
        return await _db.ContentSectionItems
            .AsNoTracking()
            .Where(x => x.Section == section && x.IsVisible)
            .OrderBy(x => x.SortOrder)
            .Select(x => new ContentSectionItemDto(
                x.Section, x.Title, x.Description, x.ImageUrl, x.IconName, x.Url, x.SortOrder))
            .ToListAsync(cancellationToken);
    }
}

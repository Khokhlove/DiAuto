using DiAvto.Admin.Data;
using DiAvto.Domain;
using DiAvto.Admin.Features.Marketplaces;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Admin.Features.Listings;

public sealed class ListingService
{
    private readonly DiAvtoDbContext _db;
    private readonly IMarketplaceClientFactory _clients;
    private readonly ILogger<ListingService> _logger;

    public ListingService(
        DiAvtoDbContext db,
        IMarketplaceClientFactory clients,
        ILogger<ListingService> logger)
    {
        _db = db;
        _clients = clients;
        _logger = logger;
    }

    public async Task<CarListing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.CarListings
            .AsNoTracking()
            .Include(x => x.SourceSite)
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(List<CarListing> Items, int Total)> GetPagedAsync(
        string? search,
        int? sourceSiteId,
        int page = 1,
        int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var query = _db.CarListings
            .AsNoTracking()
            .Include(x => x.SourceSite)
            .Include(x => x.Photos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term) ||
                                     (x.BrandName != null && x.BrandName.Contains(term)) ||
                                     (x.SeriesName != null && x.SeriesName.Contains(term)) ||
                                     x.ExternalId == term);
        }

        if (sourceSiteId is not null)
        {
            query = query.Where(x => x.SourceSiteId == sourceSiteId);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.LastFetchedAt)
            .Skip((Math.Max(1, page) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _db.CarListings.CountAsync(cancellationToken);
    }

    public async Task<List<CarListing>> GetRecentAsync(int take = 6, CancellationToken cancellationToken = default)
    {
        return await _db.CarListings
            .AsNoTracking()
            .Include(x => x.SourceSite)
            .Include(x => x.Photos)
            .OrderByDescending(x => x.LastFetchedAt)
            .Take(Math.Clamp(take, 1, 50))
            .ToListAsync(cancellationToken);
    }

    public async Task<CarListing> RefreshAsync(int id, CancellationToken cancellationToken = default)
    {
        var listing = await _db.CarListings
            .Include(x => x.SourceSite)
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Автомобиль не найден.");

        var client = _clients.Get(listing.SourceSite.Key);
        var details = await client.GetCarDetailsAsync(listing.ExternalId, cancellationToken);

        ListingMapper.ApplyDetails(listing, details, DateTime.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Автомобиль {ListingId} обновлён из источника.", id);

        return listing;
    }
}


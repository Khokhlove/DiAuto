using DiAvto.Web.Data;
using DiAvto.Web.Domain;
using DiAvto.Web.Features.Marketplaces;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Web.Features.Catalog;

public sealed class CatalogService
{
    private static readonly SemaphoreSlim BrandsSyncLock = new(1, 1);
    private static readonly SemaphoreSlim SeriesSyncLock = new(1, 1);

    private readonly ApplicationDbContext _db;
    private readonly IMarketplaceClientFactory _clients;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(
        ApplicationDbContext db,
        IMarketplaceClientFactory clients,
        ILogger<CatalogService> logger)
    {
        _db = db;
        _clients = clients;
        _logger = logger;
    }

    public async Task<List<Brand>> GetBrandsAsync(
        bool includeInactive = false,
        int? sourceSiteId = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureCatalogAsync(cancellationToken);

        var query = _db.Brands
            .AsNoTracking()
            .Include(x => x.SourceSite)
            .AsQueryable();

        if (sourceSiteId is not null)
        {
            query = query.Where(x => x.SourceSiteId == sourceSiteId);
        }

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive && x.SourceSite.IsEnabled);
        }

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Brand>> GetAdminBrandsAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Brands
            .AsNoTracking()
            .Include(x => x.SourceSite)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) ||
                                     (x.NameLatin != null && x.NameLatin.Contains(term)) ||
                                     (x.NameRu != null && x.NameRu.Contains(term)));
        }

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Take(500)
            .ToListAsync(cancellationToken);
    }

    public async Task<Brand?> GetBrandAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Brands
            .AsNoTracking()
            .Include(x => x.SourceSite)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<CarSeries>> GetSeriesAsync(int brandId, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var brand = await _db.Brands
            .Include(x => x.SourceSite)
            .FirstOrDefaultAsync(x => x.Id == brandId, cancellationToken);

        if (brand is null)
        {
            return [];
        }

        var hasSeries = await _db.CarSeries.AnyAsync(x => x.BrandId == brandId, cancellationToken);
        if (!hasSeries)
        {
            try
            {
                await SyncSeriesAsync(brandId, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Не удалось загрузить модели бренда {BrandId}", brandId);
            }
        }

        var query = _db.CarSeries
            .AsNoTracking()
            .Where(x => x.BrandId == brandId);

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.NameRu ?? x.Name)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<CarSeries?> GetSeriesByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.CarSeries
            .AsNoTracking()
            .Include(x => x.Brand)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task EnsureCatalogAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.Brands.AnyAsync(cancellationToken))
        {
            return;
        }

        await BrandsSyncLock.WaitAsync(cancellationToken);
        try
        {
            if (await _db.Brands.AnyAsync(cancellationToken))
            {
                return;
            }

            var source = await _db.SourceSites
                .FirstOrDefaultAsync(x => x.Key == "che168", cancellationToken);

            if (source is null)
            {
                throw new InvalidOperationException("Источник che168 не настроен.");
            }

            await SyncBrandsCoreAsync(source, cancellationToken);
        }
        finally
        {
            BrandsSyncLock.Release();
        }
    }

    public async Task<int> SyncBrandsAsync(int sourceSiteId, CancellationToken cancellationToken = default)
    {
        await BrandsSyncLock.WaitAsync(cancellationToken);
        try
        {
            var source = await _db.SourceSites.FirstOrDefaultAsync(x => x.Id == sourceSiteId, cancellationToken)
                         ?? throw new InvalidOperationException("Источник не найден.");

            return await SyncBrandsCoreAsync(source, cancellationToken);
        }
        finally
        {
            BrandsSyncLock.Release();
        }
    }

    public async Task<int> SyncSeriesAsync(int brandId, CancellationToken cancellationToken = default)
    {
        await SeriesSyncLock.WaitAsync(cancellationToken);
        try
        {
            var brand = await _db.Brands
                .Include(x => x.SourceSite)
                .FirstOrDefaultAsync(x => x.Id == brandId, cancellationToken)
                ?? throw new InvalidOperationException("Бренд не найден.");

            var client = _clients.Get(brand.SourceSite.Key);
            var remote = await client.GetSeriesAsync(brand.ExternalId, cancellationToken);
            if (remote.Count == 0)
            {
                return 0;
            }

            var existing = await _db.CarSeries
                .Where(x => x.BrandId == brandId)
                .ToListAsync(cancellationToken);

            var byExternalId = existing.ToDictionary(x => x.ExternalId, StringComparer.Ordinal);
            var remoteIds = remote.Select(x => x.ExternalId).ToHashSet(StringComparer.Ordinal);
            var now = DateTime.UtcNow;

            foreach (var item in remote)
            {
                if (!byExternalId.TryGetValue(item.ExternalId, out var series))
                {
                    series = new CarSeries
                    {
                        BrandId = brandId,
                        ExternalId = item.ExternalId
                    };
                    _db.CarSeries.Add(series);
                    byExternalId[item.ExternalId] = series;
                }

                series.Name = item.Name;
                series.NameRu ??= ModelLocalization.GetName(item.Name);
                series.Pinyin = item.Pinyin;
                series.IconUrl = item.IconUrl;
                series.PriceRange = item.PriceRange;
                series.IsActive = true;
                series.LastSyncedAt = now;
            }

            foreach (var series in existing.Where(x => !remoteIds.Contains(x.ExternalId)))
            {
                series.IsActive = false;
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Синхронизировано моделей {Brand}: {Count}", brand.Name, remote.Count);
            return remote.Count;
        }
        finally
        {
            SeriesSyncLock.Release();
        }
    }

    public async Task<MarketplaceHealth> CheckSourceAsync(int sourceSiteId, CancellationToken cancellationToken = default)
    {
        var source = await _db.SourceSites.FirstOrDefaultAsync(x => x.Id == sourceSiteId, cancellationToken)
                     ?? throw new InvalidOperationException("Источник не найден.");

        MarketplaceHealth health;
        try
        {
            var client = _clients.Get(source.Key);
            health = await client.CheckHealthAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            health = new MarketplaceHealth(false, ex.Message);
        }

        source.LastHealthCheckAt = DateTime.UtcNow;
        source.LastHealthCheckOk = health.Ok;
        source.LastHealthCheckMessage = health.Message;
        await _db.SaveChangesAsync(cancellationToken);

        return health;
    }

    private async Task<int> SyncBrandsCoreAsync(SourceSite source, CancellationToken cancellationToken)
    {
        var client = _clients.Get(source.Key);
        var remote = await client.GetBrandsAsync(cancellationToken);
        if (remote.Count == 0)
        {
            return 0;
        }

        var existing = await _db.Brands
            .Where(x => x.SourceSiteId == source.Id)
            .ToListAsync(cancellationToken);

        var byExternalId = existing.ToDictionary(x => x.ExternalId, StringComparer.Ordinal);
        var now = DateTime.UtcNow;

        foreach (var item in remote)
        {
            if (!byExternalId.TryGetValue(item.ExternalId, out var brand))
            {
                brand = new Brand
                {
                    SourceSiteId = source.Id,
                    ExternalId = item.ExternalId
                };
                _db.Brands.Add(brand);
                byExternalId[item.ExternalId] = brand;
            }

            brand.Name = item.Name;
            brand.NameLatin = item.Pinyin;
            brand.NameRu = BrandLocalization.GetRussianName(item.Name);
            brand.IconUrl = item.IconUrl;
            brand.SortOrder = item.SortOrder;
            brand.IsActive = true;
            brand.LastSyncedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Синхронизировано брендов che168: {Count}", remote.Count);
        return remote.Count;
    }
}

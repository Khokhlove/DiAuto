using DiAvto.Web.Data;
using DiAvto.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Web.Features.Pricing;

/// <summary>Чтение тарифов, из которых складываются расходы поиска и карточки клиента.</summary>
public sealed class ServiceTariffService
{
    private readonly ApplicationDbContext _db;

    public ServiceTariffService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<ServiceTariff>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _db.ServiceTariffs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Kind)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ServiceTariff>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.ServiceTariffs
            .AsNoTracking()
            .OrderBy(x => x.Kind)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ServiceTariff>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await _db.ServiceTariffs
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.IsActive)
            .ToListAsync(cancellationToken);
    }
}

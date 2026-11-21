using DiAvto.Application.Public;
using DiAvto.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Infrastructure.Services;

public sealed class ShowcaseCarService : IShowcaseCarService
{
    private readonly DiAvtoDbContext _db;

    public ShowcaseCarService(DiAvtoDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ShowcaseCarDto>> GetPublishedAsync(
        ShowcaseCarFilter filter,
        CancellationToken cancellationToken = default)
    {
        // Витрина небольшая (сотни автомобилей), поэтому фильтры и сортировка
        // применяются в памяти: SQLite хранит decimal как TEXT, и ORDER BY
        // по цене даёт лексикографический, а не числовой порядок.
        var cars = await _db.ShowcaseCars
            .AsNoTracking()
            .Include(x => x.Photos)
            .Where(x => x.IsPublished)
            .ToListAsync(cancellationToken);

        IEnumerable<Domain.ShowcaseCar> query = cars;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x =>
                x.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Make.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Description.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.Country))
        {
            query = query.Where(x => x.Country == filter.Country);
        }

        if (!string.IsNullOrWhiteSpace(filter.Make))
        {
            query = query.Where(x => x.Make == filter.Make);
        }

        if (!string.IsNullOrWhiteSpace(filter.BodyType))
        {
            query = query.Where(x => x.BodyType == filter.BodyType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Transmission))
        {
            query = query.Where(x => x.Transmission == filter.Transmission);
        }

        if (!string.IsNullOrWhiteSpace(filter.Fuel))
        {
            query = query.Where(x => x.Fuel == filter.Fuel);
        }

        if (filter.PriceFrom is not null)
        {
            query = query.Where(x => x.PriceRub >= filter.PriceFrom);
        }

        if (filter.PriceTo is not null)
        {
            query = query.Where(x => x.PriceRub <= filter.PriceTo);
        }

        if (filter.YearFrom is not null)
        {
            query = query.Where(x => x.Year >= filter.YearFrom);
        }

        if (filter.YearTo is not null)
        {
            query = query.Where(x => x.Year <= filter.YearTo);
        }

        query = filter.Sort switch
        {
            "price-asc" => query.OrderBy(x => x.PriceRub).ThenBy(x => x.SortOrder),
            "price-desc" => query.OrderByDescending(x => x.PriceRub).ThenBy(x => x.SortOrder),
            "year-desc" => query.OrderByDescending(x => x.Year).ThenBy(x => x.SortOrder),
            "year-asc" => query.OrderBy(x => x.Year).ThenBy(x => x.SortOrder),
            _ => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
        };

        var take = Math.Clamp(filter.Take <= 0 ? 24 : filter.Take, 1, 500);
        var skip = Math.Max(0, filter.Skip);

        return query
            .Skip(skip)
            .Take(take)
            .Select(Map)
            .ToList();
    }

    public async Task<ShowcaseCarDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var car = await _db.ShowcaseCars
            .AsNoTracking()
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsPublished, cancellationToken);

        return car is null ? null : Map(car);
    }

    public async Task<IReadOnlyList<ShowcaseCarDto>> GetFeaturedAsync(
        int take = 6,
        CancellationToken cancellationToken = default)
    {
        var cars = await _db.ShowcaseCars
            .AsNoTracking()
            .Include(x => x.Photos)
            .Where(x => x.IsPublished && x.InStock)
            .OrderByDescending(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Take(Math.Clamp(take, 1, 24))
            .ToListAsync(cancellationToken);

        return cars.Select(Map).ToList();
    }

    public async Task<CatalogFacets> GetFacetsAsync(CancellationToken cancellationToken = default)
    {
        var cars = await _db.ShowcaseCars
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .Select(x => new { x.Country, x.Make, x.BodyType, x.Transmission, x.Fuel, x.PriceRub, x.Year })
            .ToListAsync(cancellationToken);

        return new CatalogFacets(
            cars.Select(x => x.Country).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList(),
            cars.Select(x => x.Make).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList(),
            cars.Select(x => x.BodyType).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList(),
            cars.Select(x => x.Transmission).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList(),
            cars.Select(x => x.Fuel).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList(),
            cars.Select(x => x.PriceRub).DefaultIfEmpty(0m).Min(),
            cars.Select(x => x.PriceRub).DefaultIfEmpty(0m).Max(),
            cars.Select(x => x.Year).DefaultIfEmpty(0).Min(),
            cars.Select(x => x.Year).DefaultIfEmpty(0).Max());
    }

    private static ShowcaseCarDto Map(Domain.ShowcaseCar car)
    {
        var images = car.Photos
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Url)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (images.Count == 0)
        {
            images.Add("/images/car-placeholder.svg");
        }

        return new ShowcaseCarDto(
            car.Id,
            car.Name,
            car.Make,
            car.Year,
            car.PriceRub,
            car.Country,
            car.Fuel,
            car.BodyType,
            car.Transmission,
            car.Drive,
            car.Engine,
            car.MileageKm,
            car.Description,
            car.InStock,
            images);
    }
}

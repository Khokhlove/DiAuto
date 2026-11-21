using DiAvto.Application.Formatting;
using DiAvto.Application.Public;
using DiAvto.Domain;
using DiAvto.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Infrastructure.Services;

public sealed class ClientCardPublicService : IClientCardPublicService
{
    private readonly DiAvtoDbContext _db;

    public ClientCardPublicService(DiAvtoDbContext db)
    {
        _db = db;
    }

    public async Task<ClientCardPublicDto?> GetAsync(
        string publicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return null;
        }

        var id = publicId.Trim();

        var card = await _db.ClientCards
            .AsNoTracking()
            .Include(x => x.CarListing)
                .ThenInclude(x => x.Photos)
            .FirstOrDefaultAsync(x => x.PublicId == id, cancellationToken);

        if (card is null)
        {
            return null;
        }

        var siteName = await LoadSiteNameAsync(cancellationToken);
        var contactText = await LoadContactTextAsync(siteName, cancellationToken);
        var listing = card.CarListing;

        var photos = listing.Photos
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Url)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (photos.Count == 0)
        {
            var fallback = string.IsNullOrWhiteSpace(listing.ImageUrl)
                ? "/images/car-placeholder.svg"
                : listing.ImageUrl;
            photos.Add(fallback);
        }

        var cityName = CarSpecTranslator.Translate(listing.CityName);
        var gearbox = CarSpecTranslator.Translate(listing.Gearbox);
        var fuelType = CarSpecTranslator.Translate(listing.FuelType);
        var bodyType = CarSpecTranslator.Translate(listing.BodyType);
        var color = CarSpecTranslator.Translate(listing.Color);
        var emission = CarSpecTranslator.Translate(listing.EmissionStandard);
        var displacement = Display(listing.Displacement);
        var mileage = FormatMileage(listing.MileageKm);

        var title = CarSpecTranslator.BuildTitle(
            listing.BrandName,
            listing.SeriesName,
            listing.Year,
            listing.Displacement,
            listing.Gearbox,
            listing.Title);

        var hasCityDelivery =
            !string.IsNullOrWhiteSpace(card.DeliveryFromVladivostokTitle) &&
            !string.Equals(card.DestinationCity, "Без доставки", StringComparison.OrdinalIgnoreCase);

        var specs = new List<ClientCardSpecDto>
        {
            new("Марка", Display(CarSpecTranslator.TranslateBrand(listing.BrandName))),
            new("Модель", Display(CarSpecTranslator.TranslateSeries(listing.SeriesName))),
            new("Год выпуска", listing.Year?.ToString() ?? "—"),
            new("Первая регистрация", Display(listing.FirstRegistration)),
            new("Пробег", mileage),
            new("Двигатель", displacement),
            new("Коробка передач", Display(gearbox)),
            new("Топливо", Display(fuelType)),
            new("Кузов", Display(bodyType)),
            new("Цвет", Display(color)),
            new("Экологический класс", Display(emission)),
            new("Владельцев по ПТС", listing.TransferCount?.ToString() ?? "—"),
            new("Страна нахождения", Display(card.CountryName)),
            new("Город", string.IsNullOrWhiteSpace(cityName) ? "—" : cityName)
        };

        return new ClientCardPublicDto(
            card.PublicId,
            card.IsActive,
            card.CreatedAt,
            card.CustomerName,
            card.CountryName,
            cityName,
            card.DestinationCity,
            title,
            listing.Year,
            mileage,
            displacement,
            gearbox,
            fuelType,
            bodyType,
            color,
            emission,
            Display(listing.FirstRegistration),
            listing.TransferCount,
            listing.IsNewEnergy,
            card.Note,
            card.TotalRub,
            card.ExchangeRate,
            card.MarkupPercent,
            decimal.Round(
                card.ExchangeRate * (1m + card.MarkupPercent / 100m),
                4,
                MidpointRounding.AwayFromZero),
            hasCityDelivery,
            photos,
            specs,
            contactText,
            siteName);
    }

    private async Task<string> LoadSiteNameAsync(CancellationToken cancellationToken)
    {
        var publicName = await _db.SiteSettings
            .AsNoTracking()
            .Where(x => x.Key == "company_name")
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(publicName))
        {
            return publicName;
        }

        var legacyName = await _db.AppSettings
            .AsNoTracking()
            .Where(x => x.Key == "SiteName")
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(legacyName) ? "ДИ.авто" : legacyName;
    }

    private async Task<string> LoadContactTextAsync(string siteName, CancellationToken cancellationToken)
    {
        var custom = await _db.AppSettings
            .AsNoTracking()
            .Where(x => x.Key == "ClientCardContactText")
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(custom))
        {
            return custom.Trim();
        }

        var phone = await _db.SiteSettings
            .AsNoTracking()
            .Where(x => x.Key == "phone")
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(phone)
            ? $"Свяжитесь с менеджером {siteName}, чтобы уточнить детали и оформить покупку."
            : $"Свяжитесь с менеджером {siteName}: {phone.Trim()}";
    }

    private static string Display(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();
    }

    private static string FormatMileage(int? km)
    {
        return km is null
            ? "—"
            : km.Value.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("ru-RU")) + " км";
    }
}

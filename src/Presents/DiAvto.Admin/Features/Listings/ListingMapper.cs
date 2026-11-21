using DiAvto.Domain;
using DiAvto.Admin.Features.Marketplaces;

namespace DiAvto.Admin.Features.Listings;

/// <summary>
/// Переносит данные площадки в <see cref="CarListing"/>. Единое место для
/// правил «что перезаписать, а что оставить», чтобы поиск и обновление
/// карточки не расходились.
/// </summary>
public static class ListingMapper
{
    public static CarListing CreateFromSearchResult(
        SourceSite source,
        Brand brand,
        CarSeries? series,
        MarketplaceSearchItem item,
        MarketplaceCarDetails? details,
        DateTime now)
    {
        var listing = new CarListing
        {
            SourceSiteId = source.Id,
            ExternalId = item.ExternalId,
            CreatedAt = now
        };

        ApplySearchResult(listing, source, brand, series, item, details, now);
        return listing;
    }

    public static void ApplySearchResult(
        CarListing listing,
        SourceSite source,
        Brand brand,
        CarSeries? series,
        MarketplaceSearchItem item,
        MarketplaceCarDetails? details,
        DateTime now)
    {
        listing.Title = FirstNonEmpty(details?.Title, item.Title)!;
        listing.BrandName = FirstNonEmpty(details?.BrandName, brand.Name);
        listing.SeriesName = FirstNonEmpty(details?.SeriesName, series?.Name);
        listing.SpecName = FirstNonEmpty(details?.SpecName, item.Title);
        listing.Year = details?.Year ?? item.Year;
        listing.FirstRegistration = FirstNonEmpty(details?.FirstRegistration, item.FirstRegistration);
        listing.PriceCny = (details?.PriceCny ?? 0m) > 0 ? details!.PriceCny : item.PriceCny;
        listing.GuidancePriceCny = details?.GuidancePriceCny;
        listing.MileageKm = details?.MileageKm ?? item.MileageKm;
        listing.CityName = FirstNonEmpty(details?.CityName, item.CityName);
        listing.Displacement = FirstNonEmpty(details?.Displacement, item.Displacement);
        listing.Gearbox = details?.Gearbox;
        listing.FuelType = details?.FuelType;
        listing.EmissionStandard = FirstNonEmpty(details?.EmissionStandard, item.EmissionStandard);
        listing.Color = details?.Color;
        listing.TransferCount = details?.TransferCount;
        listing.BodyType = details?.BodyType;
        listing.ImageUrl = FirstNonEmpty(details?.ImageUrl, item.ImageUrl);
        listing.DealerId = FirstNonEmpty(details?.DealerId, item.DealerId);
        listing.DealerName = details?.DealerName;
        listing.PublicUrl = FirstNonEmpty(
            details?.PublicUrl,
            ApplyUrlTemplate(source.DetailUrlTemplate, item.ExternalId));
        listing.Description = details?.Description;
        listing.IsNewEnergy = details?.IsNewEnergy ?? item.IsNewEnergy;
        listing.PublishedAtText = details?.PublishedAtText;
        listing.LastFetchedAt = now;

        if (!string.IsNullOrWhiteSpace(details?.RawJson))
        {
            listing.RawJson = details.RawJson;
        }

        var photos = details?.Photos ?? [];
        if (photos.Count > 0)
        {
            ReplacePhotos(listing, photos);
        }
        else if (listing.Photos.Count == 0 && !string.IsNullOrWhiteSpace(listing.ImageUrl))
        {
            listing.Photos.Add(new CarPhoto { Url = listing.ImageUrl, SortOrder = 0 });
        }

        listing.PhotoCount = listing.Photos.Count > 0
            ? listing.Photos.Count
            : Math.Max(details?.PhotoCount ?? 0, item.PhotoCount);
    }

    /// <summary>Обновляет существующую карточку деталями, не затирая данные пустыми значениями.</summary>
    public static void ApplyDetails(CarListing listing, MarketplaceCarDetails details, DateTime now)
    {
        listing.Title = FirstNonEmpty(details.Title, listing.Title)!;
        listing.BrandName = FirstNonEmpty(details.BrandName, listing.BrandName);
        listing.SeriesName = FirstNonEmpty(details.SeriesName, listing.SeriesName);
        listing.SpecName = FirstNonEmpty(details.SpecName, listing.SpecName);
        listing.Year = details.Year ?? listing.Year;
        listing.FirstRegistration = FirstNonEmpty(details.FirstRegistration, listing.FirstRegistration);

        if (details.PriceCny > 0)
        {
            listing.PriceCny = details.PriceCny;
        }

        listing.GuidancePriceCny = details.GuidancePriceCny ?? listing.GuidancePriceCny;
        listing.MileageKm = details.MileageKm ?? listing.MileageKm;
        listing.CityName = FirstNonEmpty(details.CityName, listing.CityName);
        listing.Displacement = FirstNonEmpty(details.Displacement, listing.Displacement);
        listing.Gearbox = FirstNonEmpty(details.Gearbox, listing.Gearbox);
        listing.FuelType = FirstNonEmpty(details.FuelType, listing.FuelType);
        listing.EmissionStandard = FirstNonEmpty(details.EmissionStandard, listing.EmissionStandard);
        listing.Color = FirstNonEmpty(details.Color, listing.Color);
        listing.TransferCount = details.TransferCount ?? listing.TransferCount;
        listing.BodyType = FirstNonEmpty(details.BodyType, listing.BodyType);
        listing.ImageUrl = FirstNonEmpty(details.ImageUrl, listing.ImageUrl);
        listing.DealerId = FirstNonEmpty(details.DealerId, listing.DealerId);
        listing.DealerName = FirstNonEmpty(details.DealerName, listing.DealerName);
        listing.PublicUrl = FirstNonEmpty(details.PublicUrl, listing.PublicUrl);
        listing.Description = FirstNonEmpty(details.Description, listing.Description);
        listing.IsNewEnergy = details.IsNewEnergy;
        listing.PublishedAtText = FirstNonEmpty(details.PublishedAtText, listing.PublishedAtText);
        listing.RawJson = FirstNonEmpty(details.RawJson, listing.RawJson);
        listing.LastFetchedAt = now;

        if (details.Photos.Count > 0)
        {
            ReplacePhotos(listing, details.Photos);
            listing.PhotoCount = details.Photos.Count;
        }
    }

    private static void ReplacePhotos(CarListing listing, IReadOnlyList<string> photos)
    {
        listing.Photos.Clear();
        for (var index = 0; index < photos.Count; index++)
        {
            listing.Photos.Add(new CarPhoto { Url = photos[index], SortOrder = index });
        }
    }

    private static string? ApplyUrlTemplate(string? template, string externalId)
    {
        return string.IsNullOrWhiteSpace(template)
            ? null
            : template.Replace("{infoid}", Uri.EscapeDataString(externalId), StringComparison.Ordinal);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    }
}


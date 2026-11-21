using System.Collections.Concurrent;
using DiAvto.Admin.Data;
using DiAvto.Domain;
using DiAvto.Admin.Features.Currency;
using DiAvto.Admin.Features.Listings;
using DiAvto.Admin.Features.Marketplaces;
using DiAvto.Application.Pricing;
using DiAvto.Admin.Features.Settings;using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DiAvto.Admin.Features.Search;

public sealed class SearchExecutionService
{
    private readonly DiAvtoDbContext _db;
    private readonly IMarketplaceClientFactory _clients;
    private readonly CurrencyService _currency;
    private readonly ServiceTariffService _tariffs;
    private readonly AppSettingsService _settings;
    private readonly SearchOptions _options;
    private readonly ILogger<SearchExecutionService> _logger;

    public SearchExecutionService(
        DiAvtoDbContext db,
        IMarketplaceClientFactory clients,
        CurrencyService currency,
        ServiceTariffService tariffs,
        AppSettingsService settings,
        IOptions<SearchOptions> options,
        ILogger<SearchExecutionService> logger)
    {
        _db = db;
        _clients = clients;
        _currency = currency;
        _tariffs = tariffs;
        _settings = settings;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SearchRequest> ExecuteAsync(SearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        SearchCriteriaValidator.Validate(criteria);

        var brand = await _db.Brands
            .Include(x => x.SourceSite)
            .FirstOrDefaultAsync(x => x.Id == criteria.BrandId, cancellationToken)
            ?? throw new InvalidOperationException("Выбранный бренд не найден.");

        if (!brand.SourceSite.IsEnabled)
        {
            throw new InvalidOperationException($"Источник «{brand.SourceSite.Name}» отключён.");
        }

        CarSeries? series = null;
        if (criteria.SeriesId is not null)
        {
            series = await _db.CarSeries
                .FirstOrDefaultAsync(x => x.Id == criteria.SeriesId && x.BrandId == brand.Id, cancellationToken)
                ?? throw new InvalidOperationException("Выбранная модель не относится к этому бренду.");
        }

        var quote = await _currency.GetCurrentAsync(cancellationToken: cancellationToken);
        var markup = await _currency.GetMarkupPercentAsync(cancellationToken);

        criteria.ExchangeRate = quote.Rate;
        criteria.MarkupPercent = markup;

        // Сначала вычитаем расходы из бюджета, и только потом идём на площадку.
        var rates = await _currency.GetRatesAsync(["CNY", "EUR", "JPY", "KRW"], cancellationToken);
        var activeTariffs = await _tariffs.GetActiveAsync(cancellationToken);
        var cityTariff = activeTariffs.FirstOrDefault(x => x.Id == criteria.DeliveryFromVladivostokTariffId)
                         ?? ServiceTariffPicker.PickDefault(
                             activeTariffs.Where(x => x.Kind == ServiceTariffKind.DeliveryFromVladivostok));

        var template = new ImportPricingParameters
        {
            DeliveryToVladivostokAmount = criteria.DeliveryToVladivostokAmount,
            DeliveryToVladivostokCurrency = criteria.DeliveryToVladivostokCurrency,
            DeliveryToCityRub = cityTariff?.AmountRub ?? 0m,
            IncludeDeliveryToCity = criteria.IncludeDeliveryToCity,
            EngineCc = criteria.EngineCc,
            PowerHp = criteria.PowerHp,
            VehicleYear = criteria.VehicleYear,
            IsDiesel = criteria.IsDiesel,
            IsElectric = criteria.IsElectric,
            IsLegalEntity = criteria.IsLegalEntity,
            UseResaleUtilization = criteria.UseResaleUtilization,
            Rates = new ImportCurrencyRates
            {
                CnyRub = rates["CNY"].Rate,
                EurRub = rates["EUR"].Rate,
                JpyRub = rates["JPY"].Rate,
                KrwRub = rates["KRW"].Rate
            }
        };

        criteria.BudgetPlan = SearchBudgetPlanner.Build(
            criteria.PriceFrom,
            criteria.PriceTo,
            criteria.PriceCurrency,
            template);

        if (criteria.BudgetPlan.IsTooLow)
        {
            throw new InvalidOperationException(
                "Бюджет меньше суммы расходов. Увеличьте бюджет или измените тарифы.");
        }

        var resultsPerTier = Math.Clamp(
            await _settings.GetIntAsync("SearchResultsPerTier", _options.ResultsPerTier, cancellationToken),
            1,
            10);

        var request = new SearchRequest
        {
            PublicId = Guid.NewGuid().ToString("N")[..16],
            SourceSiteId = brand.SourceSiteId,
            BrandName = brand.DisplayName,
            SeriesName = series?.DisplayName,
            YearFrom = criteria.YearFrom,
            YearTo = criteria.YearTo,
            PriceFromCny = criteria.PriceFromCny,
            PriceToCny = criteria.PriceToCny,
            MileageMaxKm = criteria.MileageMaxKm,
            CityName = criteria.CityName,
            Status = SearchStatus.Running,
            ExchangeRate = quote.Rate,
            MarkupPercent = markup,
            CriteriaJson = SearchCriteriaSerializer.Serialize(criteria),
            CreatedAt = DateTime.UtcNow
        };

        _db.SearchRequests.Add(request);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var client = _clients.Get(brand.SourceSite.Key);
            var baseQuery = BuildMarketplaceQuery(criteria, brand, series);

            var cheapest = await CollectTierAsync(
                client, baseQuery, MarketplaceSort.PriceAscending, criteria, resultsPerTier,
                new HashSet<string>(StringComparer.Ordinal), cancellationToken);

            var usedIds = cheapest.Select(x => x.ExternalId).ToHashSet(StringComparer.Ordinal);
            var mostExpensive = await CollectTierAsync(
                client, baseQuery, MarketplaceSort.PriceDescending, criteria, resultsPerTier, usedIds, cancellationToken);

            var selected = cheapest.Concat(mostExpensive).ToList();
            var details = await LoadDetailsAsync(client, selected, cancellationToken);

            var listings = await UpsertListingsAsync(brand.SourceSite, brand, series, selected, details, cancellationToken);

            var resultCount = 0;
            AddTierItems(request, listings, cheapest, ResultTier.Cheapest, quote.Rate, markup, ref resultCount);
            AddTierItems(request, listings, mostExpensive, ResultTier.MostExpensive, quote.Rate, markup, ref resultCount);

            request.ResultCount = resultCount;
            request.Status = SearchStatus.Completed;
            request.CompletedAt = DateTime.UtcNow;
            request.ErrorMessage = null;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Поиск {PublicId} завершён: {Count} автомобилей (бренд {Brand}).",
                request.PublicId, resultCount, brand.Name);

            return request;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Поиск {PublicId} завершился ошибкой.", request.PublicId);

            request.Status = SearchStatus.Failed;
            request.CompletedAt = DateTime.UtcNow;
            request.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;

            try
            {
                await _db.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx, "Не удалось сохранить статус ошибки поиска {PublicId}.", request.PublicId);
            }

            throw;
        }
    }

    public async Task<SearchRequest?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        return await RequestsWithDetails()
            .FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public async Task<SearchRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await RequestsWithDetails()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<SearchRequest>> GetRecentAsync(int take = 30, CancellationToken cancellationToken = default)
    {
        return await _db.SearchRequests
            .AsNoTracking()
            .Include(x => x.SourceSite)
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync(cancellationToken);
    }

    private IQueryable<SearchRequest> RequestsWithDetails()
    {
        return _db.SearchRequests
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.SourceSite)
            .Include(x => x.Items)
                .ThenInclude(x => x.CarListing)
                    .ThenInclude(x => x.Photos)
            .Include(x => x.Items)
                .ThenInclude(x => x.CarListing)
                    .ThenInclude(x => x.SourceSite);
    }

    private static MarketplaceSearchQuery BuildMarketplaceQuery(
        SearchCriteria criteria,
        Brand brand,
        CarSeries? series)
    {
        return new MarketplaceSearchQuery
        {
            BrandExternalId = brand.ExternalId,
            SeriesExternalId = series?.ExternalId,
            CityExternalId = criteria.CityExternalId,
            YearFrom = criteria.YearFrom,
            YearTo = criteria.YearTo,
            PriceFromCny = criteria.PriceFromCny,
            PriceToCny = criteria.PriceToCny,
            MileageMaxKm = criteria.MileageMaxKm,
            DisplacementFromL = criteria.DisplacementFromL,
            DisplacementToL = criteria.DisplacementToL,
            Gearbox = criteria.Gearbox,
            FuelType = criteria.FuelType,
            Powertrain = criteria.Powertrain,
            CategoryId = criteria.CategoryId,
            Structure = criteria.Structure,
            Environmental = criteria.Environmental,
            Seats = criteria.Seats
        };
    }

    private async Task<List<MarketplaceSearchItem>> CollectTierAsync(
        IMarketplaceClient client,
        MarketplaceSearchQuery baseQuery,
        MarketplaceSort sort,
        SearchCriteria criteria,
        int needed,
        IReadOnlySet<string> excludedIds,
        CancellationToken cancellationToken)
    {
        var result = new List<MarketplaceSearchItem>(needed);
        var seen = new HashSet<string>(excludedIds, StringComparer.Ordinal);

        for (var page = 1; page <= Math.Max(1, _options.MaxPagesPerTier); page++)
        {
            var pageSize = Math.Clamp(_options.PageSize, 1, 20);
            var query = baseQuery.CloneForPage(sort, page, pageSize);
            var response = await client.SearchAsync(query, cancellationToken);

            foreach (var item in response.Items)
            {
                if (!MatchesCriteria(item, criteria))
                {
                    continue;
                }

                if (seen.Add(item.ExternalId))
                {
                    result.Add(item);
                    if (result.Count >= needed)
                    {
                        return result;
                    }
                }
            }

            if (page >= response.PageCount || response.Items.Count == 0)
            {
                break;
            }
        }

        return result;
    }

    private static bool MatchesCriteria(MarketplaceSearchItem item, SearchCriteria criteria)
    {
        if (item.PriceCny <= 0 || string.IsNullOrWhiteSpace(item.Title))
        {
            return false;
        }

        if (criteria.YearFrom is not null && (item.Year is null || item.Year < criteria.YearFrom))
        {
            return false;
        }

        if (criteria.YearTo is not null && (item.Year is null || item.Year > criteria.YearTo))
        {
            return false;
        }

        if (criteria.PriceFromCny is not null && item.PriceCny < criteria.PriceFromCny)
        {
            return false;
        }

        if (criteria.PriceToCny is not null && item.PriceCny > criteria.PriceToCny)
        {
            return false;
        }

        if (criteria.MileageMaxKm is not null && (item.MileageKm is null || item.MileageKm > criteria.MileageMaxKm))
        {
            return false;
        }

        return true;
    }

    private async Task<ConcurrentDictionary<string, MarketplaceCarDetails>> LoadDetailsAsync(
        IMarketplaceClient client,
        IReadOnlyList<MarketplaceSearchItem> items,
        CancellationToken cancellationToken)
    {
        var result = new ConcurrentDictionary<string, MarketplaceCarDetails>(StringComparer.Ordinal);
        if (items.Count == 0)
        {
            return result;
        }

        using var semaphore = new SemaphoreSlim(Math.Clamp(_options.DetailsConcurrency, 1, 10));

        var tasks = items.Select(async item =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var details = await client.GetCarDetailsAsync(item.ExternalId, cancellationToken);
                result[item.ExternalId] = details;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Не удалось загрузить детали автомобиля {ExternalId}; используем данные из выдачи.", item.ExternalId);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return result;
    }

    private async Task<Dictionary<string, CarListing>> UpsertListingsAsync(
        SourceSite source,
        Brand brand,
        CarSeries? series,
        IReadOnlyList<MarketplaceSearchItem> items,
        ConcurrentDictionary<string, MarketplaceCarDetails> details,
        CancellationToken cancellationToken)
    {
        var externalIds = items.Select(x => x.ExternalId).Distinct(StringComparer.Ordinal).ToList();
        var existing = await _db.CarListings
            .Include(x => x.Photos)
            .Where(x => x.SourceSiteId == source.Id && externalIds.Contains(x.ExternalId))
            .ToListAsync(cancellationToken);

        var byExternalId = existing.ToDictionary(x => x.ExternalId, StringComparer.Ordinal);
        var now = DateTime.UtcNow;

        foreach (var item in items)
        {
            details.TryGetValue(item.ExternalId, out var detail);

            if (byExternalId.TryGetValue(item.ExternalId, out var listing))
            {
                ListingMapper.ApplySearchResult(listing, source, brand, series, item, detail, now);
            }
            else
            {
                listing = ListingMapper.CreateFromSearchResult(source, brand, series, item, detail, now);
                _db.CarListings.Add(listing);
                byExternalId[item.ExternalId] = listing;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return byExternalId;
    }

    private static void AddTierItems(
        SearchRequest request,
        IReadOnlyDictionary<string, CarListing> listings,
        IReadOnlyList<MarketplaceSearchItem> tierItems,
        ResultTier tier,
        decimal rate,
        decimal markup,
        ref int resultCount)
    {
        var rank = 1;

        foreach (var item in tierItems)
        {
            if (!listings.TryGetValue(item.ExternalId, out var listing))
            {
                continue;
            }

            var priceCny = listing.PriceCny;
            request.Items.Add(new SearchResultItem
            {
                SearchRequest = request,
                CarListing = listing,
                Tier = tier,
                Rank = rank++,
                PriceCny = priceCny,
                PriceRub = PricingCalculator.CnyToRub(priceCny, rate, markup)
            });

            resultCount++;
        }
    }
}



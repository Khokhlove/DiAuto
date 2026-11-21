using DiAvto.Admin.Data;
using DiAvto.Domain;
using DiAvto.Admin.Features.Currency;
using DiAvto.Application.Pricing;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Admin.Features.Cards;

public sealed class ClientCardService
{
    private readonly DiAvtoDbContext _db;
    private readonly CurrencyService _currency;
    private readonly ServiceTariffService _tariffs;
    private readonly ILogger<ClientCardService> _logger;

    public ClientCardService(
        DiAvtoDbContext db,
        CurrencyService currency,
        ServiceTariffService tariffs,
        ILogger<ClientCardService> logger)
    {
        _db = db;
        _currency = currency;
        _tariffs = tariffs;
        _logger = logger;
    }

    public async Task<ClientCard> CreateAsync(
        int listingId,
        decimal deliveryToVladivostokAmount,
        string deliveryToVladivostokCurrency,
        int engineCc,
        int powerHp,
        int vehicleYear,
        bool isDiesel,
        bool isElectric,
        bool isLegalEntity,
        bool useResaleUtilization,
        bool includeDeliveryToCity,
        int? deliveryFromVladivostokTariffId,
        string? customerName,
        string countryName,
        string? note,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var listing = await _db.CarListings
            .Include(x => x.SourceSite)
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == listingId, cancellationToken)
            ?? throw new InvalidOperationException("Автомобиль не найден.");

        ServiceTariff? cityTariff = null;
        if (includeDeliveryToCity && deliveryFromVladivostokTariffId is > 0)
        {
            cityTariff = (await _tariffs.GetByIdsAsync([deliveryFromVladivostokTariffId.Value], cancellationToken))
                .FirstOrDefault(x => x.Kind == ServiceTariffKind.DeliveryFromVladivostok)
                ?? throw new InvalidOperationException("Тариф доставки до города клиента не найден или отключён.");
        }

        var rates = await _currency.GetRatesAsync(["CNY", "EUR", "JPY", "KRW"], cancellationToken);
        var breakdown = ImportPricingCalculator.Calculate(new ImportPricingParameters
        {
            CarPrice = listing.PriceCny,
            CarCurrency = "CNY",
            DeliveryToVladivostokAmount = deliveryToVladivostokAmount,
            DeliveryToVladivostokCurrency = deliveryToVladivostokCurrency,
            DeliveryToCityRub = cityTariff?.AmountRub ?? 0m,
            IncludeDeliveryToCity = includeDeliveryToCity && cityTariff is not null,
            EngineCc = engineCc,
            PowerHp = powerHp,
            VehicleYear = vehicleYear,
            IsDiesel = isDiesel,
            IsElectric = isElectric,
            IsLegalEntity = isLegalEntity,
            UseResaleUtilization = useResaleUtilization,
            Rates = new ImportCurrencyRates
            {
                CnyRub = rates["CNY"].Rate,
                EurRub = rates["EUR"].Rate,
                JpyRub = rates["JPY"].Rate,
                KrwRub = rates["KRW"].Rate
            }
        });

        var card = new ClientCard
        {
            PublicId = Guid.NewGuid().ToString("N"),
            CarListingId = listing.Id,
            CustomerName = Normalize(customerName),
            CountryName = string.IsNullOrWhiteSpace(countryName) ? "Китай" : countryName.Trim(),
            DestinationCity = cityTariff?.Title ?? "Без доставки",
            DeliveryToVladivostokTitle = deliveryToVladivostokAmount > 0
                ? $"{deliveryToVladivostokAmount:N0} {deliveryToVladivostokCurrency}"
                : "Доставка до Владивостока",
            DeliveryFromVladivostokTitle = cityTariff?.Title,
            InspectionTitle = "Осмотр (900 ¥)",
            CustomsClearanceTitle = "Таможня, брокер, лаборатория и комиссии",
            CarPriceCny = listing.PriceCny,
            ExchangeRate = rates["CNY"].Rate,
            MarkupPercent = 0m,
            CarPriceRub = breakdown.CarPriceRub,
            DeliveryToVladivostokRub = breakdown.DeliveryToVladivostokRub,
            DeliveryFromVladivostokRub = breakdown.DeliveryToCityRub,
            InspectionRub = breakdown.InspectionRub,
            CustomsClearanceRub = breakdown.Customs.TotalRub +
                                  breakdown.BrokerLaboratoryRub +
                                  breakdown.OurCommissionRub +
                                  breakdown.BankCommissionRub,
            TotalRub = breakdown.TotalRub,
            Note = Normalize(note),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _db.ClientCards.Add(card);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Создана карточка клиента {PublicId} для автомобиля {ListingId}: итого {TotalRub} ₽.",
            card.PublicId, card.CarListingId, card.TotalRub);

        return card;
    }

    public async Task<ClientCard?> GetPublicAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return null;
        }

        return await _db.ClientCards
            .AsNoTracking()
            .Include(x => x.CarListing)
                .ThenInclude(x => x.SourceSite)
            .Include(x => x.CarListing)
                .ThenInclude(x => x.Photos)
            .FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public async Task<List<ClientCard>> GetRecentAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        return await _db.ClientCards
            .AsNoTracking()
            .Include(x => x.CarListing)
                .ThenInclude(x => x.SourceSite)
            .Include(x => x.CarListing)
                .ThenInclude(x => x.Photos)
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ClientCard>> GetForListingAsync(
        int listingId,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        return await _db.ClientCards
            .AsNoTracking()
            .Where(x => x.CarListingId == listingId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var card = await _db.ClientCards.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (card is null)
        {
            return false;
        }

        card.IsActive = isActive;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}



using DiAvto.Domain;
using DiAvto.Infrastructure.Persistence;
using DiAvto.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Tests;

public class ClientCardPublicServiceTests
{
    [Fact]
    public async Task GetAsync_MapsCarPhotosAndContact()
    {
        await using var fixture = await CardFixture.CreateAsync();
        var service = new ClientCardPublicService(fixture.Db);

        var card = await service.GetAsync(fixture.PublicId);

        Assert.NotNull(card);
        Assert.True(card.IsActive);
        Assert.Equal("Volvo V40 2013 2.0T Автомат", card.Title);
        Assert.Equal("Вэньчжоу", card.CityName);
        Assert.Equal(2, card.PhotoUrls.Count);
        Assert.Equal("/images/cars/volvo-1.jpg", card.MainPhotoUrl);
        Assert.Equal(14, card.Specs.Count);
        Assert.Equal("ДИ.авто", card.SiteName);
        Assert.Contains("+7 (923) 317-77-79", card.ContactText);
        Assert.Equal(1_620_000m, card.TotalRub);
        Assert.True(card.HasCityDelivery);
    }

    [Fact]
    public async Task GetAsync_ReturnsInactiveCardForFriendlyMessage()
    {
        await using var fixture = await CardFixture.CreateAsync(isActive: false);
        var service = new ClientCardPublicService(fixture.Db);

        var card = await service.GetAsync(fixture.PublicId);

        Assert.NotNull(card);
        Assert.False(card.IsActive);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullForUnknownId()
    {
        await using var fixture = await CardFixture.CreateAsync();
        var service = new ClientCardPublicService(fixture.Db);

        Assert.Null(await service.GetAsync("unknown"));
        Assert.Null(await service.GetAsync(""));
    }

    private sealed class CardFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private CardFixture(SqliteConnection connection, DiAvtoDbContext db, string publicId)
        {
            _connection = connection;
            Db = db;
            PublicId = publicId;
        }

        public DiAvtoDbContext Db { get; }

        public string PublicId { get; }

        public static async Task<CardFixture> CreateAsync(bool isActive = true)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<DiAvtoDbContext>()
                .UseSqlite(connection)
                .Options;

            var db = new DiAvtoDbContext(options);
            await db.Database.EnsureCreatedAsync();

            var source = new SourceSite
            {
                Key = "che168",
                Name = "Che168",
                BaseUrl = "https://www.che168.com",
                AttributionText = "che168.com"
            };

            var listing = new CarListing
            {
                SourceSite = source,
                ExternalId = "1001",
                Title = "沃尔沃V40 2013款 2.0T 智雅版",
                BrandName = "沃尔沃",
                SeriesName = "沃尔沃V40",
                Year = 2013,
                FirstRegistration = "2013-06",
                PriceCny = 120_000m,
                MileageKm = 85_000,
                CityName = "温州",
                Displacement = "2.0T",
                Gearbox = "自动",
                FuelType = "汽油",
                EmissionStandard = "欧IV",
                Color = "香槟/棕色",
                TransferCount = 1,
                BodyType = "旅行车",
                ImageUrl = "/images/cars/volvo-1.jpg"
            };

            listing.Photos.Add(new CarPhoto { Url = "/images/cars/volvo-1.jpg", SortOrder = 0 });
            listing.Photos.Add(new CarPhoto { Url = "/images/cars/volvo-2.jpg", SortOrder = 1 });

            var publicId = Guid.NewGuid().ToString("N");
            var card = new ClientCard
            {
                PublicId = publicId,
                CarListing = listing,
                CustomerName = "Иван",
                CountryName = "Китай",
                DestinationCity = "Санкт-Петербург",
                DeliveryToVladivostokTitle = "150 000 RUB",
                DeliveryFromVladivostokTitle = "Санкт-Петербург",
                CarPriceCny = 120_000m,
                ExchangeRate = 12m,
                MarkupPercent = 0m,
                CarPriceRub = 1_440_000m,
                TotalRub = 1_620_000m,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "test"
            };

            db.AddRange(
                source,
                listing,
                card,
                new SiteSetting { Key = "company_name", Value = "ДИ.авто" },
                new SiteSetting { Key = "phone", Value = "+7 (923) 317-77-79" });

            await db.SaveChangesAsync();

            return new CardFixture(connection, db, publicId);
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}

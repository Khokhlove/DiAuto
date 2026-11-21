using DiAvto.Application.Public;
using DiAvto.Domain;
using DiAvto.Infrastructure.Persistence;
using DiAvto.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Tests;

public class ShowcaseCarServiceTests
{
    [Fact]
    public async Task GetPublishedAsync_FiltersHiddenCarsAndAppliesFacets()
    {
        await using var fixture = await ShowcaseFixture.CreateAsync();
        var service = new ShowcaseCarService(fixture.Db);

        var all = await service.GetPublishedAsync(new ShowcaseCarFilter());
        Assert.Equal(2, all.Count);
        Assert.DoesNotContain(all, x => x.Name == "Hidden");

        var china = await service.GetPublishedAsync(new ShowcaseCarFilter { Country = "Китай" });
        Assert.Single(china);
        Assert.Equal("Visible China", china[0].Name);

        var sorted = await service.GetPublishedAsync(new ShowcaseCarFilter { Sort = "price-asc" });
        Assert.Equal("Visible China", sorted[0].Name);

        var facets = await service.GetFacetsAsync();
        Assert.Contains("Китай", facets.Countries);
        Assert.Contains("Япония", facets.Countries);
        Assert.Equal(1_000_000m, facets.MinPrice);
        Assert.Equal(2_000_000m, facets.MaxPrice);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOnlyPublishedCars()
    {
        await using var fixture = await ShowcaseFixture.CreateAsync();
        var service = new ShowcaseCarService(fixture.Db);

        Assert.NotNull(await service.GetByIdAsync(fixture.VisibleId));
        Assert.Null(await service.GetByIdAsync(fixture.HiddenId));
    }

    private sealed class ShowcaseFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private ShowcaseFixture(SqliteConnection connection, DiAvtoDbContext db, int visibleId, int hiddenId)
        {
            _connection = connection;
            Db = db;
            VisibleId = visibleId;
            HiddenId = hiddenId;
        }

        public DiAvtoDbContext Db { get; }

        public int VisibleId { get; }

        public int HiddenId { get; }

        public static async Task<ShowcaseFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<DiAvtoDbContext>()
                .UseSqlite(connection)
                .Options;

            var db = new DiAvtoDbContext(options);
            await db.Database.EnsureCreatedAsync();

            var china = new ShowcaseCar
            {
                Name = "Visible China",
                Make = "Geely",
                Year = 2024,
                PriceRub = 1_000_000m,
                Country = "Китай",
                IsPublished = true,
                InStock = true,
                SortOrder = 1
            };
            china.Photos.Add(new ShowcaseCarPhoto { Url = "/images/cars/china.jpg", SortOrder = 0 });

            var japan = new ShowcaseCar
            {
                Name = "Visible Japan",
                Make = "Toyota",
                Year = 2023,
                PriceRub = 2_000_000m,
                Country = "Япония",
                IsPublished = true,
                InStock = false,
                SortOrder = 2
            };
            japan.Photos.Add(new ShowcaseCarPhoto { Url = "/images/cars/japan.jpg", SortOrder = 0 });

            var hidden = new ShowcaseCar
            {
                Name = "Hidden",
                Make = "Nissan",
                Year = 2022,
                PriceRub = 500_000m,
                Country = "Япония",
                IsPublished = false,
                SortOrder = 3
            };

            db.ShowcaseCars.AddRange(china, japan, hidden);
            await db.SaveChangesAsync();

            return new ShowcaseFixture(connection, db, china.Id, hidden.Id);
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}

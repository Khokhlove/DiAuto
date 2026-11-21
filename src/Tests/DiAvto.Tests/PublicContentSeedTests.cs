using DiAvto.Infrastructure.Persistence;
using DiAvto.Infrastructure.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Tests;

public class PublicContentSeedTests
{
    [Fact]
    public async Task MigrationAndSeed_CreatePublicContent()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<DiAvtoDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new DiAvtoDbContext(options);
        await db.Database.MigrateAsync();
        await PublicContentSeeder.SeedAsync(db);

        Assert.Equal(140, await db.ShowcaseCars.CountAsync());
        Assert.Equal(6, await db.Reviews.CountAsync());
        Assert.Equal(3, await db.Offices.CountAsync());
        Assert.Equal(7, await db.FaqItems.CountAsync());
        Assert.True(await db.SiteSettings.CountAsync() >= 8);
        Assert.True(await db.ContentSectionItems.CountAsync() >= 40);
        Assert.True(await db.ShowcaseCarPhotos.CountAsync() > 2000);
    }

    [Fact]
    public async Task ShowcaseCars_ArePublishedAndHaveImages()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<DiAvtoDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new DiAvtoDbContext(options);
        await db.Database.MigrateAsync();
        await PublicContentSeeder.SeedAsync(db);

        var first = await db.ShowcaseCars
            .Include(x => x.Photos)
            .OrderBy(x => x.SortOrder)
            .FirstAsync();

        Assert.True(first.IsPublished);
        Assert.NotEmpty(first.Photos);
        Assert.True(first.PriceRub > 0);
    }
}

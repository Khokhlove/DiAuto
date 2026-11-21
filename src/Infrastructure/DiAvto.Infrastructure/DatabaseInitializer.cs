using DiAvto.Infrastructure.Persistence;
using DiAvto.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DiAvto.Infrastructure;

public static class DatabaseInitializer
{
    /// <summary>
    /// Применяет миграции и (по умолчанию) загружает публичный контент витрины.
    /// Публичный контент загружает только сайт, чтобы два хоста не сидировали его одновременно.
    /// </summary>
    public static async Task InitializeAsync(
        IServiceProvider services,
        bool seedPublicContent = true,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DiAvtoDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        if (seedPublicContent)
        {
            await PublicContentSeeder.SeedAsync(db, cancellationToken);
        }
    }
}

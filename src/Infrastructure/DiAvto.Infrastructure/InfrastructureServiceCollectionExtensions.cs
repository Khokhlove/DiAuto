using DiAvto.Application.Public;
using DiAvto.Infrastructure.Persistence;
using DiAvto.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiAvto.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddDiAvtoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string? contentRoot = null)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=Data/diavto.db;Cache=Shared";

        var sqlite = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
        if (!string.IsNullOrWhiteSpace(sqlite.DataSource) && !Path.IsPathRooted(sqlite.DataSource))
        {
            var root = contentRoot ?? Directory.GetCurrentDirectory();
            sqlite.DataSource = Path.GetFullPath(Path.Combine(root, sqlite.DataSource));
        }

        var directory = Path.GetDirectoryName(sqlite.DataSource);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        services.AddDbContext<DiAvtoDbContext>(options => options.UseSqlite(sqlite.ConnectionString));

        services.AddScoped<IShowcaseCarService, ShowcaseCarService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IOfficeService, OfficeService>();
        services.AddScoped<IFaqService, FaqService>();
        services.AddScoped<ISiteSettingsService, SiteSettingsService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IContentSectionService, ContentSectionService>();
        services.AddScoped<IClientCardPublicService, ClientCardPublicService>();

        return services;
    }
}

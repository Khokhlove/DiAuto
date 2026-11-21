using DiAvto.Domain;
using DiAvto.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Infrastructure.Persistence;

public class DiAvtoDbContext(DbContextOptions<DiAvtoDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<SourceSite> SourceSites => Set<SourceSite>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<CarSeries> CarSeries => Set<CarSeries>();
    public DbSet<CarListing> CarListings => Set<CarListing>();
    public DbSet<CarPhoto> CarPhotos => Set<CarPhoto>();
    public DbSet<SearchRequest> SearchRequests => Set<SearchRequest>();
    public DbSet<SearchResultItem> SearchResultItems => Set<SearchResultItem>();
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();
    public DbSet<ServiceTariff> ServiceTariffs => Set<ServiceTariff>();
    public DbSet<ClientCard> ClientCards => Set<ClientCard>();

    public DbSet<ShowcaseCar> ShowcaseCars => Set<ShowcaseCar>();
    public DbSet<ShowcaseCarPhoto> ShowcaseCarPhotos => Set<ShowcaseCarPhoto>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Office> Offices => Set<Office>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    public DbSet<ContentSection> ContentSections => Set<ContentSection>();
    public DbSet<ContentSectionItem> ContentSectionItems => Set<ContentSectionItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SourceSite>(entity =>
        {
            entity.HasIndex(x => x.Key).IsUnique();
            entity.Property(x => x.Key).HasMaxLength(64);
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.BaseUrl).HasMaxLength(500);
            entity.Property(x => x.DetailUrlTemplate).HasMaxLength(1000);
            entity.Property(x => x.LogoUrl).HasMaxLength(1000);
            entity.Property(x => x.AttributionText).HasMaxLength(500);
        });

        builder.Entity<Brand>(entity =>
        {
            entity.HasIndex(x => new { x.SourceSiteId, x.ExternalId }).IsUnique();
            entity.Property(x => x.ExternalId).HasMaxLength(64);
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.NameLatin).HasMaxLength(160);
            entity.Property(x => x.NameRu).HasMaxLength(160);
            entity.Property(x => x.IconUrl).HasMaxLength(1000);

            entity.HasOne(x => x.SourceSite)
                .WithMany(x => x.Brands)
                .HasForeignKey(x => x.SourceSiteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CarSeries>(entity =>
        {
            entity.HasIndex(x => new { x.BrandId, x.ExternalId }).IsUnique();
            entity.Property(x => x.ExternalId).HasMaxLength(64);
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.NameRu).HasMaxLength(160);
            entity.Property(x => x.Pinyin).HasMaxLength(160);
            entity.Property(x => x.IconUrl).HasMaxLength(1000);
            entity.Property(x => x.PriceRange).HasMaxLength(64);

            entity.HasOne(x => x.Brand)
                .WithMany(x => x.Series)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CarListing>(entity =>
        {
            entity.HasIndex(x => new { x.SourceSiteId, x.ExternalId }).IsUnique();
            entity.HasIndex(x => x.PriceCny);
            entity.HasIndex(x => x.LastFetchedAt);
            entity.Property(x => x.ExternalId).HasMaxLength(64);
            entity.Property(x => x.Title).HasMaxLength(500);
            entity.Property(x => x.BrandName).HasMaxLength(160);
            entity.Property(x => x.SeriesName).HasMaxLength(160);
            entity.Property(x => x.SpecName).HasMaxLength(300);
            entity.Property(x => x.CityName).HasMaxLength(120);
            entity.Property(x => x.ImageUrl).HasMaxLength(1000);
            entity.Property(x => x.PublicUrl).HasMaxLength(1000);
            entity.Property(x => x.DealerId).HasMaxLength(64);
            entity.Property(x => x.DealerName).HasMaxLength(300);
            entity.Property(x => x.PriceCny).HasPrecision(18, 2);
            entity.Property(x => x.GuidancePriceCny).HasPrecision(18, 2);

            entity.HasOne(x => x.SourceSite)
                .WithMany(x => x.CarListings)
                .HasForeignKey(x => x.SourceSiteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CarPhoto>(entity =>
        {
            entity.HasIndex(x => new { x.CarListingId, x.SortOrder });
            entity.Property(x => x.Url).HasMaxLength(1000);

            entity.HasOne(x => x.CarListing)
                .WithMany(x => x.Photos)
                .HasForeignKey(x => x.CarListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SearchRequest>(entity =>
        {
            entity.HasIndex(x => x.PublicId).IsUnique();
            entity.HasIndex(x => x.CreatedAt);
            entity.Property(x => x.PublicId).HasMaxLength(32);
            entity.Property(x => x.BrandName).HasMaxLength(160);
            entity.Property(x => x.SeriesName).HasMaxLength(160);
            entity.Property(x => x.CityName).HasMaxLength(120);
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.Property(x => x.MarkupPercent).HasPrecision(9, 2);
            entity.Property(x => x.PriceFromCny).HasPrecision(18, 2);
            entity.Property(x => x.PriceToCny).HasPrecision(18, 2);

            entity.HasOne(x => x.SourceSite)
                .WithMany(x => x.SearchRequests)
                .HasForeignKey(x => x.SourceSiteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SearchResultItem>(entity =>
        {
            entity.HasIndex(x => new { x.SearchRequestId, x.Tier, x.Rank }).IsUnique();
            entity.HasIndex(x => new { x.SearchRequestId, x.CarListingId }).IsUnique();
            entity.Property(x => x.PriceCny).HasPrecision(18, 2);
            entity.Property(x => x.PriceRub).HasPrecision(18, 2);

            entity.HasOne(x => x.SearchRequest)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SearchRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.CarListing)
                .WithMany(x => x.SearchResults)
                .HasForeignKey(x => x.CarListingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CurrencyRate>(entity =>
        {
            entity.HasIndex(x => new { x.Source, x.BaseCurrency, x.QuoteCurrency, x.EffectiveDate }).IsUnique();
            entity.Property(x => x.BaseCurrency).HasMaxLength(8);
            entity.Property(x => x.QuoteCurrency).HasMaxLength(8);
            entity.Property(x => x.Source).HasMaxLength(32);
            entity.Property(x => x.Rate).HasPrecision(18, 6);
        });

        builder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(x => x.Key);
            entity.Property(x => x.Key).HasMaxLength(160);
            entity.Property(x => x.Value).HasMaxLength(4000);
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        builder.Entity<AdminAuditLog>(entity =>
        {
            entity.HasIndex(x => x.CreatedAt);
            entity.Property(x => x.UserName).HasMaxLength(256);
            entity.Property(x => x.Action).HasMaxLength(160);
            entity.Property(x => x.Details).HasMaxLength(4000);
        });

        builder.Entity<ServiceTariff>(entity =>
        {
            entity.HasIndex(x => new { x.Kind, x.IsActive, x.SortOrder });
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.AmountRub).HasPrecision(18, 2);
        });

        builder.Entity<ClientCard>(entity =>
        {
            entity.HasIndex(x => x.PublicId).IsUnique();
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.CarListingId, x.CreatedAt });
            entity.Property(x => x.PublicId).HasMaxLength(32);
            entity.Property(x => x.CustomerName).HasMaxLength(200);
            entity.Property(x => x.CountryName).HasMaxLength(120);
            entity.Property(x => x.DestinationCity).HasMaxLength(120);
            entity.Property(x => x.DeliveryToVladivostokTitle).HasMaxLength(200);
            entity.Property(x => x.DeliveryFromVladivostokTitle).HasMaxLength(200);
            entity.Property(x => x.InspectionTitle).HasMaxLength(200);
            entity.Property(x => x.CustomsClearanceTitle).HasMaxLength(200);
            entity.Property(x => x.CarPriceCny).HasPrecision(18, 2);
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.Property(x => x.MarkupPercent).HasPrecision(9, 2);
            entity.Property(x => x.CarPriceRub).HasPrecision(18, 2);
            entity.Property(x => x.DeliveryToVladivostokRub).HasPrecision(18, 2);
            entity.Property(x => x.DeliveryFromVladivostokRub).HasPrecision(18, 2);
            entity.Property(x => x.InspectionRub).HasPrecision(18, 2);
            entity.Property(x => x.CustomsClearanceRub).HasPrecision(18, 2);
            entity.Property(x => x.TotalRub).HasPrecision(18, 2);
            entity.Property(x => x.Note).HasMaxLength(2000);
            entity.Property(x => x.CreatedBy).HasMaxLength(256);

            entity.HasOne(x => x.CarListing)
                .WithMany(x => x.ClientCards)
                .HasForeignKey(x => x.CarListingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ShowcaseCar>(entity =>
        {
            entity.HasIndex(x => new { x.IsPublished, x.SortOrder });
            entity.HasIndex(x => x.PriceRub);
            entity.HasIndex(x => x.Make);
            entity.Property(x => x.Name).HasMaxLength(300);
            entity.Property(x => x.Make).HasMaxLength(120);
            entity.Property(x => x.Country).HasMaxLength(80);
            entity.Property(x => x.Fuel).HasMaxLength(80);
            entity.Property(x => x.BodyType).HasMaxLength(80);
            entity.Property(x => x.Transmission).HasMaxLength(80);
            entity.Property(x => x.Drive).HasMaxLength(80);
            entity.Property(x => x.Engine).HasMaxLength(80);
            entity.Property(x => x.ExternalId).HasMaxLength(80);
            entity.Property(x => x.PriceRub).HasPrecision(18, 2);
        });

        builder.Entity<ShowcaseCarPhoto>(entity =>
        {
            entity.HasIndex(x => new { x.ShowcaseCarId, x.SortOrder });
            entity.Property(x => x.Url).HasMaxLength(1000);

            entity.HasOne(x => x.ShowcaseCar)
                .WithMany(x => x.Photos)
                .HasForeignKey(x => x.ShowcaseCarId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Review>(entity =>
        {
            entity.HasIndex(x => new { x.IsVisible, x.SortOrder });
            entity.Property(x => x.Type).HasMaxLength(32);
            entity.Property(x => x.AuthorName).HasMaxLength(160);
            entity.Property(x => x.City).HasMaxLength(120);
            entity.Property(x => x.Source).HasMaxLength(80);
            entity.Property(x => x.ScreenshotUrl).HasMaxLength(1000);
            entity.Property(x => x.VideoUrl).HasMaxLength(1000);
            entity.Property(x => x.VideoTitle).HasMaxLength(300);
        });

        builder.Entity<Office>(entity =>
        {
            entity.HasIndex(x => x.SortOrder);
            entity.Property(x => x.City).HasMaxLength(120);
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.Phone).HasMaxLength(64);
            entity.Property(x => x.Phone2).HasMaxLength(64);
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
        });

        builder.Entity<SiteSetting>(entity =>
        {
            entity.HasKey(x => x.Key);
            entity.Property(x => x.Key).HasMaxLength(160);
            entity.Property(x => x.Value).HasMaxLength(4000);
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        builder.Entity<Lead>(entity =>
        {
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.IsProcessed);
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.Phone).HasMaxLength(64);
            entity.Property(x => x.Message).HasMaxLength(2000);
            entity.Property(x => x.Source).HasMaxLength(80);
            entity.Property(x => x.ManagerComment).HasMaxLength(2000);
        });

        builder.Entity<FaqItem>(entity =>
        {
            entity.HasIndex(x => new { x.IsVisible, x.SortOrder });
            entity.Property(x => x.Question).HasMaxLength(500);
            entity.Property(x => x.Answer).HasMaxLength(4000);
        });

        builder.Entity<ContentSection>(entity =>
        {
            entity.HasIndex(x => new { x.Page, x.Slug }).IsUnique();
            entity.Property(x => x.Page).HasMaxLength(80);
            entity.Property(x => x.Slug).HasMaxLength(120);
            entity.Property(x => x.Title).HasMaxLength(300);
            entity.Property(x => x.Subtitle).HasMaxLength(1000);
        });

        builder.Entity<ContentSectionItem>(entity =>
        {
            entity.HasIndex(x => new { x.Section, x.SortOrder });
            entity.Property(x => x.Section).HasMaxLength(80);
            entity.Property(x => x.Title).HasMaxLength(300);
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.ImageUrl).HasMaxLength(1000);
            entity.Property(x => x.IconName).HasMaxLength(80);
            entity.Property(x => x.Url).HasMaxLength(1000);
        });
    }
}

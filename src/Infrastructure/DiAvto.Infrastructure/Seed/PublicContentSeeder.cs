using System.Text.Json;
using DiAvto.Domain;
using DiAvto.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Infrastructure.Seed;

public static class PublicContentSeeder
{
    public static async Task SeedAsync(DiAvtoDbContext db, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Seed", "showcase-seed.json");
        if (!File.Exists(path))
        {
            return;
        }

        await using var stream = File.OpenRead(path);
        var seed = await JsonSerializer.DeserializeAsync<ShowcaseSeed>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);

        if (seed is null)
        {
            return;
        }

        if (!await db.ShowcaseCars.AnyAsync(cancellationToken))
        {
            var order = 0;
            foreach (var car in seed.Cars)
            {
                var entity = new ShowcaseCar
                {
                    ExternalId = car.ExternalId,
                    Name = car.Name,
                    Make = car.Make,
                    Year = car.Year,
                    PriceRub = car.PriceRub,
                    Country = car.Country,
                    Fuel = car.Fuel,
                    BodyType = car.BodyType,
                    Transmission = car.Transmission,
                    Drive = car.Drive,
                    Engine = car.Engine,
                    MileageKm = car.MileageKm,
                    Description = car.Description,
                    InStock = car.InStock,
                    IsPublished = true,
                    SortOrder = car.SortOrder <= 0 ? ++order : car.SortOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var photoOrder = 0;
                foreach (var url in car.Images)
                {
                    entity.Photos.Add(new ShowcaseCarPhoto { Url = url, SortOrder = photoOrder++ });
                }

                db.ShowcaseCars.Add(entity);
            }
        }

        if (!await db.Reviews.AnyAsync(cancellationToken))
        {
            foreach (var review in seed.Reviews)
            {
                db.Reviews.Add(new Review
                {
                    Type = review.Type,
                    AuthorName = NullIfEmpty(review.AuthorName),
                    City = NullIfEmpty(review.City),
                    Text = NullIfEmpty(review.Text),
                    Rating = review.Rating,
                    Source = NullIfEmpty(review.Source),
                    ScreenshotUrl = NullIfEmpty(review.ScreenshotUrl),
                    VideoUrl = NullIfEmpty(review.VideoUrl),
                    VideoTitle = NullIfEmpty(review.VideoTitle),
                    IsVisible = review.IsVisible,
                    SortOrder = review.SortOrder,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (!await db.Offices.AnyAsync(cancellationToken))
        {
            foreach (var office in seed.Offices)
            {
                var (lat, lng) = office.City switch
                {
                    "Хабаровск" => (48.4894m, 135.1096m),
                    "Санкт-Петербург" => (59.9453m, 30.2531m),
                    "Красноярск" => (55.9934m, 92.9086m),
                    _ => ((decimal?)null, (decimal?)null)
                };

                db.Offices.Add(new Office
                {
                    City = office.City,
                    Address = office.Address,
                    Phone = NullIfEmpty(office.Phone),
                    Phone2 = NullIfEmpty(office.Phone2),
                    Latitude = lat,
                    Longitude = lng,
                    SortOrder = office.SortOrder,
                    IsActive = true
                });
            }
        }

        if (!await db.SiteSettings.AnyAsync(cancellationToken))
        {
            foreach (var setting in seed.SiteSettings)
            {
                db.SiteSettings.Add(new SiteSetting { Key = setting.Key, Value = setting.Value });
            }
        }

        if (!await db.FaqItems.AnyAsync(cancellationToken))
        {
            foreach (var faq in seed.Faq)
            {
                db.FaqItems.Add(new FaqItem
                {
                    Question = faq.Question,
                    Answer = faq.Answer,
                    SortOrder = faq.SortOrder,
                    IsVisible = true
                });
            }
        }

        if (!await db.ContentSections.AnyAsync(cancellationToken))
        {
            foreach (var section in seed.Sections)
            {
                db.ContentSections.Add(new ContentSection
                {
                    Page = section.Page,
                    Slug = section.Slug,
                    Title = section.Title,
                    Subtitle = NullIfEmpty(section.Subtitle),
                    SortOrder = section.SortOrder,
                    IsVisible = section.IsVisible
                });
            }
        }

        if (!await db.ContentSectionItems.AnyAsync(cancellationToken))
        {
            foreach (var item in seed.SectionItems)
            {
                db.ContentSectionItems.Add(new ContentSectionItem
                {
                    Section = item.Section,
                    Title = item.Title,
                    Description = NullIfEmpty(item.Description),
                    ImageUrl = NullIfEmpty(item.ImageUrl),
                    IconName = NullIfEmpty(item.IconName),
                    Url = NullIfEmpty(item.Url),
                    SortOrder = item.SortOrder,
                    IsVisible = item.IsVisible
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed class ShowcaseSeed
    {
        public List<SeedCar> Cars { get; set; } = [];
        public List<SeedReview> Reviews { get; set; } = [];
        public List<SeedOffice> Offices { get; set; } = [];
        public List<SeedSetting> SiteSettings { get; set; } = [];
        public List<SeedFaq> Faq { get; set; } = [];
        public List<SeedSection> Sections { get; set; } = [];
        public List<SeedSectionItem> SectionItems { get; set; } = [];
    }

    private sealed class SeedCar
    {
        public string? ExternalId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal PriceRub { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Fuel { get; set; } = string.Empty;
        public string BodyType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public string Drive { get; set; } = string.Empty;
        public string Engine { get; set; } = string.Empty;
        public int MileageKm { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool InStock { get; set; }
        public int SortOrder { get; set; }
        public List<string> Images { get; set; } = [];
    }

    private sealed class SeedReview
    {
        public string Type { get; set; } = "text";
        public string? AuthorName { get; set; }
        public string? City { get; set; }
        public string? Text { get; set; }
        public int? Rating { get; set; }
        public string? Source { get; set; }
        public string? ScreenshotUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? VideoTitle { get; set; }
        public bool IsVisible { get; set; } = true;
        public int SortOrder { get; set; }
    }

    private sealed class SeedOffice
    {
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Phone2 { get; set; }
        public int SortOrder { get; set; }
    }

    private sealed class SeedSetting
    {
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
    }

    private sealed class SeedFaq
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    private sealed class SeedSection
    {
        public string Page { get; set; } = "index";
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; }
    }

    private sealed class SeedSectionItem
    {
        public string Section { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? IconName { get; set; }
        public string? Url { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; }
    }
}

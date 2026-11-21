using System.Globalization;
using DiAvto.Admin.Data;
using DiAvto.Domain;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Admin.Features.Settings;

public sealed class AppSettingsService
{
    private readonly DiAvtoDbContext _db;

    public AppSettingsService(DiAvtoDbContext db)
    {
        _db = db;
    }

    public async Task<string> GetAsync(string key, string fallback = "", CancellationToken cancellationToken = default)
    {
        var setting = await _db.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        return setting?.Value ?? fallback;
    }

    public async Task<decimal> GetDecimalAsync(string key, decimal fallback, CancellationToken cancellationToken = default)
    {
        var raw = await GetAsync(key, string.Empty, cancellationToken);
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }

    public async Task<bool> GetBoolAsync(string key, bool fallback, CancellationToken cancellationToken = default)
    {
        var raw = await GetAsync(key, string.Empty, cancellationToken);
        return bool.TryParse(raw, out var value) ? value : fallback;
    }

    public async Task<int> GetIntAsync(string key, int fallback, CancellationToken cancellationToken = default)
    {
        var raw = await GetAsync(key, string.Empty, cancellationToken);
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }

    public async Task SetAsync(string key, string value, string? description = null, CancellationToken cancellationToken = default)
    {
        var setting = await _db.AppSettings.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (setting is null)
        {
            setting = new AppSetting { Key = key, Description = description };
            _db.AppSettings.Add(setting);
        }

        setting.Value = value;
        if (!string.IsNullOrWhiteSpace(description))
        {
            setting.Description = description;
        }

        setting.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AppSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.AppSettings
            .AsNoTracking()
            .OrderBy(x => x.Key)
            .ToListAsync(cancellationToken);
    }

    public static IReadOnlyList<AppSetting> Defaults =>
    [
        new() { Key = "SiteName", Value = "DiAvto", Description = "Название сайта" },
        new() { Key = "SiteTagline", Value = "Автомобили из Китая — без посредников", Description = "Подзаголовок на главной" },
        new() { Key = "PriceMarkupPercent", Value = "0", Description = "Наценка к курсу ЦБ при пересчёте цены, %" },
        new() { Key = "ShowSourceAttribution", Value = "true", Description = "Показывать подпись источника под карточками" },
        new() { Key = "SearchResultsPerTier", Value = "3", Description = "Сколько самых дорогих и самых дешёвых авто показывать" },
        new() { Key = "Che168DetailUrlTemplate", Value = "https://m.che168.com/cardetail/index?infoid={infoid}", Description = "Шаблон ссылки на карточку che168" },
        new() { Key = "PublicBaseUrl", Value = "https://di-auto.ru", Description = "Публичный адрес сайта для ссылок клиентам" },
        new() { Key = "ClientCardContactText", Value = "", Description = "Контакты менеджера на карточке клиента" },
        new() { Key = "ClientCardDefaultCountry", Value = "Китай", Description = "Страна нахождения автомобиля в карточке клиента" }
    ];
}


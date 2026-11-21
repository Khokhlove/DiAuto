using DiAvto.Domain;
using DiAvto.Admin.Features.Catalog;
using DiAvto.Admin.Features.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DiAvto.Admin.Data.Seed;

public static class DbSeeder
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<DiAvtoDbContext>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var options = provider.GetRequiredService<IOptions<SeedOptions>>().Value;

        await db.Database.MigrateAsync();
        logger.LogInformation("Миграции базы данных применены.");

        await SeedRolesAsync(provider);
        await SeedAdminAsync(provider, options, logger);
        await SeedSourcesAsync(db, options);
        await SeedSettingsAsync(db);
        await SeedTariffsAsync(db);
        await LocalizeSeriesNamesAsync(db, logger);
    }

    private static async Task SeedRolesAsync(IServiceProvider provider)
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "Admin", "Manager" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private const string LegacyAdminEmail = "admin@chinacars.local";

    private static async Task SeedAdminAsync(
        IServiceProvider provider,
        SeedOptions options,
        ILogger logger)
    {
        if (!options.Enabled)
        {
            return;
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByNameAsync(options.AdminUserName)
                   ?? await userManager.FindByEmailAsync(options.AdminEmail)
                   ?? await userManager.FindByEmailAsync(LegacyAdminEmail);

        var isLegacyAccount = user is not null
                              && !string.Equals(user.Email, options.AdminEmail, StringComparison.OrdinalIgnoreCase);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = options.AdminUserName,
                Email = options.AdminEmail,
                EmailConfirmed = true,
                DisplayName = options.AdminDisplayName
            };

            var result = await userManager.CreateAsync(user, options.AdminPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Не удалось создать администратора: {Errors}",
                    string.Join("; ", result.Errors.Select(x => x.Description)));
                return;
            }

            logger.LogWarning(
                "Создан администратор {Login}. Смените пароль после первого входа!", options.AdminUserName);
        }
        else
        {
            var changed = false;

            if (!string.Equals(user.UserName, options.AdminUserName, StringComparison.OrdinalIgnoreCase))
            {
                user.UserName = options.AdminUserName;
                changed = true;
            }

            if (!string.Equals(user.Email, options.AdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = options.AdminEmail;
                changed = true;
            }

            if (changed)
            {
                var update = await userManager.UpdateAsync(user);
                if (!update.Succeeded)
                {
                    logger.LogError("Не удалось обновить администратора: {Errors}",
                        string.Join("; ", update.Errors.Select(x => x.Description)));
                }
            }

            if (isLegacyAccount)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var reset = await userManager.ResetPasswordAsync(user, token, options.AdminPassword);
                if (!reset.Succeeded)
                {
                    logger.LogError("Не удалось обновить пароль администратора: {Errors}",
                        string.Join("; ", reset.Errors.Select(x => x.Description)));
                }
            }
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }

    private static async Task SeedSourcesAsync(DiAvtoDbContext db, SeedOptions options)
    {
        var changed = false;

        var che168 = await db.SourceSites.FirstOrDefaultAsync(x => x.Key == "che168");

        if (che168 is null)
        {
            db.SourceSites.Add(new SourceSite
            {
                Key = "che168",
                Name = "Che168 · Автомобили с пробегом",
                BaseUrl = "https://www.che168.com",
                DetailUrlTemplate = options.Che168DetailUrlTemplate,
                AttributionText = "Данные и фотографии: che168.com (Autohome). Переход по ссылке источника обязателен.",
                IsEnabled = true,
                Notes = "Интеграция с мобильным API che168. Данные носят справочный характер, актуальную цену уточняйте у продавца."
            });
            changed = true;
        }
        else if (che168.Name.Contains("二手车之家", StringComparison.Ordinal))
        {
            che168.Name = "Che168 · Автомобили с пробегом";
            changed = true;
        }

        if (!await db.SourceSites.AnyAsync(x => x.Key == "source-2"))
        {
            db.SourceSites.Add(new SourceSite
            {
                Key = "source-2",
                Name = "Второй сайт",
                BaseUrl = "",
                DetailUrlTemplate = null,
                AttributionText = "Второй источник пока не подключён.",
                IsEnabled = false,
                Notes = "Заготовка для второго сайта. Реализуйте IMarketplaceClient, зарегистрируйте его в DI, заполните URL и включите источник."
            });
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedSettingsAsync(DiAvtoDbContext db)
    {
        var existingKeys = await db.AppSettings.Select(x => x.Key).ToListAsync();
        var existing = existingKeys.ToHashSet(StringComparer.Ordinal);

        foreach (var setting in AppSettingsService.Defaults)
        {
            if (existing.Add(setting.Key))
            {
                db.AppSettings.Add(new AppSetting
                {
                    Key = setting.Key,
                    Value = setting.Value,
                    Description = setting.Description,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Старые установки могли сохранить пустой адрес. Не перезаписываем
        // явно заданное значение, но заполняем пустое публичным доменом.
        var publicBaseUrl = await db.AppSettings.FirstOrDefaultAsync(x => x.Key == "PublicBaseUrl");
        if (publicBaseUrl is not null && string.IsNullOrWhiteSpace(publicBaseUrl.Value))
        {
            publicBaseUrl.Value = "https://di-auto.ru";
            publicBaseUrl.Description = "Публичный адрес сайта для ссылок клиентам";
            publicBaseUrl.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedTariffsAsync(DiAvtoDbContext db)
    {
        if (await db.ServiceTariffs.AnyAsync())
        {
            // В уже работающей базе tariffs появились раньше, чем растаможка.
            // Добавляем строку, чтобы новая настройка была доступна без ручного создания.
            if (!await db.ServiceTariffs.AnyAsync(x => x.Kind == ServiceTariffKind.CustomsClearance))
            {
                db.ServiceTariffs.Add(new ServiceTariff
                {
                    Kind = ServiceTariffKind.CustomsClearance,
                    Title = "Китай (базовый)",
                    AmountRub = 0,
                    IsDefault = true,
                    IsActive = true,
                    SortOrder = 10,
                    Notes = "Заполните стоимость таможенного оформления автомобиля."
                });

                await db.SaveChangesAsync();
            }

            return;
        }

        // Стартовые строки с нулевой стоимостью: администратор заполняет
        // реальные суммы в разделе «Тарифы». Нулевые значения подсвечиваются
        // в интерфейсе как незаполненные.
        db.ServiceTariffs.AddRange(
            new ServiceTariff
            {
                Kind = ServiceTariffKind.DeliveryToVladivostok,
                Title = "Китай (базовый)",
                AmountRub = 0,
                IsDefault = true,
                IsActive = true,
                SortOrder = 10,
                Notes = "Заполните стоимость доставки из региона покупки до Владивостока."
            },
            new ServiceTariff
            {
                Kind = ServiceTariffKind.DeliveryFromVladivostok,
                Title = "Владивосток",
                AmountRub = 0,
                IsDefault = true,
                IsActive = true,
                SortOrder = 10,
                Notes = "Выдача автомобиля во Владивостоке. Если доставка по городу не нужна, оставьте 0."
            },
            new ServiceTariff
            {
                Kind = ServiceTariffKind.DeliveryFromVladivostok,
                Title = "Хабаровск",
                AmountRub = 0,
                IsActive = true,
                SortOrder = 20
            },
            new ServiceTariff
            {
                Kind = ServiceTariffKind.DeliveryFromVladivostok,
                Title = "Москва",
                AmountRub = 0,
                IsActive = true,
                SortOrder = 30
            },
            new ServiceTariff
            {
                Kind = ServiceTariffKind.Inspection,
                Title = "Китай (базовый)",
                AmountRub = 0,
                IsDefault = true,
                IsActive = true,
                SortOrder = 10,
                Notes = "Заполните стоимость осмотра автомобиля в регионе покупки."
            },
            new ServiceTariff
            {
                Kind = ServiceTariffKind.CustomsClearance,
                Title = "Китай (базовый)",
                AmountRub = 0,
                IsDefault = true,
                IsActive = true,
                SortOrder = 10,
                Notes = "Заполните стоимость таможенного оформления автомобиля."
            });

        await db.SaveChangesAsync();
    }

    private static async Task LocalizeSeriesNamesAsync(DiAvtoDbContext db, ILogger logger)
    {
        var series = await db.CarSeries
            .Where(x => x.NameRu == null)
            .ToListAsync();

        var updated = 0;

        foreach (var item in series)
        {
            var localized = ModelLocalization.GetName(item.Name);
            if (!string.IsNullOrWhiteSpace(localized))
            {
                item.NameRu = localized;
                updated++;
            }
        }

        if (updated > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Локализовано названий моделей: {Count}.", updated);
        }
    }
}


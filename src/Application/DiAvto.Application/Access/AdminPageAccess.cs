using System.Security.Claims;

namespace DiAvto.Application.Access;

public sealed record AdminPageDefinition(string Key, string Title);

public static class AdminPageAccess
{
    public const string ClaimType = "admin_page";
    public const string PolicyPrefix = "AdminPage.";
    public const string PanelPolicy = "AdminPanel";

    public static IReadOnlyList<AdminPageDefinition> Pages { get; } =
    [
        new("dashboard", "Дашборд"),
        new("searches", "Поиски"),
        new("listings", "Автомобили"),
        new("cards", "Карточки клиента"),
        new("showcase", "Витрина автомобилей"),
        new("reviews", "Отзывы"),
        new("offices", "Офисы"),
        new("leads", "Заявки"),
        new("sitecontent", "Контент сайта"),
        new("tariffs", "Тарифы доставки и осмотра"),
        new("sources", "Источники"),
        new("catalog", "Каталог"),
        new("rates", "Курс валют"),
        new("users", "Пользователи"),
        new("audit", "Журнал"),
        new("integration", "Интеграция")
    ];

    public static bool HasPage(ClaimsPrincipal user, string pageKey)
    {
        return user.IsInRole("Admin") || user.HasClaim(ClaimType, pageKey);
    }

    public static bool HasAnyPage(ClaimsPrincipal user)
    {
        return user.IsInRole("Admin") || user.HasClaim(claim => claim.Type == ClaimType);
    }

    public static string Policy(string pageKey) => PolicyPrefix + pageKey;
}



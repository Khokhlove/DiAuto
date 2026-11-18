using System.Globalization;
using System.Net.Http.Headers;
using DiAvto.Web.Components;
using DiAvto.Web.Components.Account;
using DiAvto.Web.Data;
using DiAvto.Web.Data.Seed;
using DiAvto.Web.Features.Access;
using DiAvto.Web.Features.Audit;
using DiAvto.Web.Features.Cards;
using DiAvto.Web.Features.Catalog;
using DiAvto.Web.Features.Currency;
using DiAvto.Web.Features.Listings;
using DiAvto.Web.Features.Marketplaces;
using DiAvto.Web.Features.Marketplaces.Che168;
using DiAvto.Web.Features.Pricing;
using DiAvto.Web.Features.Search;
using DiAvto.Web.Features.Settings;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var culture = CultureInfo.GetCultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? "Data Source=Data/diavto.db;Cache=Shared";
var sqlite = new SqliteConnectionStringBuilder(rawConnectionString);
if (!string.IsNullOrWhiteSpace(sqlite.DataSource) && !Path.IsPathRooted(sqlite.DataSource))
{
    sqlite.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqlite.DataSource);
}
var connectionString = sqlite.ConnectionString;
var dataDirectory = Path.GetDirectoryName(sqlite.DataSource);
if (!string.IsNullOrWhiteSpace(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = false;
        options.Password.RequiredLength = 5;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 8;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
    .AddErrorDescriber<RussianIdentityErrorDescriber>();

var authorization = builder.Services.AddAuthorizationBuilder();
authorization.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
authorization.AddPolicy(AdminPageAccess.PanelPolicy, policy =>
    policy.RequireAssertion(context => AdminPageAccess.HasAnyPage(context.User)));

foreach (var page in AdminPageAccess.Pages)
{
    authorization.AddPolicy(AdminPageAccess.Policy(page.Key), policy =>
        policy.RequireAssertion(context => AdminPageAccess.HasPage(context.User, page.Key)));
}

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.Configure<Che168Options>(builder.Configuration.GetSection(Che168Options.SectionName));
builder.Services.Configure<CurrencyOptions>(builder.Configuration.GetSection(CurrencyOptions.SectionName));
builder.Services.Configure<SearchOptions>(builder.Configuration.GetSection(SearchOptions.SectionName));
builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(SeedOptions.SectionName));

builder.Services.AddHttpClient("che168", (sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Che168Options>>().Value;
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 10, 120));
    client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
    client.DefaultRequestHeaders.Referrer = new Uri(options.Referer);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en;q=0.6");
    client.DefaultRequestHeaders.Add("Origin", "https://m.che168.com");
});

builder.Services.AddHttpClient("cbr", client =>
{
    client.Timeout = TimeSpan.FromSeconds(25);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("DiAvto/1.0 (+https://localhost)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/xml,text/xml,*/*");
});

builder.Services.AddScoped<Che168ApiClient>();
builder.Services.AddScoped<IMarketplaceClient>(sp => sp.GetRequiredService<Che168ApiClient>());
builder.Services.AddScoped<IMarketplaceClientFactory, MarketplaceClientFactory>();

builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<CbrCurrencyClient>();
builder.Services.AddScoped<CurrencyService>();
builder.Services.AddScoped<SearchExecutionService>();
builder.Services.AddScoped<ListingService>();
builder.Services.AddScoped<ClientCardService>();
builder.Services.AddScoped<ServiceTariffService>();
builder.Services.AddScoped<AppSettingsService>();
builder.Services.AddScoped<AuditService>();

builder.Services.AddHostedService<CurrencyRateUpdater>();
builder.Services.AddHostedService<CatalogWarmupService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    if (builder.Configuration.GetValue("Https:Enabled", true))
    {
        app.UseHsts();
    }
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.Use(async (context, next) =>
{
    // Images are hotlinked from the source CDN; it rejects requests with a localhost referer.
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});
if (!app.Environment.IsDevelopment() && builder.Configuration.GetValue("Https:Enabled", true))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.MapGet("/healthz", async (ApplicationDbContext db, CurrencyService currency, CancellationToken cancellationToken) =>
{
    var rate = await currency.GetCurrentAsync(cancellationToken: cancellationToken);
    return Results.Ok(new
    {
        status = "ok",
        utc = DateTimeOffset.UtcNow,
        database = await db.Database.CanConnectAsync(cancellationToken),
        cnyRub = rate.Rate,
        rateSource = rate.Source,
        rateDate = rate.EffectiveDate
    });
}).AllowAnonymous();

await DbSeeder.InitializeAsync(app.Services);

app.Run();

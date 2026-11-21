using DiAvto.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDiAvtoInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    if (builder.Configuration.GetValue("Https:Enabled", true))
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }
}

app.Use(async (context, next) =>
{
    // Фото автомобилей из che168 отдаются по внешним ссылкам и могут
    // отклонять запросы с чужим Referer.
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/health", () => Results.Text("OK"));
app.MapRazorComponents<DiAvto.Site.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

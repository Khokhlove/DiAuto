namespace DiAvto.Web.Features.Catalog;

public sealed class CatalogWarmupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CatalogWarmupService> _logger;

    public CatalogWarmupService(
        IServiceScopeFactory scopeFactory,
        ILogger<CatalogWarmupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(8), stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            var catalog = scope.ServiceProvider.GetRequiredService<CatalogService>();
            await catalog.EnsureCatalogAsync(stoppingToken);
            _logger.LogInformation("Каталог брендов прогрет.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Штатная остановка приложения: не считаем отмену ошибкой.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось прогреть каталог брендов.");
        }
    }
}

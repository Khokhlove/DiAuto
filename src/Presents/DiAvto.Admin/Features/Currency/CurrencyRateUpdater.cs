using Microsoft.Extensions.Options;

namespace DiAvto.Admin.Features.Currency;

public sealed class CurrencyRateUpdater : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CurrencyOptions _options;
    private readonly ILogger<CurrencyRateUpdater> _logger;

    public CurrencyRateUpdater(
        IServiceScopeFactory scopeFactory,
        IOptions<CurrencyOptions> options,
        ILogger<CurrencyRateUpdater> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(4), stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromHours(Math.Max(1, _options.RefreshIntervalHours)));

            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var currency = scope.ServiceProvider.GetRequiredService<CurrencyService>();
                    await currency.RefreshAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Фоновое обновление курса ЦБ РФ не удалось.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Штатная остановка приложения.
        }
    }
}


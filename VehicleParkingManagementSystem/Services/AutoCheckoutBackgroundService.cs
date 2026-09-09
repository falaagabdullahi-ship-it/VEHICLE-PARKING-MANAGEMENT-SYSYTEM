using VehicleParkingManagementSystem.Services.Interfaces;

namespace VehicleParkingManagementSystem.Services;

/// <summary>Periodically checks out any parking session whose prepaid package or reservation window has expired.</summary>
public class AutoCheckoutBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoCheckoutBackgroundService> _logger;

    public AutoCheckoutBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AutoCheckoutBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var operations = scope.ServiceProvider.GetRequiredService<IParkingOperationService>();
                await operations.ProcessExpiredPaidSessionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-checkout sweep failed.");
            }
        }
    }
}

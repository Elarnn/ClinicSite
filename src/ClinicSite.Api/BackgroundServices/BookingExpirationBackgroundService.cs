using ClinicSite.Application.Interfaces;

namespace ClinicSite.Api.BackgroundServices;

/// <summary>
/// Periodically expires bookings whose confirmation window has elapsed and frees their slots.
/// Runs about once a minute. A DbContext is resolved per sweep from a fresh scope. One failing
/// booking never aborts the sweep, and a failed sweep never kills the loop.
/// </summary>
public class BookingExpirationBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingExpirationBackgroundService> _logger;

    public BookingExpirationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingExpirationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking expiration sweep started (interval: {Interval}).", Interval);

        using var timer = new PeriodicTimer(Interval);

        try
        {
            do
            {
                await RunSweepAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }

        _logger.LogInformation("Booking expiration sweep stopped.");
    }

    private async Task RunSweepAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            var expired = await bookingService.ExpirePendingBookingsAsync(stoppingToken);
            if (expired > 0)
            {
                _logger.LogInformation("Expired {Count} unconfirmed booking(s) and freed their slots.", expired);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Never let a single bad sweep terminate the background service.
            _logger.LogError(ex, "Booking expiration sweep failed; will retry next interval.");
        }
    }
}

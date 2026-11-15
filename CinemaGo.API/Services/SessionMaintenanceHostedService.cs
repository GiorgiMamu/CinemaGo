using CinemaGo.Application.Interfaces.Services;
namespace CinemaGo.API.Services;

public class SessionMaintenanceHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<SessionMaintenanceHostedService> _logger;

    public SessionMaintenanceHostedService(IServiceScopeFactory scopes, ILogger<SessionMaintenanceHostedService> logger)
    { _scopes = scopes; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        do
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var count = await scope.ServiceProvider.GetRequiredService<ISessionService>().FinishPastSessionsAsync(stoppingToken);
                if (count > 0) _logger.LogInformation("{Count} session(s) marked as Finished", count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Session maintenance failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
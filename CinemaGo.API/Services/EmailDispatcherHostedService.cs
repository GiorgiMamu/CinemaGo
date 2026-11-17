using CinemaGo.Infrastructure.Notifications;
namespace CinemaGo.API.Services;

public class EmailDispatcherHostedService : BackgroundService
{
    private readonly EmailQueue _queue;
    private readonly IEmailTransport _transport;
    private readonly ILogger<EmailDispatcherHostedService> _logger;

    public EmailDispatcherHostedService(EmailQueue queue, IEmailTransport transport, ILogger<EmailDispatcherHostedService> logger)
    { _queue = queue; _transport = transport; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
            await SendWithRetryAsync(message, stoppingToken);
    }

    private async Task SendWithRetryAsync(EmailMessage m, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try { await _transport.SendAsync(m.To, m.Subject, m.Body, ct); return; }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "E-mail attempt {Attempt}/3 to {To} failed", attempt, m.To);
                if (attempt < 3) await Task.Delay(TimeSpan.FromSeconds(2 * attempt), ct);
            }
        }
        _logger.LogError("E-mail '{Subject}' to {To} could not be delivered", m.Subject, m.To);
    }
}
using CinemaGo.Domain.Interfaces.External;
using Microsoft.Extensions.Logging;
namespace CinemaGo.Infrastructure.Notifications;

public class QueuedEmailSender : IEmailSender
{
    private readonly EmailQueue _queue;
    private readonly ILogger<QueuedEmailSender> _logger;
    public QueuedEmailSender(EmailQueue queue, ILogger<QueuedEmailSender> logger) { _queue = queue; _logger = logger; }

    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        if (!_queue.TryEnqueue(new EmailMessage(to, subject, body)))
            _logger.LogError("E-mail queue is full. Dropped '{Subject}'", subject);
        return Task.CompletedTask;
    }
}
using CinemaGo.Domain.Interfaces.External;
using Microsoft.Extensions.Logging;
namespace CinemaGo.Infrastructure.Notifications;

public class FakeEmailSender : IEmailSender
{
    private readonly ILogger<FakeEmailSender> _logger;
    public FakeEmailSender(ILogger<FakeEmailSender> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("[SIMULATED EMAIL] To: {To} | Subject: {Subject} | Body: {Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
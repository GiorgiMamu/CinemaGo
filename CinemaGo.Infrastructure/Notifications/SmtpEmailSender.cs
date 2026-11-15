using System.Net;
using System.Net.Mail;
using CinemaGo.Domain.Interfaces.External;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace CinemaGo.Infrastructure.Notifications;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    { _settings = settings.Value; _logger = logger; }

    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress ?? _settings.Username!, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = true,   // STARTTLS on port 587
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };
        await client.SendMailAsync(message, ct);
        _logger.LogInformation("E-mail sent to {To}: {Subject}", to, subject);
    }
}
namespace CinemaGo.Infrastructure.Notifications;

public interface IEmailTransport
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
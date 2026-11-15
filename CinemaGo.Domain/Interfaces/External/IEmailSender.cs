namespace CinemaGo.Domain.Interfaces.External;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
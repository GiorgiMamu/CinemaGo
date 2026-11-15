namespace CinemaGo.Infrastructure.Notifications;

public class EmailSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string? Username { get; set; }        // Gmail address (user-secrets)
    public string? Password { get; set; }        // Gmail App Password (user-secrets)
    public string FromName { get; set; } = "CinemaGo";
    public string? FromAddress { get; set; }     // defaults to Username

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}
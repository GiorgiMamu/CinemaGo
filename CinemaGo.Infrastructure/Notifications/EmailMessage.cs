namespace CinemaGo.Infrastructure.Notifications;

public sealed record EmailMessage(string To, string Subject, string Body);
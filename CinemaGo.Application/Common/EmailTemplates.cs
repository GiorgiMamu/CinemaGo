using System.Globalization;
using System.Net;
using CinemaGo.Domain.Entities;
namespace CinemaGo.Application.Common;

public static class EmailTemplates
{
    private const string Brand = "#6d28d9";
    private const string Currency = "GEL";

    // ---------------- account ----------------
    public static (string Subject, string Body) Activation(string userName, string code)
        => ("Activate your CinemaGo account", Layout("Activate your account", "Your activation code",
            P($"Hi {Enc(userName)}, welcome to CinemaGo! Enter this code to activate your account:") + CodeBlock(code) +
            Muted("The code expires in a few minutes. If you did not create an account, ignore this e-mail.")));

    public static (string Subject, string Body) Welcome(string userName)
        => ("Welcome to CinemaGo", Layout("You're all set 🎉", "Your account is active",
            P($"Hi {Enc(userName)}, your account is active. You can now log in, pick a session and book your seats.")));

    public static (string Subject, string Body) PasswordReset(string userName, string code)
        => ("Your CinemaGo password reset code", Layout("Reset your password", "Your password reset code",
            P($"Hi {Enc(userName)}, use this code to choose a new password:") + CodeBlock(code) +
            Muted("The code expires in a few minutes. If you did not ask for it, ignore this e-mail; your password stays the same.")));

    public static (string Subject, string Body) PasswordChanged(string userName)
        => ("Your CinemaGo password was changed", Layout("Password changed", "Security notice",
            P($"Hi {Enc(userName)}, the password of your account was just changed. You were logged out on all devices.") +
            Muted("If this was not you, reset your password immediately.")));

    public static (string Subject, string Body) AccountDeleted(string userName, bool byStaff)
        => ("Your CinemaGo account was deleted", Layout("Account deleted", "Your account was deleted",
            P($"Hi {Enc(userName)}, your account was deleted" + (byStaff ? " by the cinema staff." : " as you requested.") +
              " Your upcoming bookings were cancelled and your personal data was removed.")));

    // ---------------- bookings ----------------
    public static (string Subject, string Body) BookingConfirmed(string userName, IReadOnlyList<Booking> bookings)
    {
        var session = bookings[0].Session;
        var total = bookings.Sum(b => b.Ticket!.FinalPrice);
        var content =
            P($"Hi {Enc(userName)}, your booking is confirmed. Show the ticket code at the entrance.") +
            SessionBox(session) +
            string.Concat(bookings.Select(TicketCard)) +
            $"<p style=\"text-align:right;font-size:16px;margin:16px 0 0;\">Total: <b>{Money(total)}</b></p>" +
            Muted("Check-in opens 60 minutes before the session. You can cancel until the session starts.");
        return ($"Your tickets for {session.Movie.Title}",
            Layout("Booking confirmed 🎟️", $"{bookings.Count} ticket(s) for {session.Movie.Title}", content));
    }

    public static (string Subject, string Body) BookingCancelled(string userName, Booking booking, bool byStaff)
    {
        var seat = $"Row {booking.Seat.RowNumber} · Seat {booking.Seat.SeatNumber}";
        var content =
            P($"Hi {Enc(userName)}, your booking was cancelled" + (byStaff ? " by the cinema staff." : ".")) +
            SessionBox(booking.Session) +
            P($"Seat: {Enc(seat)}" + (booking.Ticket is null ? "" : $"<br>Ticket <b>{Enc(booking.Ticket.TicketCode)}</b> is no longer valid.")) +
            (byStaff ? Muted("Please contact the cinema if you have questions.") : "");
        return ($"Booking cancelled: {booking.Session.Movie.Title}",
            Layout("Booking cancelled", "Your booking was cancelled", content));
    }

    // ---------------- sessions ----------------
    public static (string Subject, string Body) SessionCancelled(string userName, string movieTitle, DateTime startsAt, int ticketCount)
        => ($"Session cancelled: {movieTitle}", Layout("Session cancelled", "Your session was cancelled",
            P($"Hi {Enc(userName)}, we are sorry: the session of <b>{Enc(movieTitle)}</b> on {Enc(When(startsAt))} was cancelled.") +
            P($"Your {ticketCount} booking(s) and ticket(s) were cancelled automatically. You can book another session at any time.")));

    public static (string Subject, string Body) SessionRescheduled(string userName, string movieTitle, DateTime oldStart, DateTime newStart, int ticketCount)
        => ($"Session rescheduled: {movieTitle}", Layout("Session rescheduled", "Your session time changed",
            P($"Hi {Enc(userName)}, the session of <b>{Enc(movieTitle)}</b> was rescheduled.") +
            $"<table role=\"presentation\" width=\"100%\" style=\"background:#f5f3ff;border-radius:10px;margin:16px 0;\"><tr><td style=\"padding:16px;\">" +
            $"<div style=\"color:#6b7280;\">Before: <s>{Enc(When(oldStart))}</s></div>" +
            $"<div style=\"font-size:18px;font-weight:bold;color:{Brand};margin-top:4px;\">Now: {Enc(When(newStart))}</div></td></tr></table>" +
            P($"Your {ticketCount} booking(s) stay valid. If the new time does not suit you, you can cancel them in your bookings.")));

    // ---------------- building blocks ----------------
    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
    private static string Money(decimal value) => $"{value.ToString("0.00", CultureInfo.InvariantCulture)} {Currency}";
    private static string When(DateTime value) => value.ToString("dddd, d MMMM yyyy 'at' HH:mm 'UTC'", CultureInfo.InvariantCulture);

    private static string P(string html) => $"<p style=\"margin:0 0 14px;font-size:15px;line-height:1.6;color:#374151;\">{html}</p>";
    private static string Muted(string text) => $"<p style=\"margin:18px 0 0;font-size:12px;line-height:1.5;color:#6b7280;\">{Enc(text)}</p>";

    private static string CodeBlock(string code)
        => $"<div style=\"text-align:center;margin:20px 0;padding:18px;background:#f5f3ff;border-radius:10px;font-size:34px;font-weight:bold;letter-spacing:10px;font-family:Consolas,'Courier New',monospace;color:{Brand};\">{Enc(code)}</div>";

    private static string SessionBox(Session s)
        => "<table role=\"presentation\" width=\"100%\" style=\"background:#f5f3ff;border-radius:10px;margin:16px 0;\"><tr><td style=\"padding:16px;\">" +
           $"<div style=\"font-size:18px;font-weight:bold;color:#111827;\">{Enc(s.Movie.Title)}</div>" +
           $"<div style=\"color:#4b5563;margin-top:6px;\">{Enc(s.Hall.Name)} · {Enc(When(s.StartsAt))}</div></td></tr></table>";

    private static string TicketCard(Booking b)
    {
        var t = b.Ticket!;
        var seat = $"Row {b.Seat.RowNumber} · Seat {b.Seat.SeatNumber}" + (b.Seat.IsVip ? " · VIP" : "");
        return "<table role=\"presentation\" width=\"100%\" style=\"border:2px dashed #c4b5fd;border-radius:10px;margin:10px 0;\"><tr><td style=\"padding:16px;\">" +
               "<div style=\"font-size:11px;letter-spacing:1px;color:#6b7280;\">TICKET CODE</div>" +
               $"<div style=\"font-family:Consolas,'Courier New',monospace;font-size:22px;font-weight:bold;letter-spacing:2px;color:{Brand};\">{Enc(t.TicketCode)}</div>" +
               $"<div style=\"margin-top:8px;color:#374151;\">{Enc(seat)}</div>" +
               $"<div style=\"color:#374151;\">{Money(t.FinalPrice)}</div></td></tr></table>";
    }

    private static string Layout(string title, string preheader, string content) => $$"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
<body style="margin:0;padding:0;background:#f3f4f6;font-family:Arial,Helvetica,sans-serif;">
<div style="display:none;max-height:0;overflow:hidden;opacity:0;">{{Enc(preheader)}}</div>
<table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f3f4f6;padding:24px 12px;"><tr><td align="center">
<table role="presentation" width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;background:#ffffff;border-radius:12px;overflow:hidden;">
<tr><td style="background:{{Brand}};padding:22px 32px;color:#ffffff;font-size:22px;font-weight:bold;">🎬 CinemaGo</td></tr>
<tr><td style="padding:32px;"><h1 style="margin:0 0 18px;font-size:22px;color:#111827;">{{title}}</h1>{{content}}</td></tr>
<tr><td style="background:#f9fafb;padding:16px 32px;font-size:12px;color:#6b7280;">This is an automated message from CinemaGo. Please do not reply.</td></tr>
</table></td></tr></table></body></html>
""";
}
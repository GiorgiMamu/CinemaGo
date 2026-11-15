using System.Net;
namespace CinemaGo.Application.Common;

public static class EmailTemplates
{
    public static (string Subject, string Body) Activation(string userName, string code)
        => ("Activate your CinemaGo account", Wrap(userName, "Your activation code is", code));

    public static (string Subject, string Body) PasswordReset(string userName, string code)
        => ("Your CinemaGo password reset code", Wrap(userName, "Your password reset code is", code));

    private static string Wrap(string userName, string intro, string code)
        => $"<div style=\"font-family:Arial,sans-serif\"><p>Hi {WebUtility.HtmlEncode(userName)},</p>" +
           $"<p>{intro}</p><p style=\"font-size:28px;letter-spacing:6px\"><b>{code}</b></p>" +
           "<p>The code is valid for about 10 minutes. If you did not request it, ignore this e-mail.</p></div>";
}
namespace CinemaGo.Domain.Constants;

public static class SuccessMessages
{
    public const string Registered = "Registration completed successfully.";
    public const string LoggedIn = "Logged in successfully.";
    public const string BookingCreated = "Booking created successfully.";
    public const string BookingCancelled = "Booking cancelled successfully.";
    public const string SessionCancelled = "Session cancelled. Affected bookings were cancelled and customers notified.";
    public const string PosterUploaded = "Poster uploaded successfully.";
    public const string PosterDeleted = "Poster deleted successfully.";
    public const string Retrieved = "Data retrieved successfully.";

    public static string Created(string entity) => $"{entity} created successfully.";
    public static string Updated(string entity) => $"{entity} updated successfully.";
    public static string Deleted(string entity) => $"{entity} deleted successfully.";
    public const string RoleChanged = "User role updated successfully.";
    public const string StatusChanged = "User status updated successfully.";
    public const string RegisteredCheckEmail = "Registration successful. We sent an activation code to your email.";
    public const string AccountActivated = "Account activated. You can log in now.";
    public const string LoggedOut = "Logged out successfully.";
    public const string CodeSentIfExists = "If the account exists, a code was sent to the email address.";
    public const string PasswordReset = "Password reset successfully. You can log in with the new password.";
    public const string PasswordChanged = "Password changed. Please log in again.";
    public const string AccountDeleted = "Your account was deleted.";
    public const string TicketCheckedIn = "Ticket checked in. Enjoy the movie!";
}
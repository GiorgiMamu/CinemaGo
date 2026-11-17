namespace CinemaGo.Domain.Constants;

// A failed operation: a stable machine-readable Code (for the frontend) and a human-readable Message.
public sealed record Error(string Code, string Message);

// THE single source of truth for failure codes and messages. Code format: "<Area>.<Name>".
public static class Errors
{
    public static class General
    {
        public static readonly Error Unexpected = new("General.Unexpected", "An unexpected error occurred. Please try again later.");
        public static readonly Error ValidationFailed = new("General.ValidationFailed", "Validation errors occurred.");
        public static readonly Error Unauthorized = new("General.Unauthorized", "Authentication is required to access this resource.");
        public static readonly Error Forbidden = new("General.Forbidden", "You do not have permission to perform this action.");
        public static readonly Error TooManyRequests = new("General.TooManyRequests", "Too many requests. Please wait a moment and try again.");
        public static readonly Error DuplicateValue = new("General.DuplicateValue", "A record with the same unique value already exists.");
        public static readonly Error HasDependencies = new("General.HasDependencies", "This record cannot be deleted because other records depend on it.");

        public static Error NotFound(string entity, object key) => new("General.NotFound", $"{entity} with id '{key}' was not found.");
        public static Error AlreadyExists(string entity, string value) => new("General.AlreadyExists", $"{entity} '{value}' already exists.");
    }

    public static class Auth
    {
        public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid email or password.");
        public static readonly Error AccountDisabled = new("Auth.AccountDisabled", "This account is disabled.");
        public static readonly Error AccountNotActivated = new("Auth.AccountNotActivated", "Your account is not activated. Enter the code we sent to your email, or request a new one.");
        public static readonly Error AccountLocked = new("Auth.AccountLocked", "Too many failed attempts. The account is locked for 15 minutes.");
        public static readonly Error InvalidCode = new("Auth.InvalidCode", "The code is invalid or has expired.");
        public static readonly Error EmailTaken = new("Auth.EmailTaken", "A user with this email already exists.");
        public static readonly Error UserNameTaken = new("Auth.UserNameTaken", "This user name is already taken.");
    }

    public static class User
    {
        public static readonly Error SuperAdminProtected = new("User.SuperAdminProtected", "The SuperAdmin account cannot be modified.");
        public static readonly Error AdminCannotManageStaff = new("User.AdminCannotManageStaff", "Admins can manage only customer accounts. Ask a SuperAdmin.");
        public static readonly Error CannotDeleteViaAdmin = new("User.CannotDeleteViaAdmin", "To delete your own account use DELETE /api/auth/account.");
    }

    public static class Seat
    {
        public static readonly Error PositionTaken = new("Seat.PositionTaken", "A seat with this row and number already exists in the hall.");
    }

    public static class Session
    {
        public static readonly Error NotBookable = new("Session.NotBookable", "This session is cancelled or has already started.");
        public static readonly Error Overlap = new("Session.Overlap", "Another session is already scheduled in this hall at the selected time.");
        public static readonly Error AlreadyCancelled = new("Session.AlreadyCancelled", "This session is already cancelled.");
        public static readonly Error CancelledNoEdit = new("Session.CancelledNoEdit", "A cancelled session cannot be modified.");
        public static readonly Error HallLocked = new("Session.HallLocked", "The hall cannot be changed because the session already has active bookings.");
    }

    public static class Booking
    {
        public static readonly Error SeatTaken = new("Booking.SeatTaken", "One or more selected seats are already booked for this session.");
        public static readonly Error SeatNotInHall = new("Booking.SeatNotInHall", "One or more selected seats do not belong to the hall of this session.");
        public static readonly Error AlreadyCancelled = new("Booking.AlreadyCancelled", "This booking is already cancelled.");
        public static readonly Error NotCancellable = new("Booking.NotCancellable", "A booking cannot be cancelled after the session has started.");
    }

    public static class Ticket
    {
        public static readonly Error Cancelled = new("Ticket.Cancelled", "This ticket is cancelled.");
        public static readonly Error AlreadyUsed = new("Ticket.AlreadyUsed", "This ticket has already been used.");
        public static readonly Error NotValidNow = new("Ticket.NotValidNow", "A ticket can be checked in from 60 minutes before the session until it ends.");
    }
    public static class Genre
    {
        public static readonly Error InUse = new("Genre.InUse", "This genre is used by one or more movies. Remove it from those movies first.");
    }
}
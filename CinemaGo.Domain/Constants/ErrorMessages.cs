namespace CinemaGo.Domain.Constants;

public static class ErrorMessages
{
    public const string Unexpected = "An unexpected error occurred. Please try again later.";
    public const string ValidationFailed = "One or more validation errors occurred.";
    public const string DuplicateValue = "A record with the same unique value already exists.";
    public const string Forbidden = "You do not have permission to perform this action.";
    public const string Unauthorized = "Authentication is required to access this resource.";

    public const string InvalidCredentials = "Invalid email or password.";
    public const string AccountDisabled = "This account is disabled.";
    public const string EmailTaken = "A user with this email already exists.";
    public const string UserNameTaken = "This user name is already taken.";
    public const string SuperAdminProtected = "The SuperAdmin account cannot be modified.";

    public const string SeatAlreadyBooked = "One or more selected seats are already booked for this session.";
    public const string SeatNotInHall = "One or more selected seats do not belong to the hall of this session.";
    public const string SessionNotBookable = "This session is cancelled or has already started.";
    public const string SessionOverlap = "Another session is already scheduled in this hall at the selected time.";
    public const string BookingAlreadyCancelled = "This booking is already cancelled.";
    public const string BookingNotCancellable = "A booking cannot be cancelled after the session has started.";

    public const string InvalidFileType = "Only JPG, PNG or WEBP images are allowed.";
    public const string FileTooLarge = "The file exceeds the maximum allowed size.";
    public const string EmptyFile = "The uploaded file is empty.";
    public const string HasDependencies = "This record cannot be deleted because other records depend on it.";

    public static string NotFound(string entity, object key) => $"{entity} with id '{key}' was not found.";
    public const string SessionAlreadyCancelled = "This session is already cancelled.";
    public const string SessionCancelledNoEdit = "A cancelled session cannot be modified.";
    public const string SessionHallLocked = "The hall cannot be changed because the session already has active bookings.";
    public const string InvalidRole = "Only the Admin and Customer roles can be assigned.";
    public const string SeatPositionTaken = "A seat with this row and number already exists in the hall.";

    public static string AlreadyExists(string entity, string value) => $"{entity} '{value}' already exists.";
    public const string AccountNotActivated = "Your account is not activated. Enter the code we sent to your email, or request a new one.";
    public const string AccountLocked = "Too many failed attempts. The account is locked for 15 minutes.";
    public const string InvalidCode = "The code is invalid or has expired.";
    public const string TooManyRequests = "Too many requests. Please wait a moment and try again.";
    public const string TicketCancelled = "This ticket is cancelled.";
    public const string TicketAlreadyUsed = "This ticket has already been used.";
    public const string TicketNotValidNow = "A ticket can be checked in from 60 minutes before the session until it ends.";
    public const string CannotDeleteViaAdmin = "To delete your own account use DELETE /api/auth/account.";
    public const string AdminCannotManageStaff = "Admins can manage only customer accounts. Ask a SuperAdmin.";
}
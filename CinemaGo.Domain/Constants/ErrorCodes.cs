namespace CinemaGo.Domain.Constants;

public static class ErrorCodes
{
    public const string ServerError = "server_error";
    public const string ValidationFailed = "validation_failed";
    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string DuplicateValue = "duplicate_value";
    public const string BadRequest = "bad_request";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";

    public const string SeatTaken = "seat_taken";
    public const string SeatNotInHall = "seat_not_in_hall";
    public const string SessionNotBookable = "session_not_bookable";
    public const string SessionOverlap = "session_overlap";
    public const string BookingAlreadyCancelled = "booking_already_cancelled";
    public const string BookingNotCancellable = "booking_not_cancellable";
    public const string InvalidFile = "invalid_file";
    public const string AccountNotActivated = "account_not_activated";
    public const string AccountLocked = "account_locked";
    public const string InvalidCode = "invalid_code";
    public const string TooManyRequests = "too_many_requests";
    public const string TicketCancelled = "ticket_cancelled";
    public const string TicketAlreadyUsed = "ticket_already_used";
    public const string TicketNotValidNow = "ticket_not_valid_now";
}
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message, string errorCode = ErrorCodes.Conflict, Exception? inner = null)
        : base(message, errorCode, inner) { }
}
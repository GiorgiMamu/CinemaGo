using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = ErrorMessages.Forbidden, string errorCode = ErrorCodes.Forbidden)
        : base(message, errorCode) { }
}
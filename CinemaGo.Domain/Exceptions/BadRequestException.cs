using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message, string errorCode = ErrorCodes.BadRequest)
        : base(message, errorCode) { }
}
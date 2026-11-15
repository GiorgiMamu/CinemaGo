using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = ErrorMessages.Unauthorized)
        : base(message, ErrorCodes.Unauthorized) { }
}
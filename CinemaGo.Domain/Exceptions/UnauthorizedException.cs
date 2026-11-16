using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(Error? error = null) : base(error ?? Errors.General.Unauthorized) { }
}
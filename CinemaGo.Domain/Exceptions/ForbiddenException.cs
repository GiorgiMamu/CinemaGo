using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(Error? error = null) : base(error ?? Errors.General.Forbidden) { }
}
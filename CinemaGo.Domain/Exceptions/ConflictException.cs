using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(Error error, Exception? inner = null) : base(error, inner) { }
}
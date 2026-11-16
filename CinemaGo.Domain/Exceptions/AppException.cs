using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public abstract class AppException : Exception
{
    public Error Error { get; }
    public string ErrorCode => Error.Code;

    protected AppException(Error error, Exception? inner = null) : base(error.Message, inner) => Error = error;
}

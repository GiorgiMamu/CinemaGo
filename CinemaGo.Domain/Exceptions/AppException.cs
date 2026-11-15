namespace CinemaGo.Domain.Exceptions;

public abstract class AppException : Exception
{
    public string ErrorCode { get; }

    protected AppException(string message, string errorCode, Exception? inner = null)
        : base(message, inner) => ErrorCode = errorCode;
}
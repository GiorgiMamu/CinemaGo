using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class DuplicateKeyException : ConflictException
{
    public DuplicateKeyException(string message, Exception? inner = null)
        : base(message, ErrorCodes.DuplicateValue, inner) { }
}
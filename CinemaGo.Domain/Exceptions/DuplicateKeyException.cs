using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class DuplicateKeyException : ConflictException
{
    public DuplicateKeyException(Exception? inner = null) : base(Errors.General.DuplicateValue, inner) { }
}
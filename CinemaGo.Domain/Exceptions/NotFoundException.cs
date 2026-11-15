using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key)
        : base(ErrorMessages.NotFound(entity, key), ErrorCodes.NotFound) { }
}
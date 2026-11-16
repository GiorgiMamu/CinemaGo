using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key) : base(Errors.General.NotFound(entity, key)) { }
}
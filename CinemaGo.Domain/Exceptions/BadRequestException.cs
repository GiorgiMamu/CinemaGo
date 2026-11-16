using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(Error error) : base(error) { }
}
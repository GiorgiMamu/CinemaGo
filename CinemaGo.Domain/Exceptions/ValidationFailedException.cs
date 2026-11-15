using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class ValidationFailedException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationFailedException(IDictionary<string, string[]> errors)
        : base(ErrorMessages.ValidationFailed, ErrorCodes.ValidationFailed) => Errors = errors;
}
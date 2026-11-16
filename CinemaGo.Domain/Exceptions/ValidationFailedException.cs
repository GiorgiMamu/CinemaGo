using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.Exceptions;

public class ValidationFailedException : AppException
{
    public IDictionary<string, string[]> FieldErrors { get; }

    public ValidationFailedException(IDictionary<string, string[]> fieldErrors) : base(Errors.General.ValidationFailed)
        => FieldErrors = fieldErrors;
}
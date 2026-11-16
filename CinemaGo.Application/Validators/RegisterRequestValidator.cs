using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName).Matches("^[a-zA-Z0-9_.-]+$").WithMessage(ValidationMessages.UserNamePattern);
        RuleFor(x => x.Password).MustBeStrongPassword();
    }
}
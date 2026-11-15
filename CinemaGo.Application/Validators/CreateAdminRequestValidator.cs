using CinemaGo.Domain.DTOs;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public class CreateAdminRequestValidator : AbstractValidator<CreateAdminRequest>
{
    public CreateAdminRequestValidator()
    {
        RuleFor(x => x.UserName).Matches("^[a-zA-Z0-9_.-]+$")
            .WithMessage("User name may contain only letters, digits, '.', '_' and '-'.");
        RuleFor(x => x.Password).MustBeStrongPassword();
    }
}
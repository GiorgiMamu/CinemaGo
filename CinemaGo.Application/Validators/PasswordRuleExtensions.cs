using CinemaGo.Domain.Constants;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public static class PasswordRuleExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(this IRuleBuilder<T, string> rule)
        => rule.Matches("[A-Z]").WithMessage(ValidationMessages.PasswordUppercase)
               .Matches("[a-z]").WithMessage(ValidationMessages.PasswordLowercase)
               .Matches("[0-9]").WithMessage(ValidationMessages.PasswordDigit);
}
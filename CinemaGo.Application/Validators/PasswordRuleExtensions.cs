using FluentValidation;
namespace CinemaGo.Application.Validators;

public static class PasswordRuleExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(this IRuleBuilder<T, string> rule)
        => rule.Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
               .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
               .Matches("[0-9]").WithMessage("Password must contain a digit.");
}

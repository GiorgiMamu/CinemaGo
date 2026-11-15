using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public class ChangeRoleRequestValidator : AbstractValidator<ChangeRoleRequest>
{
    public ChangeRoleRequestValidator()
        => RuleFor(x => x.Role).Must(r => r is RoleNames.Admin or RoleNames.Customer).WithMessage(ErrorMessages.InvalidRole);
}
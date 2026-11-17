using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public class SessionRequestValidator : AbstractValidator<SessionRequest>
{
    public SessionRequestValidator()
    {
        RuleFor(x => x.StartsAt).Cascade(CascadeMode.Stop)
            .Must(d => d.Kind == DateTimeKind.Utc).WithMessage(ValidationMessages.StartMustBeUtc)
            .Must(d => d > DateTime.UtcNow).WithMessage(ValidationMessages.StartMustBeFuture);
        RuleFor(x => x.VipPrice).GreaterThanOrEqualTo(x => x.StandardPrice).WithMessage(ValidationMessages.VipBelowStandard);
    }
}

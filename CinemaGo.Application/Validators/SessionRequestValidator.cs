using CinemaGo.Domain.DTOs;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public class SessionRequestValidator : AbstractValidator<SessionRequest>
{
    public SessionRequestValidator()
    {
        RuleFor(x => x.StartsAt).Must(d => d > DateTime.UtcNow).WithMessage("Start time must be in the future (send UTC).");
        RuleFor(x => x.VipPrice).GreaterThanOrEqualTo(x => x.StandardPrice)
            .WithMessage("VIP price cannot be lower than the standard price.");
    }
}
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.SeatIds).Must(ids => ids.Distinct().Count() == ids.Count).WithMessage(ValidationMessages.SeatIdsUnique);
        RuleForEach(x => x.SeatIds).GreaterThan(0).WithMessage(ValidationMessages.SeatIdPositive);
    }
}
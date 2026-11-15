using CinemaGo.Domain.DTOs;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.SeatIds).Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Seat ids must be unique.");
        RuleForEach(x => x.SeatIds).GreaterThan(0).WithMessage("Seat id must be positive.");
    }
}
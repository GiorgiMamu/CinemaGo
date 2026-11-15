using CinemaGo.Domain.DTOs;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public class MovieRequestValidator : AbstractValidator<MovieRequest>
{
    public MovieRequestValidator()
    {
        RuleFor(x => x.GenreIds).Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Genre ids must be unique.");
        RuleForEach(x => x.GenreIds).GreaterThan(0).WithMessage("Genre id must be positive.");
    }
}
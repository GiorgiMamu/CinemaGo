using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public class MovieRequestValidator : AbstractValidator<MovieRequest>
{
    public MovieRequestValidator()
    {
        RuleFor(x => x.GenreIds).Must(ids => ids.Distinct().Count() == ids.Count).WithMessage(ValidationMessages.GenreIdsUnique);
        RuleForEach(x => x.GenreIds).GreaterThan(0).WithMessage(ValidationMessages.GenreIdPositive);
    }
}
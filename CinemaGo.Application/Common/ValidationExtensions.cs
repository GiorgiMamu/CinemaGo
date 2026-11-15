using FluentValidation.Results;
namespace CinemaGo.Application.Common;

public static class ValidationExtensions
{
    public static IDictionary<string, string[]> ToErrorDictionary(this ValidationResult result)
        => result.Errors
            .GroupBy(e => char.ToLowerInvariant(e.PropertyName[0]) + e.PropertyName[1..])
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
}
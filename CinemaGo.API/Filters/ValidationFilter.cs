using CinemaGo.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
namespace CinemaGo.API.Filters;

// Merges DataAnnotations/binding errors (ModelState) and FluentValidation errors into ONE response.
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _services;
    public ValidationFilter(IServiceProvider services) => _services = services;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, List<string>>();

        foreach (var (key, entry) in context.ModelState)
            foreach (var error in entry.Errors)
                Add(errors, key.TrimStart('$', '.'),
                    string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The value is invalid." : error.ErrorMessage);

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;
            if (_services.GetService(typeof(IValidator<>).MakeGenericType(argument.GetType())) is not IValidator validator) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            foreach (var failure in result.Errors) Add(errors, failure.PropertyName, failure.ErrorMessage);
        }

        if (errors.Count > 0)
            throw new ValidationFailedException(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));

        await next();
    }

    private static void Add(Dictionary<string, List<string>> errors, string key, string message)
    {
        key = string.IsNullOrEmpty(key) ? "request" : char.ToLowerInvariant(key[0]) + key[1..];   // camelCase for the frontend
        if (!errors.TryGetValue(key, out var list)) errors[key] = list = new List<string>();
        if (!list.Contains(message)) list.Add(message);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
namespace CinemaGo.API.Common;

public static class ErrorResponseWriter
{
    public static Task WriteAsync(HttpContext context, int status, string code, string message,
        IDictionary<string, string[]>? errors = null)
    {
        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{status}",
            Title = ReasonPhrases.GetReasonPhrase(status),
            Status = status,
            Instance = context.Request.Path
        };
        problem.Extensions["success"] = false;
        problem.Extensions["message"] = message;      // the single human-readable text, same field name as successes
        problem.Extensions["code"] = code;            // stable machine-readable code for the frontend
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (errors is { Count: > 0 }) problem.Extensions["errors"] = errors;

        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
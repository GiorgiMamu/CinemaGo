using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
namespace CinemaGo.API.Common;

public static class ErrorResponseWriter
{
    public static Task WriteAsync(HttpContext context, int status, Error error,
        IDictionary<string, string[]>? fieldErrors = null)
    {
        var body = ApiResponse<object>.Fail(error.Message, error.Code, context.TraceIdentifier,
            fieldErrors is { Count: > 0 } ? fieldErrors : null);

        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(body);   // application/json, camelCase
    }
}
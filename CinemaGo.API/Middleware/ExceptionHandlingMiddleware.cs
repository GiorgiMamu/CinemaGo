using CinemaGo.API.Common;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.Exceptions;
namespace CinemaGo.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug("Request cancelled by the client: {Path}", context.Request.Path);
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Exception after the response started: {Path}", context.Request.Path);
                throw;
            }

            var (status, error, fieldErrors) = Map(ex);
            if (status >= 500)
                _logger.LogError(ex, "Unhandled exception. TraceId {TraceId}", context.TraceIdentifier);
            else
                _logger.LogWarning("Request rejected [{Status} {Code}] TraceId {TraceId}: {Message}",
                    status, error.Code, context.TraceIdentifier, error.Message);

            await ErrorResponseWriter.WriteAsync(context, status, error, fieldErrors);
        }
    }

    // exception type -> HTTP status. Unknown exceptions never leak details: generic 500.
    private static (int Status, Error Error, IDictionary<string, string[]>? FieldErrors) Map(Exception ex) => ex switch
    {
        ValidationFailedException v => (StatusCodes.Status400BadRequest, v.Error, v.FieldErrors),
        BadRequestException b => (StatusCodes.Status400BadRequest, b.Error, null),
        UnauthorizedException u => (StatusCodes.Status401Unauthorized, u.Error, null),
        ForbiddenException f => (StatusCodes.Status403Forbidden, f.Error, null),
        NotFoundException n => (StatusCodes.Status404NotFound, n.Error, null),
        ConflictException c => (StatusCodes.Status409Conflict, c.Error, null),   // includes DuplicateKeyException
        _ => (StatusCodes.Status500InternalServerError, Errors.General.Unexpected, null)
    };
}
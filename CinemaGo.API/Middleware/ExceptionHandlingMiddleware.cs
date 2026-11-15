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

            var (status, code, message, errors) = Map(ex);
            if (status >= 500)
                _logger.LogError(ex, "Unhandled exception. TraceId {TraceId}", context.TraceIdentifier);
            else
                _logger.LogWarning("Request rejected [{Status} {Code}] TraceId {TraceId}: {Message}", status, code, context.TraceIdentifier, message);
            await ErrorResponseWriter.WriteAsync(context, status, code, message, errors);
        }
    }

    private static (int Status, string Code, string Message, IDictionary<string, string[]>? Errors) Map(Exception ex) => ex switch
    {
        ValidationFailedException v => (StatusCodes.Status400BadRequest, v.ErrorCode, v.Message, v.Errors),
        BadRequestException b => (StatusCodes.Status400BadRequest, b.ErrorCode, b.Message, null),
        UnauthorizedException u => (StatusCodes.Status401Unauthorized, u.ErrorCode, u.Message, null),
        ForbiddenException f => (StatusCodes.Status403Forbidden, f.ErrorCode, f.Message, null),
        NotFoundException n => (StatusCodes.Status404NotFound, n.ErrorCode, n.Message, null),
        ConflictException c => (StatusCodes.Status409Conflict, c.ErrorCode, c.Message, null),
        _ => (StatusCodes.Status500InternalServerError, ErrorCodes.ServerError, ErrorMessages.Unexpected, null)
    };
}
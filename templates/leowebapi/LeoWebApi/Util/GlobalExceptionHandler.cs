using Microsoft.AspNetCore.Diagnostics;

namespace LeoWebApi.Util;

/// <summary>
///     Turns unhandled exceptions into an RFC 9457 problem details response
/// </summary>
/// <remarks>
///     Exception details are never sent to the client, they are only logged.
///     The <c>traceId</c> that is added to every problem details response correlates the client side error
///     with the log entry written here.
/// </remarks>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger,
                                           IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
                                                CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred while processing the request to {Path}",
                        httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Title = "An unexpected error occurred",
                Status = StatusCodes.Status500InternalServerError
            }
        });
    }
}

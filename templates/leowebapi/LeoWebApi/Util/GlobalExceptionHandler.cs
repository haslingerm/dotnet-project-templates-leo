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
        if (exception is BadHttpRequestException badRequestException)
        {
            logger.LogInformation(exception, "A malformed request to {Path} was rejected", httpContext.Request.Path);

            return await WriteProblemDetailsAsync(httpContext, badRequestException.StatusCode,
                                                  "The request could not be processed");
        }

        logger.LogError(exception, "An unhandled exception occurred while processing the request to {Path}",
                        httpContext.Request.Path);

        return await WriteProblemDetailsAsync(httpContext, StatusCodes.Status500InternalServerError,
                                              "An unexpected error occurred");
    }

    private async ValueTask<bool> WriteProblemDetailsAsync(HttpContext httpContext, int statusCode, string title)
    {
        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Title = title,
                Status = statusCode
            }
        });
    }
}

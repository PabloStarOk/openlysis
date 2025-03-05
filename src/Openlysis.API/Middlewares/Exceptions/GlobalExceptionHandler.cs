using FastEndpoints;

using Microsoft.AspNetCore.Diagnostics;

namespace Openlysis.API.Middlewares.Exceptions;

/// <summary>
/// Handles unexpected exceptions occurred.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger to log exceptions.</param>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int statusCode, string message) = MapResponse(exception);
        LogException(httpContext, exception, statusCode);
        await SendProblemDetails(httpContext, statusCode, message);
        return true;
    }

    /// <summary>
    /// Sends a problem details response to the client.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static async Task SendProblemDetails(HttpContext httpContext, int statusCode, string message)
    {
        var problemDetails = new ProblemDetails
        {
            Detail = message,
            Status = statusCode,
        };
        await problemDetails.ExecuteAsync(httpContext); // Method by FastEndpoints.
    }

    /// <summary>
    /// Maps an exception to an appropriate HTTP status code and message.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>A tuple containing the HTTP status code and message.</returns>
    private static (int, string) MapResponse(Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException badException => (badException.StatusCode, badException.Message),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred, please try again later.")
        };
    }

    /// <summary>
    /// Logs the exception details if the status code is 500 or higher.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    private void LogException(HttpContext httpContext, Exception exception, int statusCode)
    {
        if (statusCode < 500)
        {
            return;
        }

        _logger.LogError(
            exception,
            "Exception occurred: {Method} {Path} | HTTP Status Code: {StatusCode} | Exception Message: {Message} | Trace ID: {TraceIdentifier}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            statusCode,
            exception.Message,
            httpContext.TraceIdentifier);
    }
}
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Openlysis.Authentication.API.Endpoints.Middlewares;

/// <summary>
/// Handles all unhandled exceptions globally and generates generic error responses.
/// </summary>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private const string GenericDetail = "An unexpected error occurred. Please try again later.";

    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging exceptions.</param>
    /// <param name="problemDetailsService">The service for writing problem details responses.</param>
    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        LogException(httpContext, exception);
        await SendResponseAsync(httpContext, exception);
        return true;
    }

    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Reviewed.")]
    private void LogException(HttpContext httpContext, Exception exception)
    {
        _logger.LogError(
            exception,
            "Exception occurred:"
            + "\n\tHTTP method: {Method}"
            + "\n\tEndpoint path: {Path}"
            + "\n\tTrace ID: {TraceIdentifier}"
            + "\n\tUser: {User}"
            + "\n\tQuery: {QueryString}"
            + "\n\tRemote IP: {RemoteIp}"
            + "\n\tException type: {ExceptionType}"
            + "\n\tTimestamp (UTC): {Timestamp}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier,
            httpContext.User.Identity?.Name ?? "anonymous",
            httpContext.Request.QueryString,
            httpContext.Connection.RemoteIpAddress,
            exception.GetType().FullName,
            DateTimeOffset.UtcNow);
    }

    private async Task SendResponseAsync(
        HttpContext httpContext,
        Exception exception)
    {
        await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Detail = GenericDetail,
            },
            AdditionalMetadata = null,
        });
    }
}
using Openlysis.Application.Common.Abstractions.Services;

namespace Openlysis.API.Middlewares.Files;

/// <summary>
/// Middleware that cleans up file storage after certain API requests.
/// </summary>
internal sealed class FileStorageCleanupMiddleware : IMiddleware
{
    private const string AnalyzeFileEndpointPath = "/api/v1/files";
    private const string AnalyzeMessageEndpointPath = "/api/v1/messages";

    private readonly IFileStorageContext _fileStorageContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageCleanupMiddleware"/> class.
    /// </summary>
    /// <param name="fileStorageContext">The file storage context used for cleanup operations.</param>
    public FileStorageCleanupMiddleware(IFileStorageContext fileStorageContext)
    {
        _fileStorageContext = fileStorageContext;
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var shouldCleanup =
            (context.Request.Path == AnalyzeFileEndpointPath || context.Request.Path == AnalyzeMessageEndpointPath)
            && context.Request.Method == HttpMethod.Post.Method;

        if (!shouldCleanup)
        {
            await next(context);
            return;
        }

        try
        {
            await next(context);

            if (context.Response.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
            {
                return;
            }

            switch (context.Response.StatusCode)
            {
                // Analysis retrieved, not started (accepted 202).
                case StatusCodes.Status200OK:
                case < 200 or > 299:
                    await _fileStorageContext.RemoveAllAsync();
                    break;
            }
        }
        catch
        {
            await _fileStorageContext.RemoveAllAsync();
            throw;
        }
    }
}
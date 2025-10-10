using FastEndpoints;

using Openlysis.Application.Common.Abstractions.Services;

namespace Openlysis.API.Middlewares.Files;

/// <summary>
/// Post-processor that cleans up file storage after endpoint execution.
/// Removes all files if an exception occurred or if the response status code is not successful.
/// </summary>
/// <typeparam name="TRequest">The type of the request handled by the endpoint.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the endpoint.</typeparam>
internal sealed class FileStorageCleanupPostProcessor<TRequest, TResponse>
    : IPostProcessor<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    /// <inheritdoc/>
    public async Task PostProcessAsync(IPostProcessorContext<TRequest, TResponse> context, CancellationToken ct)
    {
        IFileStorageContext fileStorageContext = context.HttpContext.RequestServices.GetRequiredService<IFileStorageContext>();
        if (context.HasExceptionOccurred)
        {
            await fileStorageContext.RemoveAllAsync();
            return;
        }

        HttpResponse response = context.HttpContext.Response;
        if (response.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
        {
            return;
        }

        switch (response.StatusCode)
        {
            // Analysis retrieved, not started (accepted 202).
            case StatusCodes.Status200OK:
            case < 200 or > 299:
                await fileStorageContext.RemoveAllAsync();
                break;
        }
    }
}
namespace Openlysis.API.Configuration.Options;

/// <summary>
/// Options for configuring file upload settings.
/// </summary>
public record FileUploadOptions
{
    /// <summary>
    /// Gets the maximum size of the request body in bytes.
    /// </summary>
    public int MaxRequestBodySize { get; init; }

    /// <summary>
    /// Gets the memory buffer threshold in bytes.
    /// </summary>
    public int MemoryBufferThreshold { get; init; }
}
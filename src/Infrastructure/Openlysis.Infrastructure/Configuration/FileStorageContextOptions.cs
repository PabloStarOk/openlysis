namespace Openlysis.Infrastructure.Configuration;

/// <summary>
/// Options for configuring file storage context.
/// </summary>
public sealed record FileStorageContextOptions
{
    /// <summary>
    /// The configuration section name for file storage context.
    /// </summary>
    public const string SectionName = "FileStorageContext";

    private const int DefaultBufferSize = 65536;
    private const string DefaultContentType = "application/octet-stream";

    /// <summary>
    /// Gets or sets the fallback content type to use when none is specified.
    /// </summary>
    required public string FallbackContentType { get; set; } = DefaultContentType;

    /// <summary>
    /// Gets or sets the buffer size in bytes for streaming file data.
    /// </summary>
    required public int StreamingBufferSizeBytes { get; set; } = DefaultBufferSize;

    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes.
    /// </summary>
    required public long MaxFileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of files that can be processed by the context.
    /// </summary>
    required public int MaxProcessableFiles { get; set; }
}
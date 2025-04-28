namespace Openlysis.API.Configuration.Options;

/// <summary>
/// Options for configuring file upload settings.
/// </summary>
public record FileUploadOptions
{
    /// <summary>
    /// The configuration section name for file upload options.
    /// </summary>
    public const string SectionName = "FileUpload";

    /// <summary>
    /// Gets the maximum file size allowed for upload in bytes.
    /// </summary>
    required public int MaxFileSize { get; init; }

    /// <summary>
    /// Gets the memory buffer threshold in bytes.
    /// </summary>
    required public int MemoryBufferThreshold { get; init; }
}
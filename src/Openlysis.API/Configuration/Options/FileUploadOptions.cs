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
    /// Gets the maximum number of files allowed in a single request.
    /// </summary>
    required public int MaxFileUploadsLimit { get; init; }
}
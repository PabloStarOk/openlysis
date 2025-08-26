namespace Openlysis.Infrastructure.Shared.Communication.Services.Files;

/// <summary>
/// Options for configuring Google Cloud Storage integration.
/// </summary>
internal sealed record GoogleCloudStorageOptions
{
    /// <summary>
    /// The configuration section name for Google Cloud Storage options.
    /// </summary>
    public const string SectionName = "GoogleCloudStorage";

    /// <summary>
    /// Gets or sets the name of the Google Cloud Storage bucket.
    /// </summary>
    required public string BucketName { get; set; }
}
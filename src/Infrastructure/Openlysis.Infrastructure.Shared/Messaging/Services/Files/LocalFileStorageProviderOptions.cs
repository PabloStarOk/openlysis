namespace Openlysis.Infrastructure.Shared.Messaging.Services.Files;

/// <summary>
/// Represents the configuration options for the local file storage provider.
/// </summary>
internal record LocalFileStorageProviderOptions
{
    /// <summary>
    /// The configuration section name for the local file storage provider.
    /// </summary>
    public const string SectionName = "LocalFileStorageProvider";

    /// <summary>
    /// Gets the root directory path for storing files locally.
    /// Defaults to the system's temporary directory path.
    /// </summary>
    public string StorageRootDirectoryPath { get; init; } = Path.GetTempPath();
}
using Microsoft.Extensions.Options;

namespace Openlysis.Infrastructure.Shared.Communication.Services.Files;

/// <summary>
/// Validates the configuration options for the local file storage provider.
/// </summary>
internal static class LocalFileStorageProviderOptionsValidator
{
    private const string PropertyName = nameof(
        LocalFileStorageProviderOptions.StorageRootDirectoryPath);

    /// <summary>
    /// Validates the provided <see cref="LocalFileStorageProviderOptions"/> instance.
    /// </summary>
    /// <param name="options">The configuration options to validate.</param>
    /// <returns>
    /// A <see cref="ValidateOptionsResult"/> indicating whether the validation was successful
    /// or providing details about the validation failure.
    /// </returns>
    internal static ValidateOptionsResult Validate(LocalFileStorageProviderOptions options)
    {
        var path = options.StorageRootDirectoryPath;

        if (string.IsNullOrWhiteSpace(path))
        {
            return ValidateOptionsResult.Fail($"{PropertyName} cannot be null, empty, or whitespace.");
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return ValidateOptionsResult.Fail($"{PropertyName} contains invalid characters.");
        }

        try
        {
            if (!Directory.Exists(path))
            {
                return ValidateOptionsResult.Fail($"{PropertyName} does not exist or is not a directory.");
            }
        }
        catch (Exception ex)
        {
            return ValidateOptionsResult.Fail($"{PropertyName} validation failed: {ex.Message}");
        }

        return ValidateOptionsResult.Success;
    }
}
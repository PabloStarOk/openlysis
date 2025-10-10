using Microsoft.Extensions.Options;

namespace Openlysis.Infrastructure.Shared.Infrastructure.ConfigLoader.Configuration;

/// <summary>
/// Validates that the <see cref="ConfigPathsOptions"/> has at least one file or directory path configured.
/// </summary>
/// <remarks>
/// This validator ensures that at least one of <see cref="ConfigPathsOptions.Directories"/>
/// or <see cref="ConfigPathsOptions.JsonFiles"/> has entries.
/// </remarks>
internal static class ConfigPathsOptionsValidator
{
    /// <summary>
    /// Validates the provided configuration options.
    /// </summary>
    /// <param name="options">The configuration options to validate.</param>
    /// <returns>
    /// <see cref="ValidateOptionsResult.Success"/> if at least one directory or file path is configured;
    /// otherwise, <see cref="ValidateOptionsResult.Failed"/> with an error message.
    /// </returns>
    internal static ValidateOptionsResult Validate(ConfigPathsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var directoryValidation = ValidateDirectories(options.Directories);
        if (directoryValidation.Failed)
        {
            return directoryValidation;
        }

        var filepathValidation = ValidateFiles(options.JsonFiles);
        return filepathValidation.Failed
            ? filepathValidation
            : ValidateOptionsResult.Success;
    }

    /// <summary>
    /// Validates that each URI in the list points to an existing directory.
    /// </summary>
    /// <param name="directoryPaths">A collection of directory paths to validate.</param>
    /// <returns>
    /// <see cref="ValidateOptionsResult.Success"/> if all URIs point to existing directories;
    /// otherwise, <see cref="ValidateOptionsResult.Failed"/> with error messages for each invalid directory.
    /// </returns>
    private static ValidateOptionsResult ValidateDirectories(
        IEnumerable<string> directoryPaths)
    {
        var errorMessages = directoryPaths
            .Where(d => File.Exists(d) || !Directory.Exists(d))
            .Select(d => $"Directory not found: '{d}'")
            .ToArray();

        return errorMessages.Length is 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errorMessages);
    }

    /// <summary>
    /// Validates that each URI in the list points to an existing file.
    /// </summary>
    /// <param name="filePaths">A collection of file paths to validate.</param>
    /// <returns>
    /// <see cref="ValidateOptionsResult.Success"/> if all URIs point to existing files;
    /// otherwise, <see cref="ValidateOptionsResult.Failed"/> with error messages for each invalid file.
    /// </returns>
    private static ValidateOptionsResult ValidateFiles(IEnumerable<string> filePaths)
    {
        var errorMessages = filePaths
            .Where(f => !File.Exists(f))
            .Select(f => $"File not found: '{f}'")
            .ToArray();

        return errorMessages.Length is 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errorMessages);
    }
}
using Microsoft.Extensions.Options;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Configuration.Common;

/// <summary>
/// Validates that the <see cref="SandboxAnalyzerOptions"/> configuration has no overlapping file types
/// between OS-specific collections and the AnyOsSupportedFiles collection.
/// </summary>
/// <remarks>
/// This validator ensures that file types are properly categorized into their specific OS environments
/// and prevents duplication between the general collection and OS-specific collections.
/// </remarks>
internal class SandboxAnalyzerOptionsValidator : IValidateOptions<SandboxAnalyzerOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, SandboxAnalyzerOptions opts)
    {
        var anyOs = opts.AnyOsSupportedMimeTypes;

        if (opts.WinOnlySupportedMimeTypes.Overlaps(anyOs))
        {
            return GetOverlapsFailResult(nameof(opts.WinOnlySupportedMimeTypes));
        }

        if (opts.Win7HwpOnlySupportedMimeTypes.Overlaps(anyOs))
        {
            return GetOverlapsFailResult(nameof(opts.Win7HwpOnlySupportedMimeTypes));
        }

        if (opts.LinuxOnlySupportedMimeTypes.Overlaps(anyOs))
        {
            return GetOverlapsFailResult(nameof(opts.LinuxOnlySupportedMimeTypes));
        }

        if (opts.MacOnlySupportedMimeTypes.Overlaps(anyOs))
        {
            return GetOverlapsFailResult(nameof(opts.MacOnlySupportedMimeTypes));
        }

        return opts.AndroidOnlySupportedMimeTypes.Overlaps(anyOs)
            ? GetOverlapsFailResult(nameof(opts.AndroidOnlySupportedMimeTypes))
            : ValidateOptionsResult.Success;
    }

    /// <summary>
    /// Creates a validation failure result when a collection overlaps with the AnyOsSupportedFiles collection.
    /// </summary>
    /// <param name="hashSetName">The name of the collection that has overlapping values.</param>
    /// <returns>A <see cref="ValidateOptionsResult"/> indicating validation failure with an appropriate error message.</returns>
    private static ValidateOptionsResult GetOverlapsFailResult(string hashSetName)
    {
        string errorMessage = $"Values in {hashSetName} must not appear in {nameof(SandboxAnalyzerOptions.AnyOsSupportedMimeTypes)}";
        return ValidateOptionsResult.Fail(errorMessage);
    }
}

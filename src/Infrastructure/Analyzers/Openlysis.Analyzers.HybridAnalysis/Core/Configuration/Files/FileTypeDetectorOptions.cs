using System.ComponentModel.DataAnnotations;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Configuration.Files;

/// <summary>
/// Represents configuration options for the file type detector.
/// </summary>
/// <remarks>
/// Contains settings that control how file types are detected, including
/// header size to read, minimum match percentage, and fallback MIME type.
/// </remarks>
internal record FileTypeDetectorOptions
{
    /// <summary>
    /// The configuration section name for file type detector settings.
    /// </summary>
    internal const string SectionName = "HybridAnalysis:FileTypeDetector";

    /// <summary>
    /// Gets the size in bytes to read from the beginning of a file.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    required public int FileHeaderSizeInBytes { get; init; }

    /// <summary>
    /// Gets the minimum percentage match required to determine the file type.
    /// Value should be between 0 and 1.0.
    /// </summary>
    [Required]
    [Range(typeof(decimal), "0.0", "1.0")]
    required public decimal MinAcceptableMatchPercentage { get; init; }

    /// <summary>
    /// Gets the default MIME type to use when the file type cannot be determined.
    /// </summary>
    [Required]
    required public string FallbackMimeType { get; init; }
}
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

using Openlysis.TestTools.ServicesSimulation.Common.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.Files.Configuration;

/// <summary>
/// Represents configuration options for the file analysis stub factory.
/// Extends the base analysis stub factory options.
/// </summary>
internal sealed record FileAnalysisStubFactoryOptions : AnalysisStubFactoryOptions
{
    /// <summary>
    /// Gets a value indicating whether to use file reports for analysis.
    /// </summary>
    [Required]
    required public bool UseFileReports { get; init; }

    /// <summary>
    /// Gets the maximum number of file reports that can be returned by the stub factory.
    /// </summary>
    /// <remarks>
    /// The value must be at least 1.
    /// </remarks>
    [Required]
    [Range(1, int.MaxValue)]
    required public int MaxFileReports { get; init; }

    /// <summary>
    /// Gets an array of file report stub options that can be returned by the stub factory.
    /// </summary>
    [ValidateEnumeratedItems]
    required public FileReportStubOptions[] ReturnableFileReports { get; init; } = [];
}
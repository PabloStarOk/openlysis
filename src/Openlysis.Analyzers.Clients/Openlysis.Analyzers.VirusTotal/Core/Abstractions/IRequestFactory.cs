using Openlysis.Analyzers.VirusTotal.Core.Models.Objects;

namespace Openlysis.Analyzers.VirusTotal.Core.Abstractions;

/// <summary>
/// Defines a factory to create <see cref="VirusTotalAnalysisRequest"/>.
/// </summary>
public interface IRequestFactory
{
    /// <summary>
    /// Creates a VirusTotal analysis request from the given analyze request.
    /// </summary>
    /// <returns>A <see cref="VirusTotalAnalysisRequest"/> object.</returns>
    public VirusTotalAnalysisRequest Create();
}
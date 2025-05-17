using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Common;

/// <summary>
/// Factory interface for creating Hybrid Analysis request objects.
/// </summary>
internal interface IRequestFactory
{
    /// <summary>
    /// Creates a new Hybrid Analysis submit request.
    /// </summary>
    /// <returns>A new instance of <see cref="HybridAnalysisAnalyzeRequest"/>.</returns>
    public HybridAnalysisAnalyzeRequest Create();
}
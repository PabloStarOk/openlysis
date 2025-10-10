namespace Openlysis.Analyzers.HybridAnalysis.Core.Constants;

/// <summary>
/// Contains the API endpoint addresses for the Hybrid Analysis service.
/// </summary>
internal static class Addresses
{
    /// <summary>
    /// The endpoint for retrieving the state of services of quick scan.
    /// </summary>
    internal const string QuickScanStateEndpoint = "quick-scan/state";

    /// <summary>
    /// The endpoint for submitting a file for quick scanning.
    /// </summary>
    internal const string FileQuickScanEndpoint = "quick-scan/file";

    /// <summary>
    /// The endpoint for getting a quick scan result using the provided identifier.
    /// </summary>
    internal const string GetQuickScanEndpoint = "quick-scan/{0}";

    /// <summary>
    /// The endpoint for retrieving the analysis overview summary for a specific identifier.
    /// </summary>
    internal const string AnalysisSummaryEndpoint = "overview/{0}/summary";

    /// <summary>
    /// The endpoint for submitting a URL to the sandbox.
    /// </summary>
    internal const string SandboxSubmitFileEndpoint = "submit/file";

    /// <summary>
    /// The endpoint for submitting a URL to the sandbox.
    /// </summary>
    internal const string SandboxSubmitUrlEndpoint = "submit/url";

    /// <summary>
    /// The endpoint for retrieving the state of a sandbox report.
    /// </summary>
    internal const string SandboxReportStateEndpoint = "report/{0,0}/state";

    /// <summary>
    /// The endpoint for retrieving the summary of a sandbox report.
    /// </summary>
    internal const string SandboxReportSummary = "report/{0,0}/summary";

    /// <summary>
    /// The endpoint for submitting a hash to retrieve a URL.
    /// </summary>
    internal const string SubmitHashForUrl = "submit/hash-for-url";
}
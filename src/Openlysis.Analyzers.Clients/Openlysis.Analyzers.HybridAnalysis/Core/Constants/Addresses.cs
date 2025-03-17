namespace Openlysis.Analyzers.HybridAnalysis.Core.Constants;

/// <summary>
/// Contains the API endpoint addresses for the Hybrid Analysis service.
/// </summary>
internal static class Addresses
{
    /// <summary>
    /// The base URL for the Hybrid Analysis API.
    /// </summary>
    internal const string Base = "https://www.hybrid-analysis.com/api/v2/";

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
}
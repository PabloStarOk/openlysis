namespace Openlysis.Analyzers.URLQuery.Core.Constants;

/// <summary>
/// Contains the addresses for various endpoints of the URLQuery API.
/// </summary>
internal static class Addresses
{
    /// <summary>
    /// The endpoint for submitting a URL.
    /// </summary>
    internal const string SubmitUrlEndpoint = "submit/url";

    /// <summary>
    /// The endpoint for checking the status of a submitted URL.
    /// </summary>
    internal const string SubmitStatusEndpoint = "submit/status/{0,0}";

    /// <summary>
    /// The endpoint for getting a detailed report of a URL.
    /// </summary>
    internal const string ReportEndpoint = "report/{0,0}";
}
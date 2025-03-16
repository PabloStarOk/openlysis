namespace Openlysis.Analyzers.URLQuery.Core.Constants;

/// <summary>
/// Contains the addresses for various endpoints of the URLQuery API.
/// </summary>
internal static class Addresses
{
    /// <summary>
    /// The base URL for the URLQuery API.
    /// </summary>
    internal const string Base = "https://api.urlquery.net/public/v1/";

    /// <summary>
    /// The endpoint for submitting a URL.
    /// </summary>
    internal const string SubmitUrlEndpoint = "submit/url";

    /// <summary>
    /// The endpoint for checking the status of a submitted URL.
    /// </summary>
    internal const string SubmitStatusEndpoint = "submit/status/{0,0}";

    /// <summary>
    /// The endpoint for getting an overview report of a URL.
    /// </summary>
    internal const string ReportOverviewEndpoint = "report/{0,0}/overview";
}
using Openlysis.Analyzers.URLQuery.Core.Models.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Models.Requests;

/// <summary>
/// Represents a request to submit a URL for analysis.
/// </summary>
/// <param name="Url">The URL to be analyzed.</param>
/// <param name="UserAgent">The user agent string to be used for the request.</param>
/// <param name="Access">The access level for the URL submission.</param>
/// <param name="Referer">The referer URL, if any. Default is an empty string.</param>
internal record SubmitUrlRequest(
    string Url,
    string UserAgent,
    Access Access,
    string Referer = "");
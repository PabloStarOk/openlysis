using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Constants;
using Openlysis.Analyzers.VirusTotal.Core.Models.Objects;

namespace Openlysis.Analyzers.VirusTotal.Infrastructure.Factories;

/// <summary>
/// Creates VirusTotal analysis requests for URLs.
/// </summary>
public class UrlRequestFactory : IRequestFactory
{
    private readonly AnalyzeUrlRequest _request;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlRequestFactory"/> class with the specified request.
    /// </summary>
    /// <param name="request">The URL analysis request.</param>
    public UrlRequestFactory(AnalyzeUrlRequest request)
    {
        _request = request;
    }

    /// <inheritdoc/>
    public VirusTotalAnalysisRequest Create()
    {
        var dictionary = new Dictionary<string, string>(1)
        {
            { "url", _request.Url.AbsoluteUri },
        };
        var urlEncodedEncoded = new FormUrlEncodedContent(dictionary);
        return new VirusTotalAnalysisRequest(
            Addresses.UrlsEndpoint,
            urlEncodedEncoded);
    }
}
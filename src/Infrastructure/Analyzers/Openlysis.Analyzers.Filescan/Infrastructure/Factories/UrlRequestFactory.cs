using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Constants;
using Openlysis.Analyzers.Filescan.Core.Models.Objects;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;

namespace Openlysis.Analyzers.Filescan.Infrastructure.Factories;

/// <summary>
/// Factory class for creating URL requests for analysis.
/// </summary>
public class UrlRequestFactory : IRequestFactory
{
    private readonly AnalyzeUrlRequest _request;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlRequestFactory"/> class.
    /// </summary>
    /// <param name="request">The URL request to be analyzed.</param>
    public UrlRequestFactory(AnalyzeUrlRequest request)
    {
        _request = request;
    }

    /// <inheritdoc/>
    public FilescanAnalysisRequest Create()
    {
        var dictionary = new Dictionary<string, string>
        {
            { ScanFieldNames.Url, _request.Url.AbsoluteUri },
            { ScanFieldNames.Description, _request.Description ?? string.Empty },
            { ScanFieldNames.IsPrivateFile, _request.IsPrivate.ToString() },
            { ScanFieldNames.SkipWhiteListed, false.ToString() },
            { ScanFieldNames.RapidMode, false.ToString() },
            { ScanFieldNames.ResolveDomains, true.ToString() },
            { ScanFieldNames.UrlAnalysis, true.ToString() },
            { ScanFieldNames.PhishingDetection, true.ToString() },
        }.AsReadOnly();

        var urlEncodedContent = new FormUrlEncodedContent(dictionary);

        return new FilescanAnalysisRequest(
            Addresses.ScanUrl,
            urlEncodedContent);
    }
}
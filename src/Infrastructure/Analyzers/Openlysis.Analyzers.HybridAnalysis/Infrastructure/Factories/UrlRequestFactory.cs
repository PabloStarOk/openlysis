using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Factories;

/// <summary>
/// Factory for creating Hybrid Analysis URL submission requests.
/// Handles the conversion of URL analysis requests into the format required by the Hybrid Analysis API.
/// </summary>
internal class UrlRequestFactory : IRequestFactory
{
    private const string UrlBodyParamName = "url";
    private const string EnvironmentIdBodyParamName = "environment_id";
    private const string AntiEvasionBodyParamName = "experimental_anti_evasion";

    private readonly HybridAnalyzerOptions _analyzerOptions;
    private readonly AnalyzeUrlRequest _request;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlRequestFactory"/> class.
    /// </summary>
    /// <param name="analyzerOptions">The options containing configuration for the Hybrid Analysis analyzer.</param>
    /// <param name="request">The URL request to be analyzed.</param>
    public UrlRequestFactory(
        HybridAnalyzerOptions analyzerOptions,
        AnalyzeUrlRequest request)
    {
        _analyzerOptions = analyzerOptions;
        _request = request;
    }

    /// <inheritdoc/>
    public HybridAnalysisSubmitRequest Create()
    {
        var dictionary = new Dictionary<string, string>(3);
        AddUrlToDictionary(dictionary);
        AddEnvironmentIdToDictionary(dictionary);
        AddAntiEvasionToDictionary(dictionary);
        var content = new FormUrlEncodedContent(dictionary);

        return new HybridAnalysisSubmitRequest(
            Addresses.SandboxSubmitUrlEndpoint,
            content);
    }

    /// <summary>
    /// Adds the URL parameter to the request dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to add the URL parameter to.</param>
    private void AddUrlToDictionary(Dictionary<string, string> dictionary)
    {
        dictionary.Add(UrlBodyParamName, _request.Url.AbsoluteUri);
    }

    /// <summary>
    /// Adds the environment ID parameter to the request dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to add the environment ID parameter to.</param>
    private void AddEnvironmentIdToDictionary(Dictionary<string, string> dictionary)
    {
        SandboxEnvironment environmentId = _analyzerOptions.DefaultSandboxEnvironment;
        dictionary.Add(EnvironmentIdBodyParamName, environmentId.ToString("D"));
    }

    /// <summary>
    /// Adds the experimental anti-evasion parameter to the request dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to add the anti-evasion parameter to.</param>
    private void AddAntiEvasionToDictionary(Dictionary<string, string> dictionary)
    {
        bool useAntiEvasion = _analyzerOptions.UseExperimentalAntiEvasion;
        dictionary.Add(AntiEvasionBodyParamName, useAntiEvasion.ToString());
    }
}
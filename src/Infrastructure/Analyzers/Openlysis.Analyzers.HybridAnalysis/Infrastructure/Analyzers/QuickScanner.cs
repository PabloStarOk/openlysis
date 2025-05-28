using ErrorOr;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Common;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Objects;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Contracts.Common.Constants;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization.Abstractions;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Analyzers;

/// <summary>
/// Provides functionality for performing quick scans against Hybrid Analysis sandbox services.
/// </summary>
/// <remarks>
/// This class handles the submission of scan requests, retrieving scan results,
/// and obtaining analysis summaries from the Hybrid Analysis API.
/// </remarks>
internal class QuickScanner : IQuickScanner
{
    private readonly IServiceLogger<QuickScanner> _logger;
    private readonly IServiceDeserializer _serviceDeserializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuickScanner"/> class.
    /// </summary>
    /// <param name="logger">The service logger for logging operations.</param>
    /// <param name="serviceDeserializer">The service deserializer for processing API responses,
    /// retrieved from keyed services using the SandboxAnalyzer key.</param>
    public QuickScanner(
        IServiceLogger<QuickScanner> logger,
        [FromKeyedServices(SandboxAnalyzer.KeyedServicesKey)] IServiceDeserializer serviceDeserializer)
    {
        _logger = logger;
        _serviceDeserializer = serviceDeserializer;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<QuickScanService[]>> GetStateAsync(
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(
            Addresses.QuickScanStateEndpoint,
            cancellationToken);

        return await DeserializeResponse<QuickScanService[]>(
            response,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<QuickScanResponse>> ScanAsync(
        HttpClient httpClient,
        IRequestFactory requestFactory,
        CancellationToken cancellationToken = default)
    {
        using HybridAnalysisAnalyzeRequest request = requestFactory.Create();
        using HttpResponseMessage response = await httpClient.PostAsync(
            request.EndpointAddress,
            request.HttpContent,
            cancellationToken);

        return await DeserializeResponse<QuickScanResponse>(
            response,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<QuickScanResponse>> GetQuickScanAsync(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default)
    {
        string formattedAddress = string.Format(Addresses.GetQuickScanEndpoint, id);
        using HttpResponseMessage response = await httpClient.GetAsync(
            formattedAddress,
            cancellationToken);

        return await DeserializeResponse<QuickScanResponse>(
            response,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<AnalysisSummary>> GetAnalysisSummaryAsync(
        HttpClient httpClient,
        string sha256,
        CancellationToken cancellationToken = default)
    {
        string formattedAddress = string.Format(
            Addresses.AnalysisSummaryEndpoint,
            sha256);
        using HttpResponseMessage response = await httpClient.GetAsync(
            formattedAddress,
            cancellationToken);

        return await DeserializeResponse<AnalysisSummary>(
            response,
            cancellationToken);
    }

    /// <summary>
    /// Deserializes the HTTP response into the specified model type.
    /// </summary>
    /// <typeparam name="TModel">The type to deserialize the response content into.</typeparam>
    /// <param name="response">The HTTP response message to deserialize.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation that returns either:
    /// - The deserialized model of type <typeparamref name="TModel"/> if successful
    /// - An error if the response has a non-success status code or deserialization fails.
    /// </returns>
    private async Task<ErrorOr<TModel>> DeserializeResponse<TModel>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        if (response.IsSuccessStatusCode)
        {
            return await _serviceDeserializer.DeserializeAsync<TModel>(
                response,
                cancellationToken);
        }

        await _logger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
        return ServiceErrors.NonSuccessStatusCode;
    }
}
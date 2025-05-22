using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;

/// <summary>
/// Provides an implementation of <see cref="IOptionsMonitor{TOptions}"/> for rate quota endpoint options
/// specific to analysis endpoints.
/// </summary>
/// <typeparam name="TStubFactoryOptions">The type of stub factory options, which must inherit from
/// <see cref="AnalysisStubFactoryOptions"/>.</typeparam>
internal sealed class RateQuotaOptionsMonitor<TStubFactoryOptions>
    : IOptionsMonitor<RateQuotaEndpointOptions<AnalysisEndpointType>>
    where TStubFactoryOptions : AnalysisStubFactoryOptions
{
    /// <inheritdoc/>
    public RateQuotaEndpointOptions<AnalysisEndpointType> CurrentValue
        => throw new NotImplementedException();

    private readonly string _simulatedServiceOptionsName;
    private readonly IOptionsMonitor<AnalysisServiceOptions<TStubFactoryOptions>> _optionsMonitor;
    private readonly IReadOnlyDictionary<string, AnalysisEndpointType> _endpointOptionsMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateQuotaOptionsMonitor{TStubFactoryOptions}"/> class.
    /// </summary>
    /// <param name="simulatedServiceOptionsName">The name used to retrieve the appropriate service options.</param>
    /// <param name="optionsMonitor">The options monitor for accessing analysis service options.</param>
    /// <param name="endpointOptionsMap">A mapping between rate quota endpoint options names and their corresponding endpoint types.</param>
    public RateQuotaOptionsMonitor(
        string simulatedServiceOptionsName,
        IOptionsMonitor<AnalysisServiceOptions<TStubFactoryOptions>> optionsMonitor,
        IReadOnlyDictionary<string, AnalysisEndpointType> endpointOptionsMap)
    {
        _simulatedServiceOptionsName = simulatedServiceOptionsName;
        _optionsMonitor = optionsMonitor;
        _endpointOptionsMap = endpointOptionsMap;
    }

    /// <inheritdoc/>
    public RateQuotaEndpointOptions<AnalysisEndpointType> Get(string? name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (!_endpointOptionsMap.TryGetValue(
                name,
                out AnalysisEndpointType endpointType))
        {
            throw new InvalidOperationException($"There are no rate quota endpoint options for request '{name}' name");
        }

        var options = _optionsMonitor.Get(_simulatedServiceOptionsName);
        return endpointType switch
        {
            AnalysisEndpointType.Analyze =>
                GenerateRateQuotaEndpointOptions(
                    endpointType,
                    options.AnalyzeEndpoint),

            AnalysisEndpointType.GetStatus=>
                GenerateRateQuotaEndpointOptions(
                    endpointType,
                    options.GetStatusEndpoint),

            AnalysisEndpointType.GetResults =>
                GenerateRateQuotaEndpointOptions(
                    endpointType,
                    options.GetAnalysisEndpoint),
            _ => throw new InvalidOperationException($"Unsupported endpoint type: {endpointType}"),
        };
    }

    /// <inheritdoc/>
    public IDisposable? OnChange(
        Action<RateQuotaEndpointOptions<AnalysisEndpointType>, string?> listener)
    {
        return null;
    }

    /// <summary>
    /// Generates rate quota endpoint options for a specific analysis endpoint type.
    /// </summary>
    /// <param name="endpointType">The type of analysis endpoint.</param>
    /// <param name="endpointOptions">The simulated endpoint options containing rate limits.</param>
    /// <returns>A new <see cref="RateQuotaEndpointOptions{TEndpointType}"/> configured with the specified limits.</returns>
    private static RateQuotaEndpointOptions<AnalysisEndpointType> GenerateRateQuotaEndpointOptions(
        AnalysisEndpointType endpointType,
        SimulatedEndpointOptions endpointOptions)
    {
        return new RateQuotaEndpointOptions<AnalysisEndpointType>
        {
            EndpointTypes = [endpointType],
            MinuteRate = endpointOptions.MinuteRateLimit,
            HourlyRate = endpointOptions.HourRateLimit,
            DailyQuota = endpointOptions.DailyUsageLimit,
            MonthlyQuota = endpointOptions.MonthlyUsageLimit,
        };
    }
}

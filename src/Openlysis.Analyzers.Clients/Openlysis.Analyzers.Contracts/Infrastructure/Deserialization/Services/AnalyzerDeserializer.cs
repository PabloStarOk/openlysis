using System.Text.Json;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Core.Common.Constants;
using Openlysis.Analyzers.Contracts.Core.Configuration;
using Openlysis.Analyzers.Contracts.Infrastructure.Deserialization.Abstractions;

namespace Openlysis.Analyzers.Contracts.Infrastructure.Deserialization.Services;

/// <summary>
/// Deserializer class for analyzing JSON responses.
/// </summary>
/// <typeparam name="TOptions">The type of options used for the analyzer.</typeparam>
public class AnalyzerDeserializer<TOptions> : IAnalyzerDeserializer
    where TOptions : AnalyzerOptions
{
    private readonly ILogger<AnalyzerDeserializer<TOptions>> _logger;
    private readonly IOptionsMonitor<AnalyzerOptions> _analyzerOptions;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzerDeserializer{TOptions}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging errors and information.</param>
    /// <param name="analyzerOptions">The options monitor for accessing analyzer options.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options for deserialization.</param>
    public AnalyzerDeserializer(
        ILogger<AnalyzerDeserializer<TOptions>> logger,
        IOptionsMonitor<AnalyzerOptions> analyzerOptions,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _logger = logger;
        _analyzerOptions = analyzerOptions;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    /// <inheritdoc/>
    public ErrorOr<TModel> Deserialize<TModel>(
        JsonElement rootElement)
        where TModel : class
    {
        TModel? model;
        try
        {
            model = rootElement.Deserialize<TModel>(_jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{ServiceName}: Exception caught while trying to deserialize a response of type {ResponseType}.\n\tJSON Content: {JsonContent}",
                _analyzerOptions.CurrentValue.ServiceName,
                typeof(TModel).Name,
                rootElement.GetRawText());
            return AnalyzerErrors.DeserializationFailure;
        }

        if (model is not null)
        {
            return model;
        }

        _logger.LogError(
            "{ServiceName}: Object of type {Type} was null after deserialization.",
            _analyzerOptions.CurrentValue.ServiceName,
            typeof(TModel).Name);
        return AnalyzerErrors.DeserializationNull;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<TModel>> DeserializeAsync<TModel>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        return Deserialize<TModel>(jsonDocument.RootElement);
    }
}
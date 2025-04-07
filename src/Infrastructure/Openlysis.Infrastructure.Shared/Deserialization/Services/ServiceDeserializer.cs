using System.Text.Json;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Configuration;
using Openlysis.Infrastructure.Shared.Constants;
using Openlysis.Infrastructure.Shared.Deserialization.Abstractions;

namespace Openlysis.Infrastructure.Shared.Deserialization.Services;

/// <summary>
/// Deserializer class for analyzing JSON responses.
/// </summary>
/// <typeparam name="TOptions">The type of service options used for the deserializer.</typeparam>
public class ServiceDeserializer<TOptions> : IServiceDeserializer
    where TOptions : ServiceOptions
{
    private readonly ILogger<ServiceDeserializer<TOptions>> _logger;
    private readonly IOptionsMonitor<TOptions> _serviceOptions;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDeserializer{TOptions}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging errors and information.</param>
    /// <param name="serviceOptions">The options monitor for accessing service options.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options for deserialization.</param>
    public ServiceDeserializer(
        ILogger<ServiceDeserializer<TOptions>> logger,
        IOptionsMonitor<TOptions> serviceOptions,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _logger = logger;
        _serviceOptions = serviceOptions;
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
                _serviceOptions.CurrentValue.ServiceName,
                typeof(TModel).Name,
                rootElement.GetRawText());
            return ServiceErrors.DeserializationFailure;
        }

        if (model is not null)
        {
            return model;
        }

        _logger.LogError(
            "{ServiceName}: Object of type {Type} was null after deserialization.",
            _serviceOptions.CurrentValue.ServiceName,
            typeof(TModel).Name);
        return ServiceErrors.DeserializationNull;
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
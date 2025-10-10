using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Configuration;

namespace Openlysis.Infrastructure.Shared.Communication.Services.Broker;

/// <summary>
/// Provides URIs of endpoints of <see cref="IConsumer{TMessage}"/>.
/// </summary>
internal sealed class EndpointUriProvider : IEndpointUriProvider, IDisposable
{
    /// <inheritdoc/>
    public Uri AnalyzeFileUri { get; private set; }

    /// <inheritdoc/>
    public Uri UpdateFileMultiAnalysisUri { get; private set; }

    /// <inheritdoc/>
    public Uri AnalyzeUrlUri { get; private set; }

    /// <inheritdoc/>
    public Uri UpdateUrlMultiAnalysisUri { get; private set; }

    /// <inheritdoc/>
    public Uri MessageAnalysisUpdateUri { get; private set; }

    private readonly IOptions<ConsumersOptions> _consumersOptions;
    private readonly IDisposable? _optionsListener;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointUriProvider"/> class.
    /// </summary>
    /// <param name="brokerOptions">An <see cref="IOptionsMonitor{TOptions}"/>.</param>
    /// <param name="consumersOptions">An <see cref="IOptions{TOptions}"/> for consumer options.</param>
    public EndpointUriProvider(
        IOptionsMonitor<BrokerSettings> brokerOptions,
        IOptions<ConsumersOptions> consumersOptions)
    {
        _consumersOptions = consumersOptions;
        OnOptionsChanged(brokerOptions.CurrentValue, null);
        _optionsListener = brokerOptions.OnChange(OnOptionsChanged);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Handles changes to the broker settings and updates the URIs accordingly.
    /// </summary>
    /// <param name="settings">The new broker settings.</param>
    /// <param name="s">A string parameter (not used).</param>
    private void OnOptionsChanged(BrokerSettings settings, string? s)
    {
        // Analyze file consumer.
        var consumersOptions = _consumersOptions.Value;
        var uriBuilder = new UriBuilder
        {
            Scheme = settings.Scheme,
            Host = settings.Host,
            Port = settings.Port,
            Path = Uri.EscapeDataString(consumersOptions.AnalyzeFile.Name),
        };
        AnalyzeFileUri = uriBuilder.Uri;

        // Update file multi analysis consumer.
        uriBuilder.Path = Uri.EscapeDataString(consumersOptions.UpdateFileAnalysis.Name);
        UpdateFileMultiAnalysisUri = uriBuilder.Uri;

        // Analyze URL consumer.
        uriBuilder.Path = Uri.EscapeDataString(consumersOptions.AnalyzeUrl.Name);
        AnalyzeUrlUri = uriBuilder.Uri;

        // Update URL multi analysis consumer.
        uriBuilder.Path = Uri.EscapeDataString(consumersOptions.UpdateUrlAnalysis.Name);
        UpdateUrlMultiAnalysisUri = uriBuilder.Uri;

        uriBuilder.Path = Uri.EscapeDataString(consumersOptions.MessageAnalysisUpdate.Name);
        MessageAnalysisUpdateUri = uriBuilder.Uri;
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing && _optionsListener is not null)
        {
            _optionsListener.Dispose();
        }

        _isDisposed = true;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="EndpointUriProvider"/> class.
    /// </summary>
    ~EndpointUriProvider()
    {
        Dispose(disposing: false);
    }
}
using System;

using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.MultiAnalyzer.Adapters.Broker.Files;
using Openlysis.MultiAnalyzer.Adapters.Broker.URLs;
using Openlysis.MultiAnalyzer.Common.Abstractions;
using Openlysis.MultiAnalyzer.Common.Configuration;
using Openlysis.MultiAnalyzer.Core.Broker.Files;
using Openlysis.MultiAnalyzer.Core.Broker.URLs;

namespace Openlysis.MultiAnalyzer.Infrastructure.Broker;

/// <summary>
/// Provides URIs of endpoints of <see cref="IConsumer{TMessage}"/>.
/// </summary>
public sealed class EndpointUriProvider : IEndpointUriProvider, IDisposable
{
    /// <inheritdoc/>
    public Uri AnalyzeFileUri { get; private set; }

    /// <inheritdoc/>
    public Uri UpdateMultiAnalysisUri { get; private set; }

    /// <inheritdoc/>
    public Uri AnalyzeUrlUri { get; private set; }

    /// <inheritdoc/>
    public Uri UpdateUrlMultiAnalysisUri { get; private set; }

    private readonly IDisposable _optionsListener;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointUriProvider"/> class.
    /// </summary>
    /// <param name="brokerOptions">An <see cref="IOptionsMonitor{TOptions}"/>.</param>
    public EndpointUriProvider(IOptionsMonitor<BrokerSettings> brokerOptions)
    {
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
    private void OnOptionsChanged(BrokerSettings settings, string s)
    {
        // Analyze file consumer.
        var uriBuilder = new UriBuilder
        {
            Scheme = settings.Scheme,
            Host = settings.Host,
            Port = settings.Port,
            Path = Uri.EscapeDataString(AnalyzeFileConsumer.EndpointName),
        };
        AnalyzeFileUri = uriBuilder.Uri;

        // Update file multi analysis consumer.
        uriBuilder.Path = Uri.EscapeDataString(UpdateFileMultiAnalysisConsumer.EndpointName);
        UpdateMultiAnalysisUri = uriBuilder.Uri;

        // Analyze URL consumer.
        uriBuilder.Path = Uri.EscapeDataString(AnalyzeUrlConsumer.EndpointName);
        AnalyzeUrlUri = uriBuilder.Uri;

        // Update URL multi analysis consumer.
        uriBuilder.Path = Uri.EscapeDataString(UpdateUrlMultiAnalysisConsumer.EndpointName);
        UpdateUrlMultiAnalysisUri = uriBuilder.Uri;
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
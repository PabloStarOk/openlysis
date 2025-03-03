using System;

using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.MultiAnalyzer.Core.Abstractions;
using Openlysis.MultiAnalyzer.Features.AnalyzeFile.Consumer;
using Openlysis.MultiAnalyzer.Features.UpdateFileMultiAnalysis.Consumer;
using Openlysis.MultiAnalyzer.Infrastructure.Configuration;

namespace Openlysis.MultiAnalyzer.Infrastructure.Services;

/// <summary>
/// Provides URIs of endpoints of <see cref="IConsumer{TMessage}"/>.
/// </summary>
public class EndpointUriProvider : IEndpointUriProvider, IDisposable
{
    /// <inheritdoc/>
    public Uri AnalyzeFileUri { get; private set; }

    /// <inheritdoc/>
    public Uri UpdateMultiAnalysisUri { get; private set; }

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
        var uriBuilder = new UriBuilder
        {
            Scheme = settings.Scheme,
            Host = settings.Host,
            Port = settings.Port,
            Path = Uri.EscapeDataString(AnalyzeFileConsumer.EndpointName),
        };
        AnalyzeFileUri = uriBuilder.Uri;

        uriBuilder.Path = Uri.EscapeDataString(UpdateFileMultiAnalysisConsumer.EndpointName);

        UpdateMultiAnalysisUri = uriBuilder.Uri;
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
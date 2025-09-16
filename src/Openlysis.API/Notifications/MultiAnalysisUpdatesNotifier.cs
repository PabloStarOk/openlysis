using Microsoft.AspNetCore.SignalR;

using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.API.Endpoints.URLs.Common;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Files;
using Openlysis.Domain.URLs;

namespace Openlysis.API.Notifications;

/// <summary>
/// Notifies clients about multi analysis updates via SignalR.
/// </summary>
/// <typeparam name="TMultiAnalysis">Type of the multi-analysis entity.</typeparam>
/// <typeparam name="TAnalysis">Type of the analysis entity.</typeparam>
internal sealed class MultiAnalysisUpdatesNotifier<TMultiAnalysis, TAnalysis>
    : IMultiAnalysisUpdatesNotifier<TMultiAnalysis, TAnalysis>
    where TMultiAnalysis : MultiAnalysis<TAnalysis>
    where TAnalysis : Analysis
{
    private readonly IHubContext<AnalysisUpdatesHub, IAnalysisUpdatesClient> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiAnalysisUpdatesNotifier{TMultiAnalysis, TAnalysis}"/> class.
    /// </summary>
    /// <param name="hubContext">
    /// The SignalR hub context used to notify clients about analysis updates.
    /// </param>
    public MultiAnalysisUpdatesNotifier(IHubContext<AnalysisUpdatesHub, IAnalysisUpdatesClient> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <inheritdoc/>
    public async Task NotifyAsync(TMultiAnalysis multiAnalysis, CancellationToken cancellationToken = default)
    {
        string userId = multiAnalysis.UserId.ToString();
        IAnalysisUpdatesClient client = _hubContext.Clients.User(userId);

        switch (multiAnalysis)
        {
            case FileMultiAnalysis fileMultiAnalysis:
                var fileMultiAnalysisDto = FileMultiAnalysisDto.Parse(fileMultiAnalysis);
                await client.ReceiveFileMultiAnalysisUpdate(fileMultiAnalysisDto);
                break;
            case UrlMultiAnalysis urlMultiAnalysis:
                var urlMultiAnalysisDto = UrlMultiAnalysisDto.Parse(urlMultiAnalysis);
                await client.ReceiveUrlMultiAnalysisUpdate(urlMultiAnalysisDto);
                break;
        }
    }
}
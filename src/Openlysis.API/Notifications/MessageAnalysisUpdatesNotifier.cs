using Microsoft.AspNetCore.SignalR;

using Openlysis.API.Endpoints.EmailAddresses.GetReputation;
using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.API.Endpoints.Messages.Common.Responses;
using Openlysis.API.Endpoints.Phones.GetReputation;
using Openlysis.API.Endpoints.URLs.Common;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Messages.Services;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.API.Notifications;

/// <summary>
/// Notifies clients about message analysis updates via SignalR.
/// </summary>
internal sealed class MessageAnalysisUpdatesNotifier : IMessageAnalysisUpdatesNotifier
{
    private readonly IHubContext<AnalysisUpdatesHub, IAnalysisUpdatesClient> _hubContext;
    private readonly IMessageAnalysisResultsProvider _resultsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisUpdatesNotifier"/> class.
    /// </summary>
    /// <param name="hubContext">SignalR hub context for sending analysis updates to clients.</param>
    /// <param name="resultsProvider">Provider for retrieving message analysis results.</param>
    public MessageAnalysisUpdatesNotifier(
        IHubContext<AnalysisUpdatesHub, IAnalysisUpdatesClient> hubContext,
        IMessageAnalysisResultsProvider resultsProvider)
    {
        _hubContext = hubContext;
        _resultsProvider = resultsProvider;
    }

    /// <inheritdoc/>
    public async Task NotifyAsync(MessageAnalysis analysis, CancellationToken cancellationToken = default)
    {
        string userId = analysis.UserId.ToString();
        IAnalysisUpdatesClient client = _hubContext.Clients.User(userId);
        var dto = await BuildMessageAnalysisDtoAsync(analysis, cancellationToken);

        switch (dto.MessageInformation.Type)
        {
            case MessageType.Sms:
                await client.ReceiveSmsAnalysisUpdate(dto);
                break;
            case MessageType.Email:
                await client.ReceiveEmailAnalysisUpdate(dto);
                break;
            default:
                throw new InvalidOperationException($"Unsupported message type: {dto.MessageInformation.Type}");
        }
    }

    private async Task<MessageAnalysisDto> BuildMessageAnalysisDtoAsync(
        MessageAnalysis analysis,
        CancellationToken cancellationToken)
    {
        var fileMultiAnalyses =
            await _resultsProvider.GetFileMultiAnalysesAsync(
                analysis,
                cancellationToken);
        var urlMultiAnalyses =
            await _resultsProvider.GetUrlMultiAnalysesAsync(
                analysis,
                cancellationToken);
        var emailMultiReputations =
            await _resultsProvider.GetEmailAddressesReputationsAsync(
                analysis,
                cancellationToken);
        var phoneMultiReputations =
            await _resultsProvider.GetPhoneNumbersReputationsAsync(
                analysis,
                cancellationToken);

        var results = new MessageAnalysisResults(
            fileMultiAnalyses.Select(FileMultiAnalysisDto.Parse),
            urlMultiAnalyses.Select(UrlMultiAnalysisDto.Parse),
            emailMultiReputations.Select(EmailAddressMultiReputationDto.Parse),
            phoneMultiReputations.Select(PhoneMultiReputationDto.Parse));

        return MessageAnalysisDto.Parse(analysis, results);
    }
}
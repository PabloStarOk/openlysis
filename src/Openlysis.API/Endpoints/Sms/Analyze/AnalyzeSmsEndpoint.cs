using System.Security.Claims;

using FastEndpoints;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Application.Messages.Services;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Enums;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.API.Endpoints.Sms.Analyze;

/// <summary>
/// Endpoint for analyzing SMS messages.
/// </summary>
/// <remarks>
/// This endpoint handles requests to analyze SMS messages by processing the input
/// and returning the ID or hash of a <see cref="MessageAnalysis"/>.
/// </remarks>
public class AnalyzeSmsEndpoint : Endpoint<AnalyzeSmsRequest, AnalyzeSmsResponse>
{
    private readonly IMessageAnalysisService _messageAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeSmsEndpoint"/> class.
    /// </summary>
    /// <param name="messageAnalysisService">The service responsible for analyzing SMS messages.</param>
    public AnalyzeSmsEndpoint(IMessageAnalysisService messageAnalysisService)
    {
        _messageAnalysisService = messageAnalysisService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("sms");
        Description(
            builder =>
            {
                builder.Accepts<AnalyzeSmsRequest>("multipart/form-data");
                builder.Produces<AnalyzeSmsResponse>(statusCode: 202);
            });
        DontThrowIfValidationFails();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AnalyzeSmsRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await SendResultAsync(ValidationFailures.AsValidationProblem());
            return;
        }

        string userId = HttpContext.User.Claims
            .Single(c => c.Type == ClaimTypes.NameIdentifier)
            .Value;

        var message = new Message(MessageType.Sms, req.Sender, null, req.Content);
        MessageAnalysis messageAnalysis = await _messageAnalysisService.AnalyzeAsync(
            userId: UserId.Create(Guid.Parse(userId)),
            isPrivate: req.IsPrivate,
            message: message,
            files: null,
            reanalyzeData: req.ReanalyzeData,
            requestCountryCode: req.NormalizedCountryCode,
            cancellationToken: ct);

        Response = AnalyzeSmsResponse.Parse(messageAnalysis);

        // TODO: Replace with name of GetByIdEndpoint.
        IResult acceptedResult = Results.Accepted(uri: null, value: Response);
        await SendResultAsync(acceptedResult);
    }
}
using System.Security.Claims;

using FastEndpoints;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.API.Endpoints.Sms.GetAnalysisById;
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
        Post(string.Empty);
        Group<SmsAnalysesGroup>();
        Version(1);
        Description(
            builder =>
            {
                builder.WithName("AnalyzeSms");
                builder.WithDisplayName("AnalyzeSms");
                builder.Accepts<AnalyzeSmsRequest>("multipart/form-data");
                builder.Produces<AnalyzeSmsResponse>(statusCode: 202);
                builder.ProducesValidationProblem();
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Analyze an SMS message.";
                s.Description = "Sends an SMS message to be analyzed.";
                s.RequestParam(r => r.Sender, "Sender of the SMS message.");
                s.RequestParam(r => r.Content, "Content of the SMS message.");
                s.RequestParam(r => r.IsPrivate, "If the analysis is only available to the user who sends the SMS message. Default is true");
                s.RequestParam(r => r.ReanalyzeData, "If the data that is detected in the SMS message, should be analyzed again even if there are existing analysis results. Default is false.");
                s.RequestParam(r => r.CountryCode, "A code of the country where detected phone numbers can be associated to, it must be in ISO 3166-1 alpha-2 format (e.g. 'US').");
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

        string userId = User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;

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

        var routeValues = new RouteValueDictionary
            {
                { "id", Response.Id },
            };
        IResult acceptedResult = Results.AcceptedAtRoute (
            GetAnalysisByIdEndpoint.Name,
            routeValues,
            Response);
        await SendResultAsync(acceptedResult);
    }
}
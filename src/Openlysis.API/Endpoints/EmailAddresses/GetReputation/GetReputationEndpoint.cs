using System.Net.Mail;

using ErrorOr;

using FastEndpoints;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.Application.EmailAddresses.Services;
using Openlysis.Domain.EmailAddresses;

namespace Openlysis.API.Endpoints.EmailAddresses.GetReputation;

/// <summary>
/// Represents an endpoint for retrieving the reputation of an email address.
/// </summary>
/// <remarks>
/// This endpoint handles requests to evaluate the reputation of a given email address
/// and returns the reputation details in the form of a DTO.
/// </remarks>
public class GetReputationEndpoint : Endpoint<GetReputationRequest, EmailAddressMultiReputationDto>
{
    private readonly IEmailAddressReputationService _reputationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReputationEndpoint"/> class.
    /// </summary>
    /// <param name="reputationService">
    /// The service used to retrieve the reputation of email addresses.
    /// </param>
    public GetReputationEndpoint(
        IEmailAddressReputationService reputationService)
    {
        _reputationService = reputationService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("email-addresses/{email-address}");
        Description(
            builder =>
            {
                builder.WithName("GetEmailAddressReputation");
                builder.WithDisplayName("GetEmailAddressReputation");
                builder.Accepts<GetReputationRequest>();
                builder.Produces<EmailAddressMultiReputationDto>();
                builder.ProducesValidationProblem();
                builder.ProducesProblem(statusCode: StatusCodes.Status503ServiceUnavailable);
                builder.ProducesProblem(statusCode: StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            endpointSummary =>
            {
                endpointSummary.Summary = "Gets reputation of an email address";
                endpointSummary.Description = "Gets reputation of an email address.";
                endpointSummary.ExampleRequest = new GetReputationRequest("jhon.doe@example.com");
                endpointSummary.RequestParam(r => r.EmailAddress, "An email address.");
            });
        DontThrowIfValidationFails();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetReputationRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await SendResultAsync(ValidationFailures.AsValidationProblem());
            return;
        }

        if (!_reputationService.IsAvailable)
        {
            IResult result = Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                detail: "Service to get reputation of an email address is unavailable, try again later.");
            await SendResultAsync(result);
        }

        var emailAddress = new MailAddress(req.NormalizedEmailAddress);
        ErrorOr<EmailAddressMultiReputation> evaluateResult =
            await _reputationService.GetAsync(
                emailAddress,
                storeInDatabase: false,
                ct);

        if (evaluateResult.IsError)
        {
            IResult internalError = Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "Internal error while getting reputation, try again later.");

            Logger.LogError(
                "An error occurred while getting reputation of an email address.\nErrors:\n\t{Errors}",
                evaluateResult.Errors);
            await SendResultAsync(internalError);
            return;
        }

        EmailAddressMultiReputation multiReputation = evaluateResult.Value;
        Response = EmailAddressMultiReputationDto.Parse(multiReputation);
    }
}
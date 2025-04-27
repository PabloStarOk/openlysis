using ErrorOr;

using FastEndpoints;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.Application.Phones.Contracts.Requests;
using Openlysis.Application.Phones.Services;
using Openlysis.Domain.Phones;

namespace Openlysis.API.Endpoints.Phones.GetReputation;

/// <summary>
/// Represents an endpoint for retrieving the reputation of a phone number.
/// </summary>
/// <remarks>
/// This endpoint handles requests to assess the reputation of a phone number
/// and returns a detailed reputation response.
/// </remarks>
public class GetReputationEndpoint : Endpoint<GetReputationRequest, PhoneMultiReputationDto>
{
    private readonly IPhoneReputationService _reputationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReputationEndpoint"/> class.
    /// </summary>
    /// <param name="reputationService">
    /// The service used to assess the reputation of phone numbers.
    /// </param>
    public GetReputationEndpoint(
        IPhoneReputationService reputationService)
    {
        _reputationService = reputationService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("phone-numbers/{phone-number}");
        Description(
            builder =>
            {
                builder.WithName("GetPhoneNumberReputation");
                builder.WithDisplayName("GetPhoneNumberReputation");
                builder.Accepts<GetReputationRequest>();
                builder.Produces<PhoneMultiReputationDto>();
                builder.ProducesValidationProblem();
                builder.ProducesProblem(statusCode: StatusCodes.Status503ServiceUnavailable);
                builder.ProducesProblem(statusCode: StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            endpointSummary =>
            {
                endpointSummary.Summary = "Gets reputation of a phone number";
                endpointSummary.Description = "Gets reputation of a phone number.";
                endpointSummary.ExampleRequest = new GetReputationRequest("+1 555 123 4567");
                endpointSummary.RequestParam(r => r.PhoneNumber, "A phone number in E.164 format.");
            });
        DontThrowIfValidationFails();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(
        GetReputationRequest req,
        CancellationToken ct)
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
                detail: "Service to get reputation of a phone number is unavailable, try again later.");
            await SendResultAsync(result);
        }

        var assessPhoneNumber = new EvaluatePhoneReputation(req.NormalizedPhoneNumber);
        ErrorOr<PhoneMultiReputation> assessResult =
            await _reputationService.AssessAsync(
                assessPhoneNumber,
                storeInDatabase: false,
                ct);

        if (assessResult.IsError)
        {
            IResult internalError = Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "Internal error while getting reputation, try again later.");

            Logger.LogError(
                "An error occurred while getting reputation of a phone number.\nErrors:\n\t{Errors}",
                assessResult.Errors);
            await SendResultAsync(internalError);
            return;
        }

        var multiReputation = assessResult.Value;
        Response = PhoneMultiReputationDto.Parse(multiReputation);
    }
}
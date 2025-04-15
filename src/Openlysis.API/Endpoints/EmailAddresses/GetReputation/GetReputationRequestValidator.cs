using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Endpoints.EmailAddresses.GetReputation;

/// <summary>
/// Validator for the <see cref="GetReputationRequest"/> class.
/// Ensures that the request contains valid email address.
/// </summary>
public class GetReputationRequestValidator : Validator<GetReputationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReputationRequestValidator"/> class.
    /// </summary>
    public GetReputationRequestValidator()
    {
        RuleFor(x => x.NormalizedEmailAddress)
            .NotEmpty()
            .EmailAddress()
            .WithErrorCode("InvalidEmailAddress")
            .WithMessage("Invalid email address, make sure email address complies with RFC 5322 standard.");
    }
}
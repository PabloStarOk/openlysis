using System.Text.RegularExpressions;

using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Endpoints.Phones.GetReputation;

/// <summary>
/// Validator for the <see cref="GetReputationRequest"/> class.
/// Ensures that the request contains valid data.
/// </summary>
public partial class GetReputationRequestValidator : Validator<GetReputationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReputationRequestValidator"/> class.
    /// Defines validation rules for the request.
    /// </summary>
    public GetReputationRequestValidator()
    {
        RuleFor(x => x.NormalizedPhoneNumber)
            .NotEmpty()
            .Matches(E164FormatRegex())
            .WithErrorCode("InvalidPhoneNumber")
            .WithMessage("Invalid phone number, make sure it is in international E.164 format.");
    }

    [GeneratedRegex(@"^\+[1-9]\d{1,14}$")]
    private static partial Regex E164FormatRegex();
}
using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Endpoints.Common.Requests;

/// <summary>
/// Validator for GetAnalysesByHashRequest.
/// </summary>
public class GetAnalysesByHashRequestValidator : Validator<GetAnalysesByHashRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalysesByHashRequestValidator"/> class.
    /// </summary>
    public GetAnalysesByHashRequestValidator()
    {
        RuleFor(x => x.Hash)
            .NotEmpty().WithMessage("Hash must not be empty.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(1).WithMessage("Amount must be greater or equals to 1.")
            .LessThanOrEqualTo(30).WithMessage("Amount must be less or equals to 30.");

        RuleFor(x => x.StartedDateOrder)
            .IsInEnum().WithMessage("StartedDateOrder must be 'asc' or 'dsc'.");
    }
}
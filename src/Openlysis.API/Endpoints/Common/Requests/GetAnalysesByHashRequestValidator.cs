using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Endpoints.Common.Requests;

/// <summary>
/// Validator for GetAnalysesByHashRequest.
/// </summary>
public class GetAnalysesByHashRequestValidator : Validator<GetAnalysesByHashRequest>
{
    /// <summary>
    /// The minimum allowed value for the page number.
    /// </summary>
    public const int MinPage = 1;

    /// <summary>
    /// The minimum allowed value for the page size.
    /// </summary>
    public const int MinPageSize = 1;

    /// <summary>
    /// The maximum allowed value for the page size.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalysesByHashRequestValidator"/> class.
    /// </summary>
    public GetAnalysesByHashRequestValidator()
    {
        RuleFor(x => x.Hash)
            .NotEmpty().WithMessage("Hash must not be empty.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(MinPage).WithMessage($"Page must be greater than or equal to {MinPage}.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(MinPageSize).WithMessage($"PageSize must be greater than or equal to {MinPageSize}.")
            .LessThanOrEqualTo(MaxPageSize).WithMessage($"PageSize must be less than or equal to {MaxPageSize}.");

        RuleFor(x => x.StartedDateOrder)
            .IsInEnum().WithMessage("StartedDateOrder must be 'asc' or 'dsc'.");
    }
}
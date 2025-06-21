using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Endpoints.Users.GetAnalyses;

/// <summary>
/// Validator for <see cref="GetAnalysesRequest"/>.
/// Ensures that Type is a valid enum value, Page is greater than 0, and Size is between 1 and 100.
/// </summary>
public class GetAnalysesRequestValidator : Validator<GetAnalysesRequest>
{
    /// <summary>
    /// The minimum allowed value for the Page property.
    /// </summary>
    public const int MinPage = 1;

    /// <summary>
    /// The minimum allowed value for the Size property.
    /// </summary>
    public const int MinPageSize = 1;

    /// <summary>
    /// The maximum allowed value for the Size property.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalysesRequestValidator"/> class.
    /// Sets up validation rules for Type, Page, and Size properties.
    /// </summary>
    public GetAnalysesRequestValidator()
    {
        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Continue)
            .NotNull().WithMessage("Type is required.")
            .IsInEnum().WithMessage("Type must be a valid value.");

        RuleFor(x => x.Page)
            .NotNull().WithMessage("Page is required.")
            .GreaterThanOrEqualTo(MinPage).WithMessage($"Page must be greater than or equal to {MinPage}.");

        RuleFor(x => x.PageSize)
            .NotNull().WithMessage("Page size is required.")
            .GreaterThanOrEqualTo(MinPageSize).WithMessage($"Size must be greater than or equal to {MinPageSize}.")
            .LessThanOrEqualTo(MaxPageSize).WithMessage($"Size must be less than or equal to {MaxPageSize}.");
    }
}
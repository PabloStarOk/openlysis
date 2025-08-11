using FluentValidation.Results;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Openlysis.API.Authentication.API.Extensions;

/// <summary>
/// Provides extension methods for handling validation failures.
/// </summary>
internal static class ValidationFailureExtensions
{
    /// <summary>
    /// Creates a <see cref="ValidationProblem"/> from a list of <see cref="ValidationFailure"/>.
    /// </summary>
    /// <param name="failures">The list of <see cref="ValidationFailure"/> containing the errors.</param>
    /// <returns>A <see cref="ValidationProblem"/> containing the validation errors.</returns>
    public static ValidationProblem AsValidationProblem(this List<ValidationFailure> failures)
    {
        IdentityError[] errors = failures.Select(e => new IdentityError
            {
                Code = e.ErrorCode,
                Description = e.ErrorMessage,
            }).ToArray();
        var errorResult = IdentityResult.Failed(errors);
        return errorResult.AsValidationProblem();
    }
}
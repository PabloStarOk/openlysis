using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Openlysis.API.Authentication.API.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IdentityResult"/> class.
/// </summary>
public static class IdentityResultExtensions
{
    /// <summary>
    /// Creates a <see cref="ValidationProblem"/> from an <see cref="IdentityResult"/>.
    /// </summary>
    /// <param name="result">The <see cref="IdentityResult"/> containing the errors.</param>
    /// <returns>A <see cref="ValidationProblem"/> containing the validation errors.</returns>
    public static ValidationProblem AsValidationProblem(this IdentityResult result)
    {
        Dictionary<string, string[]> errorDictionary = new (1);

        foreach (IdentityError error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out string[]? descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }
}
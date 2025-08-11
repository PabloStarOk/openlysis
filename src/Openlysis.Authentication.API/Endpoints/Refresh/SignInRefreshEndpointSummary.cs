using FastEndpoints;

using NodaTime;

using Openlysis.Authentication.API.Application.Common.Models;

namespace Openlysis.Authentication.API.Endpoints.Refresh;

/// <summary>
/// Provides a summary for the <see cref="SignInRefreshEndpoint"/>, including request parameter descriptions.
/// </summary>
internal sealed class SignInRefreshEndpointSummary
    : EndpointSummary<SignInRefreshRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignInRefreshEndpointSummary"/> class.
    /// </summary>
    public SignInRefreshEndpointSummary()
    {
        Summary = "Refreshes authentication tokens using a valid refresh token.";
        Description = "Accepts a refresh token and returns new authentication tokens if the refresh token is valid.";

        RequestParam(r => r.RefreshToken, "The refresh token to exchange for new authentication tokens.");

        ExampleRequest = new SignInRefreshRequest(
            RefreshToken: "dGhpc0lzQVRlc3RSZWZyZXNoVG9rZW5FeGFtcGxlMTIzNDU2Nzg5MA==");

        ResponseExamples[200] = new AuthTokens(
            AccessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
            RefreshToken: "dGhpc0lzQVRlc3RSZWZyZXNoVG9rZW5FeGFtcGxlMTIzNDU2Nzg5MA==",
            RefreshTokenExpiration: SystemClock.Instance.GetCurrentInstant());

        ResponseExamples[400] = Results.ValidationProblem(
            detail: "Validation failed. One or more errors occurred!",
            errors: new Dictionary<string, string[]>
            {
                { "RefreshToken", ["Refresh token is required."] },
            });

        Responses[200] = "Returns new access and refresh tokens if the provided refresh token is valid.";
        Responses[400] = "Validation error in the request. Returned when the refresh token is missing.";
        Responses[401] = "Invalid or expired refresh token. Returned when the refresh token is invalid or expired.";
    }
}
using FastEndpoints;

using Openlysis.Authentication.API.Application.Common.Models;

namespace Openlysis.Authentication.API.Endpoints.SignIn;

/// <summary>
/// Provides a summary for the <see cref="SignInEndpoint"/>, including request parameter descriptions.
/// </summary>
internal sealed class SignInEndpointSummary : EndpointSummary<SignInRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignInEndpointSummary"/> class.
    /// </summary>
    public SignInEndpointSummary()
    {
        Summary = "Authenticates a user and return authentication tokens.";
        Description = "Authenticates a user with the provided email and password, returning an access and refresh token if successful.";

        RequestParam(r => r.Email, "The email address of the user.");
        RequestParam(r => r.Password, "The password of the user.");

        ExampleRequest = new SignInRequest(
            Email: "new-user@example.com",
            Password: "StrongPassword1234$");

        ResponseExamples[200] = new AuthTokens(
            AccessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
            RefreshToken: "dGhpc0lzQVRlc3RSZWZyZXNoVG9rZW5FeGFtcGxlMTIzNDU2Nzg5MA==");

        ResponseExamples[400] = Results.ValidationProblem(
            detail: "One or more errors occurred!",
            errors: new Dictionary<string, string[]>
            {
                { "Email", ["Email is required."] },
                { "Password", ["Password is required."] },
            });

        Responses[200] = "A successful response with the token to access the analysis API (access token) and a token to get new authentication tokens (refresh token).";
        Responses[400] = "Validation error in the request. Returned when the email or password is missing or invalid.";
        Responses[401] = "Invalid email or password. Returned when authentication fails.";
    }
}
using FastEndpoints;

namespace Openlysis.Authentication.API.Endpoints.SignUp;

/// <summary>
/// Provides a summary for the <see cref="SignUpEndpoint"/>, including request parameter descriptions.
/// </summary>
internal sealed class SignUpEndpointSummary : EndpointSummary<SignUpRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignUpEndpointSummary"/> class.
    /// </summary>
    public SignUpEndpointSummary()
    {
        Summary = "Register a new user account.";
        Description = "Creates a new user account with the provided email and password.";

        RequestParam(r => r.Email, "The email address for the new user.");
        RequestParam(r => r.Password, "The password for the new user.");

        ExampleRequest = new SignUpRequest(
            Email: "new-user@example.com",
            Password: "StrongPassword1234$");

        ResponseExamples[400] = Results.ValidationProblem(
            detail: "One or more errors occurred!",
            errors: new Dictionary<string, string[]>
            {
                { "Email", ["Email is required."] },
                { "Password", ["Password is required."] },
            });

        Responses[204] = "User account created successfully.";
        Responses[400] = "Validation error in the request. Returned when the email or password is missing or invalid.";
    }
}
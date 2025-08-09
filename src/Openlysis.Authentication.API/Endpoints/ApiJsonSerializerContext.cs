using System.Text.Json.Serialization;

using FastEndpoints;

using Openlysis.Authentication.API.Endpoints.SignIn;
using Openlysis.Authentication.API.Endpoints.SignUp;

using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Openlysis.Authentication.API.Endpoints;

/// <summary>
/// Source generation context for System.Text.Json serialization.
/// Registers types used in API endpoints for optimized serialization.
/// </summary>
[JsonSerializable(typeof(SignUpRequest))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(SignInRequest))]
[JsonSerializable(typeof(SignInResponse))]
[JsonSerializable(typeof(ProblemDetails))]
internal partial class ApiJsonSerializerContext : JsonSerializerContext
{
}
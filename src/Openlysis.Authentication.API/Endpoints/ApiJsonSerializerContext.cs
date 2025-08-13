using System.Text.Json.Serialization;

using FastEndpoints;

using Microsoft.IdentityModel.Tokens;

using Openlysis.Authentication.API.Application.Common.Models;
using Openlysis.Authentication.API.Endpoints.OpenID;
using Openlysis.Authentication.API.Endpoints.Refresh;
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
[JsonSerializable(typeof(AuthTokens))]
[JsonSerializable(typeof(SignInRefreshRequest))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
[JsonSerializable(typeof(OpenIdConfiguration))]
[JsonSerializable(typeof(JsonWebKeySet))]
internal partial class ApiJsonSerializerContext : JsonSerializerContext
{
}
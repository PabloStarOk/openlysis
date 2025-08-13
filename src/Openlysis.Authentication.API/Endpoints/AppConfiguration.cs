using FastEndpoints;
using FastEndpoints.Swagger;

using FluentValidation.Results;

using Openlysis.Authentication.API.Endpoints.OpenID;

using Scalar.AspNetCore;

namespace Openlysis.Authentication.API.Endpoints;

/// <summary>
/// Provides extension methods for configuring the API endpoints.
/// </summary>
internal static class AppConfiguration
{
    private const string DocumentationGenerationPath = "/openapi/{documentName}.json";
    private const string ApiDocumentationPath = "api-docs";
    private const string WebPageTitle = "Openlysis Authentication API";
    private const string EndpointNameSuffix = "Endpoint";
    private const string EndpointPathPrefix = "api";
    private const string VersioningPrefix = "v";
    private const int EndpointDefaultVersion = 1;
    private const string HealthCheckEndpointPath = "health";

    /// <summary>
    /// Configures FastEndpoints for the given <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    public static void ConfigureApi(this WebApplication app)
    {
        app.UseFastEndpoints(o =>
        {
            o.Serializer.Options.TypeInfoResolver =
                ApiJsonSerializerContext.Default;

            o.Endpoints.ShortNames = true;
            o.Endpoints.NameGenerator = context
                => context.EndpointType.Name.TrimEnd(EndpointNameSuffix.ToCharArray());
            o.Endpoints.RoutePrefix = EndpointPathPrefix;

            o.Versioning.DefaultVersion = EndpointDefaultVersion;
            o.Versioning.Prefix = VersioningPrefix;
            o.Versioning.PrependToRoute = true;

            o.Errors.ResponseBuilder = BuildValidationFailureResponse;
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerGen(o => o.Path = DocumentationGenerationPath);

            app.MapScalarApiReference(ApiDocumentationPath, o =>
            {
                o.WithTitle(WebPageTitle);
                o.HiddenClients = true;

                o.AddDocument(DependencyInjection.V1DocumentName);
            });
        }

        app.UseExceptionHandler();
        app.MapHealthChecks(HealthCheckEndpointPath);
        app.MapOpenIdConfigurationEndpoint();
        app.MapJsonWebKeysEndpoint(app.Services);
    }

    private static object BuildValidationFailureResponse(
        List<ValidationFailure> failures,
        HttpContext context,
        int statusCode)
    {
        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                f => f.Key,
                f => f.Select(g => g.ErrorMessage).ToArray());

        var validationProblem = TypedResults.ValidationProblem(
            detail: "Your request could not be processed due to one or more validation errors. Please review the errors and update your request accordingly.",
            errors: errors);
        return validationProblem.ProblemDetails;
    }
}
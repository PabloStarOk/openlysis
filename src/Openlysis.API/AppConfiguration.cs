using System.Text.Json;
using System.Text.Json.Serialization;

using FastEndpoints;
using FastEndpoints.Swagger;

using FluentValidation.Results;

using Openlysis.Application.Common.Enums;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Messages.Enums;

using Scalar.AspNetCore;

namespace Openlysis.API;

/// <summary>
/// Configuration of the API for the web application.
/// </summary>
public static class AppConfiguration
{
    private const string DocumentationGenerationPath = "/openapi/{documentName}.json";
    private const string ApiDocumentationPath = "api-docs";
    private const string WebPageTitle = "Openlysis Authentication API";
    private const string EndpointNameSuffix = "Endpoint";
    private const string EndpointPathPrefix = "api";
    private const string VersioningPrefix = "v";
    private const int EndpointDefaultVersion = 1;

    /// <summary>
    /// Configures the API for the web application.
    /// </summary>
    /// <param name="app">The WebApplication instance to configure.</param>
    public static void ConfigureApi(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerGen(o => o.Path = DocumentationGenerationPath);

            app.MapScalarApiReference(ApiDocumentationPath, o =>
            {
                o.WithTitle(WebPageTitle);
                o.AddDocument(DependencyInjection.V1DocumentName);
            });
        }

        app.UseAuthentication().UseAuthorization();
        app.UseFastEndpoints(
            c =>
            {
                c.Endpoints.RoutePrefix = EndpointPathPrefix;
                c.Endpoints.NameGenerator = context
                    => context.EndpointType.Name.TrimEnd(EndpointNameSuffix.ToCharArray());

                c.Versioning.Prefix = VersioningPrefix;
                c.Versioning.DefaultVersion = EndpointDefaultVersion;
                c.Versioning.PrependToRoute = true;

                c.Serializer.Options.Converters.Add(new JsonStringEnumConverter<AnalysisStatus>(JsonNamingPolicy.CamelCase));
                c.Serializer.Options.Converters.Add(new JsonStringEnumConverter<Verdict>(JsonNamingPolicy.CamelCase));
                c.Serializer.Options.Converters.Add(new JsonStringEnumConverter<ThreatZone>(JsonNamingPolicy.CamelCase));
                c.Serializer.Options.Converters.Add(new JsonStringEnumConverter<OrderType>(JsonNamingPolicy.CamelCase));
                c.Serializer.Options.Converters.Add(new JsonStringEnumConverter<MessageType>(JsonNamingPolicy.CamelCase));

                c.Errors.ResponseBuilder = BuildValidationFailureResponse;
            });
    }

    private static HttpValidationProblemDetails BuildValidationFailureResponse(
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
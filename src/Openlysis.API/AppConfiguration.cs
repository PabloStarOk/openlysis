using System.Text.Json;
using System.Text.Json.Serialization;

using FastEndpoints;

using FluentValidation.Results;

using Openlysis.Application.Common.Enums;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.API;

/// <summary>
/// Configuration of the API for the web application.
/// </summary>
public static class AppConfiguration
{
    /// <summary>
    /// Configures the API for the web application.
    /// </summary>
    /// <param name="app">The WebApplication instance to configure.</param>
    public static void ConfigureApi(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi(c => c.DocExpansion = "list");
        }

        app.UseAuthentication().UseAuthorization();
        app.UseFastEndpoints(
            c =>
            {
                c.Endpoints.RoutePrefix = "api";

                c.Versioning.Prefix = "v";
                c.Versioning.DefaultVersion = 1;
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
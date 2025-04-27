using System.Text.Json;
using System.Text.Json.Serialization;

using FastEndpoints;

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

        app.UseAuthentication().UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi(c => c.DocExpansion = "list");
        }

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
            });
    }
}
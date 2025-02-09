using FastEndpoints;

namespace Openlysis.API;

public static class AppConfiguration
{
    public static void ConfigureApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi(c => c.DocExpansion = "list");
        }

        app.UseFastEndpoints();
    }
}
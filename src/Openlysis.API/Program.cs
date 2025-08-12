using Openlysis.API;
using Openlysis.Application;
using Openlysis.Infrastructure;
using Openlysis.Infrastructure.Shared.Infrastructure.ConfigLoader;

var builder = WebApplication.CreateSlimBuilder();

builder.Configuration.UseConfigLoader();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddApi(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.ConfigureApi();

await app.RunAsync();
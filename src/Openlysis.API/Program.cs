using Filescan.Client;

using Openlysis.API;
using Openlysis.Application;
using Openlysis.Infrastructure;

var builder = WebApplication.CreateSlimBuilder();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddApi();
builder.Services.AddHttpClient();
builder.Services.AddFilescanIoAnalyzer();

var app = builder.Build();

app.UseHttpsRedirection();
app.ConfigureApi();

await app.RunAsync();
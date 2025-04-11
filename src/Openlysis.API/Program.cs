using Openlysis.API;
using Openlysis.Application;
using Openlysis.Infrastructure;
using Openlysis.MultiAnalyzer.Adapters;

var builder = WebApplication.CreateSlimBuilder();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAnalysisWorker(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddApi(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseHttpsRedirection();
app.ConfigureApi();

await app.RunAsync();
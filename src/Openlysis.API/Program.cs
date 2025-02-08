using FastEndpoints;
using FastEndpoints.Swagger;

using Openlysis.API;
using Openlysis.Application;
using Openlysis.Infrastructure;

WebApplicationOptions options = new ();
var builder = WebApplication.CreateSlimBuilder(options);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseHttpsRedirection();
app.UseFastEndpoints();

await app.RunAsync();
using Openlysis.Authentication.API.Application;
using Openlysis.Authentication.API.Endpoints;
using Openlysis.Authentication.API.Infrastructure;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddApplication();
builder.Services.AddApi(builder.Configuration);
var app = builder.Build();
app.ConfigureApi();
await app.RunAsync();
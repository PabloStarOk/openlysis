using Microsoft.AspNetCore.Mvc;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses.ValueObjects;
using Openlysis.Infrastructure;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet(
    "/api/analyses/file/{id}", ([FromServices] IFileAnalysisRepository repository, string id) =>
    {
        var guid = Guid.Parse(id);
        var fileAnalysisId = FileAnalysisId.Create(guid);
        return repository.GetByIdAsync(fileAnalysisId);
    })
    .WithName("GetFileById")
    .WithOpenApi();

app.MapGet(
    "/api/analyses/file/hash/{sha256}", ([FromServices] IFileAnalysisRepository repository, string sha256) =>
    {
        var hashSet = new HashSet("MD5", "SHA1", sha256, "SHA512");
        return repository.GetByHashAsync(hashSet);
    })
    .WithName("GetFileByHashSha256")
    .WithOpenApi();

await app.RunAsync();
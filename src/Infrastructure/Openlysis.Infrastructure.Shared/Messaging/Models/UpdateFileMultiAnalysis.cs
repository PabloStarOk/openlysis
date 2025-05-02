using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;

namespace Openlysis.Infrastructure.Shared.Messaging.Models;

/// <summary>
/// Represents a request to update the service file analyses of file's multi analysis.
/// </summary>
/// <param name="Id">The unique identifier for the file multi-analysis.</param>
/// <param name="ServiceFileAnalyses">An array of service file analyses associated with the update.</param>
public record UpdateFileMultiAnalysis(
    GlobalId Id,
    FileServiceAnalysis[] ServiceFileAnalyses);
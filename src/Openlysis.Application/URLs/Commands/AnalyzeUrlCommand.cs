using ErrorOr;

using MediatR;

using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.URLs.Commands;

/// <summary>
/// Command to analyze a URL.
/// </summary>
/// <param name="Url">The URL to be analyzed.</param>
/// <param name="UserId">The ID of the user requesting the analysis.</param>
/// <param name="IsPrivate">Indicates whether the analysis is private.</param>
public record AnalyzeUrlCommand(
    Uri Url,
    UserId UserId,
    bool IsPrivate)
    : IRequest<ErrorOr<UrlMultiAnalysis>>;
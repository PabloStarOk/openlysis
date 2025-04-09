using Openlysis.Infrastructure.Shared.Configuration;

namespace Openlysis.Assessors.Shared.Configuration;

/// <summary>
/// Defines base options required to configure an assessor.
/// </summary>
/// <remarks>
/// It must be implemented by concrete assessor services to know the name of the configuration section.
/// </remarks>
public abstract record DataAssessorOptions : ServiceOptions;
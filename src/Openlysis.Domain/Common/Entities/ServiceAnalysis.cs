using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Common.Entities;

/// <summary>
/// Defines a base entity to store an analysis from an external service.
/// </summary>
public abstract class ServiceAnalysis : Entity<ComposedServiceAnalysisId>
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the current status of the analysis.
    /// </summary>
    public AnalysisStatus Status { get; private set; }

    /// <summary>
    /// Gets the error message if the analysis failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the service analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="status">The current status of the analysis.</param>
    /// <param name="error">The error message if the analysis failed.</param>
    protected ServiceAnalysis(
        ComposedServiceAnalysisId id,
        string serviceName,
        AnalysisStatus status,
        string? error)
        : base(id)
    {
        ServiceName = serviceName;
        Status = status;
        Error = error;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceAnalysis"/> class for EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor is required by EF Core and should not be used directly in application code.
    /// </remarks>
    protected ServiceAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Updates the status of the analysis.
    /// </summary>
    /// <param name="newStatus">The new status to set.</param>
    public void UpdateStatus(AnalysisStatus newStatus)
    {
        ArgumentNullException.ThrowIfNull(newStatus);
        Status = newStatus;
    }
}
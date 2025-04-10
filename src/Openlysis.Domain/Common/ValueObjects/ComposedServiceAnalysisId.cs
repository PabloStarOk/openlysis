namespace Openlysis.Domain.Common.ValueObjects;

/// <summary>
/// Represents a composed service analysis identifier which contains the ID of the analysis
/// and the ID of an optional job when required.
/// </summary>
public record ComposedServiceAnalysisId
{
    /// <summary>
    /// The character used to separate the primary ID and the job ID in a composed service analysis identifier.
    /// </summary>
    private const char IdCharSeparator = ':';

    /// <summary>
    /// Gets the service analysis identifier.
    /// </summary>
    public ServiceAnalysisId Primary { get; init; }

    /// <summary>
    /// Gets an optional job identifier.
    /// </summary>
    public string? Job { get; init; } = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComposedServiceAnalysisId"/> class.
    /// </summary>
    /// <param name="primary">The service analysis identifier.</param>
    /// <param name="job">The job identifier.</param>
    private ComposedServiceAnalysisId(ServiceAnalysisId primary, string? job = null)
    {
        Primary = primary;
        Job = job;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ComposedServiceAnalysisId"/> record.
    /// </summary>
    /// <param name="id">The service analysis identifier as a string.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>A new instance of the <see cref="ComposedServiceAnalysisId"/> record.</returns>
    public static ComposedServiceAnalysisId Create(string id, string? jobId = null)
    {
        return new ComposedServiceAnalysisId(
            ServiceAnalysisId.Create(id),
            jobId);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ComposedServiceAnalysisId"/> record from a composed identifier string.
    /// </summary>
    /// <param name="composedId">The composed identifier string containing the primary ID and job ID separated by a colon.</param>
    /// <returns>A new instance of the <see cref="ComposedServiceAnalysisId"/> record.</returns>
    /// <exception cref="ArgumentException">Thrown when the composed identifier string is not valid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the composed identifier string does not contain exactly two values separated by a colon.</exception>
    public static ComposedServiceAnalysisId Create(string composedId)
    {
        if (!composedId.Contains(IdCharSeparator))
        {
            throw new ArgumentException("Given composed id is not valid.", nameof(composedId));
        }

        string[] values = composedId.Split(IdCharSeparator);

        if (values is { Length: > 2 or < 2 })
        {
            throw new InvalidOperationException($"Composed ID contains less or more than two values separated by {IdCharSeparator}");
        }

        string? jobId = string.IsNullOrWhiteSpace(values[1]) ? null : values[1];
        return Create(values[0], jobId);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Primary.Value}{IdCharSeparator}{Job}";
    }
}
namespace Openlysis.Domain.Common.ValueObjects;

/// <summary>
/// Represents a composed service analysis identifier which contains the ID of the analysis
/// and the ID of an optional job when required.
/// </summary>
public record ComposedServiceAnalysisId
{
    private readonly string? _job;

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
    public string? Job
    {
        get => _job;
        init
        {
            _job = string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

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
    public static ComposedServiceAnalysisId Create(
        string id,
        string? jobId = null)
    {
        return new ComposedServiceAnalysisId(
            ServiceAnalysisId.Create(id),
            jobId);
    }

    /// <summary>
    /// Parses a composed service analysis identifier from its string representation.
    /// </summary>
    /// <param name="input">
    /// The string representation of the composed service analysis identifier.
    /// It can contain a single ID or two IDs separated by the defined separator character.
    /// </param>
    /// <returns>
    /// A new instance of the <see cref="ComposedServiceAnalysisId"/> record created from the input string.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the input string contains more than two values separated by the defined separator character.
    /// </exception>
    /// <remarks>
    /// The input string must follow the format: "PrimaryID[:JobID]".
    /// </remarks>
    public static ComposedServiceAnalysisId Parse(string input)
    {
        string[] values = input.Split(IdCharSeparator);
        if (values.Length > 2)
        {
            throw new InvalidOperationException($"Composed ID contains more than two values separated by {IdCharSeparator}");
        }

        string? jobId = values.Length is 2 ? values[1] : null;

        return Create(values[0], jobId);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Job is null
            ? Primary.Value
            : $"{Primary.Value}{IdCharSeparator}{Job}";
    }
}
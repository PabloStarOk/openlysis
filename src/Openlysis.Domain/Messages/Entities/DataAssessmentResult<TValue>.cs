using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.Domain.Messages.Entities;

/// <summary>
/// Represents the result of an analysis or reputation for a piece of data like email address, phone number, URL or file.
/// </summary>
/// <typeparam name="TValue">The type of the value contained in the result. Must be non-nullable.</typeparam>
public sealed class DataAssessmentResult<TValue>
    : Entity<GlobalId>
    where TValue : notnull
{
    /// <summary>
    /// Gets the type of the data used in the analysis or reputation result.
    /// </summary>
    public DataType Type { get; }

    /// <summary>
    /// Gets the value of the data used in the analysis or reputation result.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Gets the unique identifier for the result associated with this data.
    /// </summary>
    public GlobalId ResultId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataAssessmentResult{TValue}"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="type">The type of the data used in the analysis or reputation result.</param>
    /// <param name="value">The value of the data used in the analysis or reputation result.</param>
    /// <param name="resultId">The unique identifier for the result associated with this data.</param>
    private DataAssessmentResult(
        GlobalId id,
        DataType type,
        TValue value,
        GlobalId resultId)
        : base(id)
    {
        Type = type;
        Value = value;
        ResultId = resultId;
    }

    // For EF Core.
#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="DataAssessmentResult{TValue}"/> class for EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor is required by EF Core and should not be used directly in application code.
    /// </remarks>
    private DataAssessmentResult()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="DataAssessmentResult{TValue}"/> class.
    /// </summary>
    /// <param name="type">The type of the data.</param>
    /// <param name="value">The value of the data.</param>
    /// <param name="resultId">The unique identifier for the result.</param>
    /// <returns>A new instance of <see cref="DataAssessmentResult{TValue}"/>.</returns>
    public static DataAssessmentResult<TValue> Create(
        DataType type,
        TValue value,
        GlobalId resultId)
    {
        GlobalId id = GlobalId.CreateUnique();
        return new DataAssessmentResult<TValue>(
            id,
            type,
            value,
            resultId);
    }
}
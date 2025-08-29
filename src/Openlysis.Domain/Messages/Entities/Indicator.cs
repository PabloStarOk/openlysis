using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.Domain.Messages.Entities;

/// <summary>
/// Represents an indicator entity containing analysis data.
/// </summary>
public sealed class Indicator : Entity<GlobalId>
{
    /// <summary>
    /// Gets the type of the indicator data.
    /// </summary>
    public DataType Type { get; }

    /// <summary>
    /// Gets the value of the indicator.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the unique identifier for the analysis result.
    /// </summary>
    public GlobalId ResultId { get; }

    /// <summary>
    /// Gets the current analysis state of the indicator.
    /// </summary>
    public AnalysisState State { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Indicator"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the indicator.</param>
    /// <param name="type">The type of the indicator data.</param>
    /// <param name="value">The value of the indicator.</param>
    /// <param name="resultId">The unique identifier for the analysis result.</param>
    /// <param name="state">The current analysis state of the indicator.</param>
    private Indicator(
        GlobalId id,
        DataType type,
        string value,
        GlobalId resultId,
        AnalysisState state)
        : base(id)
    {
        Type = type;
        Value = value;
        ResultId = resultId;
        State = state;
    }

    // For EF Core.
#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="Indicator"/> class for EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor is required by EF Core and should not be used directly in application code.
    /// </remarks>
    private Indicator()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new <see cref="Indicator"/> instance with the specified type, value, result identifier, verdict, and analysis status.
    /// </summary>
    /// <param name="type">The type of the indicator data.</param>
    /// <param name="value">The value of the data associated to the indicator.</param>
    /// <param name="resultId">The unique identifier for the analysis result.</param>
    /// <param name="verdict">The verdict to associate with the indicator's analysis state.</param>
    /// <param name="status">The initial analysis status. Defaults to <see cref="AnalysisStatus.Queued"/>.</param>
    /// <returns>A new <see cref="Indicator"/> object.</returns>
    internal static Indicator Create(
        DataType type,
        string value,
        GlobalId resultId,
        Verdict verdict,
        AnalysisStatus status = AnalysisStatus.Queued)
    {
        GlobalId id = GlobalId.CreateUnique();
        AnalysisState initialState = AnalysisState.Initial()
            .WithVerdict(verdict)
            .WithStatus(status);
        return new Indicator(
            id,
            type,
            value,
            resultId,
            initialState);
    }

    /// <summary>
    /// Updates the verdict of the indicator's analysis state.
    /// </summary>
    /// <param name="verdict">The verdict to set.</param>
    internal void UpdateVerdict(Verdict verdict)
    {
        State = State.WithVerdict(verdict);
    }

    /// <summary>
    /// Updates the status of the indicator's analysis state.
    /// </summary>
    /// <param name="status">The status to set.</param>
    internal void UpdateStatus(AnalysisStatus status)
    {
        State = State.WithStatus(status);
    }
}
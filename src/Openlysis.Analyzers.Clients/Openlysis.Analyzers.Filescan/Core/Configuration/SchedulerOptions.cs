namespace Openlysis.Analyzers.Filescan.Core.Configuration;

/// <summary>
/// Options for the scheduler of Filescan analyzer.
/// </summary>
public class SchedulerOptions
{
    /// <summary>
    /// The section name in the configuration.
    /// </summary>
    public const string SectionName = "Filescan:Scheduler";

    /// <summary>
    /// Gets the identifier of the scheduler.
    /// </summary>
    required public string Id { get; init;  }

    /// <summary>
    /// Gets the name of the scheduler.
    /// </summary>
    required public string Name { get; init; }
}
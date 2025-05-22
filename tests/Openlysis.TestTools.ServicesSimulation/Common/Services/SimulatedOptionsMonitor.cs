using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.Common.Services;

/// <summary>
/// Provides a simulated implementation of <see cref="IOptionsMonitor{TOptions}"/> for testing purposes.
/// This class allows injecting fixed option values without requiring the full options configuration system.
/// </summary>
/// <typeparam name="TOptions">The options type being monitored.</typeparam>
internal sealed class SimulatedOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
    where TOptions : class
{
    private readonly Dictionary<string, TOptions>? _namedOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedOptionsMonitor{TOptions}"/> class.
    /// </summary>
    /// <param name="initialValue">The initial value for the options.</param>
    internal SimulatedOptionsMonitor(TOptions initialValue) => CurrentValue = initialValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedOptionsMonitor{TOptions}"/> class with named options.
    /// </summary>
    /// <param name="namedOptions">A dictionary containing named option instances.</param>
    /// <exception cref="ArgumentException">Thrown when the named options dictionary is empty.</exception>
    internal SimulatedOptionsMonitor(Dictionary<string, TOptions> namedOptions)
    {
        if (namedOptions.Count is 0)
        {
            throw new ArgumentException("Named options dictionary must contain at least one value.", nameof(namedOptions));
        }

        _namedOptions = namedOptions;
        CurrentValue = _namedOptions.Values.First();
    }

    /// <inheritdoc/>
    public TOptions Get(string? name)
    {
        return _namedOptions is null || name is null
            ? CurrentValue
            : _namedOptions.GetValueOrDefault(name, CurrentValue);
    }

    /// <inheritdoc/>
    public TOptions CurrentValue { get; }

    /// <inheritdoc/>
    public IDisposable? OnChange(Action<TOptions, string> listener)
    {
        return null;
    }
}
using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Enums;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Enums;

namespace Openlysis.TestTools.ServicesSimulation.Common.Services.Abstractions;

/// <summary>
/// An abstract factory class for creating stub instances based on configurable options.
/// </summary>
/// <typeparam name="TOptions">The type of configuration options for the stub factory.</typeparam>
/// <typeparam name="TStub">The type of stub to create.</typeparam>
internal abstract class StubFactory<TOptions, TStub>
    where TStub : notnull
    where TOptions : StubFactoryOptions
{
    /// <summary>
    /// Gets the logger instance used for logging diagnostic information about the stub factory's operations.
    /// </summary>
    protected ILogger<StubFactory<TOptions, TStub>> Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StubFactory{TOptions, TStub}"/> class.
    /// </summary>
    /// <param name="logger">The logger used for logging information and errors during stub creation.</param>
    protected StubFactory(ILogger<StubFactory<TOptions, TStub>> logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Creates a new stub instance based on the provided service name and options.
    /// </summary>
    /// <param name="serviceName">The name of the service to create a stub for.</param>
    /// <param name="options">Configuration options that control how the stub is created.</param>
    /// <returns>A configured stub instance of type <typeparamref name="TStub"/>.</returns>
    internal TStub Create(string serviceName, TOptions options)
    {
        TStub stub = HandleCreation(serviceName, options);
        LogCreatedStub(stub);
        return stub;
    }

    /// <summary>
    /// Generates a verdict value based on the provided verdict options.
    /// </summary>
    /// <param name="verdictOptions">The options that define how the verdict should be generated.</param>
    /// <returns>A verdict value generated according to the simulation type specified in the options.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unsupported simulation type is specified.</exception>
    protected static Verdict GenerateVerdict(VerdictOptions verdictOptions)
    {
        return verdictOptions.SimulationType switch
        {
            SimulationType.Random => GetRandomValue(Enum.GetValues<Verdict>()),
            SimulationType.Fixed => verdictOptions.FixedValue,
            SimulationType.Range => throw new InvalidOperationException("Range of values is not supported for enum simulation."),
            SimulationType.Set => GetRandomValue(verdictOptions.ValuesSet.ToList()),
            _ => throw new InvalidOperationException($"Unsupported value for {nameof(verdictOptions.SimulationType)}"),
        };
    }

    /// <summary>
    /// Gets a random value from the provided list of possible values.
    /// </summary>
    /// <typeparam name="TValue">The type of values in the collection.</typeparam>
    /// <param name="possibleValues">A read-only list of possible values to choose from.</param>
    /// <returns>A randomly selected value from the provided collection.</returns>
    /// <exception cref="ArgumentException">Thrown when the collection is empty.</exception>
    protected static TValue GetRandomValue<TValue>(IReadOnlyList<TValue> possibleValues)
    {
        int randomIndex = Random.Shared.Next(0, possibleValues.Count);
        return possibleValues[randomIndex];
    }

    /// <summary>
    /// Handles the creation of a specific stub instance for the given service.
    /// </summary>
    /// <param name="serviceName">The name of the service to create a stub for.</param>
    /// <param name="options">Configuration options that control how the stub is created.</param>
    /// <returns>A configured stub instance of type <typeparamref name="TStub"/>.</returns>
    protected abstract TStub HandleCreation(string serviceName, TOptions options);

    /// <summary>
    /// Performs additional debug actions after a stub has been created.
    /// This method should be implemented by derived classes to provide
    /// debugging information or additional validation for created stubs.
    /// </summary>
    /// <param name="stub">The created stub instance that needs to be debugged.</param>
    /// <remarks>This method is only available in DEBUG builds.</remarks>
    protected abstract void LogCreatedStub(TStub stub);
}
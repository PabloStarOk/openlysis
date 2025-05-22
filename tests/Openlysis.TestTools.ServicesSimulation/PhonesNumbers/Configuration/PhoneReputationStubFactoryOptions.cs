using Openlysis.Domain.Phones.ValueObjects;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.PhonesNumbers.Configuration;

/// <summary>
/// Configuration options for the phone reputation stub factory.
/// </summary>
internal sealed record PhoneReputationStubFactoryOptions : StubFactoryOptions
{
    /// <summary>
    /// Gets or initializes the stubbed phone information.
    /// When set, this value will be used instead of filling the stub with random information.
    /// </summary>
    public PhoneInfo? PhoneInfoStub { get; init; }
}
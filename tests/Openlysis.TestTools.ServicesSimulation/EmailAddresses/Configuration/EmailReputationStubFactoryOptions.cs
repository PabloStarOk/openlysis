using System.ComponentModel.DataAnnotations;

using Openlysis.TestTools.ServicesSimulation.Common.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.EmailAddresses.Configuration;

/// <summary>
/// Configuration options for the email reputation stub factory.
/// </summary>
internal sealed record EmailReputationStubFactoryOptions : StubFactoryOptions
{
    /// <summary>
    /// Gets a value indicating whether disposable emails should be randomly identified.
    /// </summary>
    [Required]
    required public bool ReturnRandomDisposable { get; init; }

    /// <summary>
    /// Gets a value indicating whether emails should be always marked as disposable.
    /// </summary>
    required public bool? FixedIsDisposable { get; init; }

    /// <summary>
    /// Gets a value indicating whether risky TLDs should be randomly identified.
    /// </summary>
    [Required]
    required public bool ReturnRandomRiskyTld { get; init; }

    /// <summary>
    /// Gets a value indicating whether TLDs should be always marked as risky.
    /// </summary>
    required public bool? FixedIsRiskyTld { get; init; }
}
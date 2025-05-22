using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.Common.Extensions;

/// <summary>
/// Extension methods for configuration-related functionality in the service simulation test tools.
/// </summary>
internal static class ValidateOptionsResultExtensions
{
    /// <summary>
    /// Combines failure messages from multiple ValidateOptionsResults with the failures from a base ValidateOptionsResult.
    /// </summary>
    /// <param name="result">The ValidateOptionsResult containing failures to union.</param>
    /// <param name="otherResults">Additional ValidateOptionsResult instances whose failures will be included.</param>
    /// <returns>A combined list of all failure messages.</returns>
    internal static IReadOnlyList<string> UnionFailureMessages(
        this ValidateOptionsResult result,
        params ValidateOptionsResult[] otherResults)
    {
        var failureMessages = new List<string>();

        if (result.Failures is not null)
        {
            failureMessages.AddRange(result.Failures);
        }

        var allOtherFailures = otherResults
            .Where(o => o.Failures is not null)
            .SelectMany(r => r.Failures ?? []);

        failureMessages.AddRange(allOtherFailures);

        return failureMessages;
    }
}
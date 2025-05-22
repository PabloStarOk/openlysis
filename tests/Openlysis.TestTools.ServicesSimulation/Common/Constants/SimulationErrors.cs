using ErrorOr;

namespace Openlysis.TestTools.ServicesSimulation.Common.Constants;

/// <summary>
/// Contains predefined Error objects for simulating service errors in test scenarios.
/// </summary>
internal static class SimulationErrors
{
     /// <summary>
     /// Gets the error representing a failure in the Analyze endpoint simulation.
     /// </summary>
     /// <remarks>Uses error code -1 with "Simulation.AnalyzeEndpointError" type.</remarks>
     internal static Error Analyze => Error.Custom(
        -1,
        "Simulation.AnalyzeEndpointError",
        Description);

     /// <summary>
     /// Gets the error representing a failure in the GetAnalysisStatus endpoint simulation.
     /// </summary>
     /// <remarks>Uses error code -2 with "Simulation.GetAnalysisStatusEndpointError" type.</remarks>
     internal static Error GetAnalysisStatus => Error.Custom(
         -2,
         "Simulation.GetAnalysisStatusEndpointError",
         Description);

     /// <summary>
     /// Gets the error representing a failure in the GetAnalysis endpoint simulation.
     /// </summary>
     /// <remarks>Uses error code -2 with "Simulation.GetAnalysisEndpointError" type.</remarks>
     internal static Error GetAnalysis => Error.Custom(
         -3,
         "Simulation.GetAnalysisEndpointError",
         Description);

     /// <summary>
     /// Gets the error representing a failure in the GetReputation endpoint simulation.
     /// </summary>
     /// <remarks>Uses error code -4 with "Simulation.GetReputationError" type.</remarks>
     internal static Error GetReputation => Error.Custom(
         -4,
         "Simulation.GetReputationError",
         Description);

     private const string Description = "Simulation error from simulated services.";
}
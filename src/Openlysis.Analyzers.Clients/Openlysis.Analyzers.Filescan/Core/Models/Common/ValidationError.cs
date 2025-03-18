namespace Openlysis.Analyzers.Filescan.Core.Models.Common;

/// <summary>
/// Represents a validation error with details about the type, message, and location of the error.
/// </summary>
/// <param name="Type">The type of the validation error.</param>
/// <param name="Message">The message describing the validation error.</param>
/// <param name="Location">The location(s) where the validation error occurred.</param>
public record ValidationError(string Type, string Message, string[] Location);
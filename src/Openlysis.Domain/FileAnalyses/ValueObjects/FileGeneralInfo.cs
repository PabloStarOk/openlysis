namespace Openlysis.Domain.FileAnalyses.ValueObjects;

/// <summary>
/// General information of a file.
/// </summary>
/// <param name="MimeType">MIME Type of the file.</param>
/// <param name="Size">Size of the file in bytes.</param>
public sealed record FileGeneralInfo(string Name, string MimeType, int Size, DateTime CreationDate);

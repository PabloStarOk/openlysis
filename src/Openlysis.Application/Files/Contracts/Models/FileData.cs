namespace Openlysis.Application.Files.Contracts.Models;

/// <summary>
/// Represents the data of a file.
/// </summary>
/// <param name="Name">The name of the file.</param>
/// <param name="ContentType">The content type (MIME type) of the file.</param>
/// <param name="Password">The password associated with the file, if any.</param>
/// <param name="Stream">The stream containing the file's data.</param>
public record FileData(string Name,
    string ContentType,
    string Password,
    Stream Stream);
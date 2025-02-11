using Openlysis.Domain.Common.Hash;

namespace Openlysis.Domain.FileAnalyses.ValueObjects;

/// <summary>
/// Metadata of a file.
/// </summary>
public sealed record FileMetadata
{
    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the type of the content of the file.
    /// </summary>
    public string ContentType { get; init; }

    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    public long Size { get; init; }

    /// <summary>
    /// Gets the set of hash of the file.
    /// </summary>
    public ContentHashSet ContentHashSet { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMetadata"/> class.
    /// </summary>
    /// <param name="name">Name of the file.</param>
    /// <param name="contentType">Type of the content of the file.</param>
    /// <param name="size">Size of the file in bytes.</param>
    /// <param name="contentContentHashSet">The hash set associated with the file.</param>
    public FileMetadata(
        string name,
        string contentType,
        long size,
        ContentHashSet contentContentHashSet)
    {
        Name = name;
        ContentType = contentType;
        Size = size;
        ContentHashSet = contentContentHashSet;
    }

    // For EF core.
#pragma warning disable CS8618
    private FileMetadata()
    {
    }
#pragma warning restore CS8618
}

namespace Openlysis.Analyzers.Shared.Infrastructure.Files.Abstractions;

/// <summary>
/// Represents a factory for creating <see cref="Stream"/> instances asynchronously.
/// </summary>
public interface IStreamFactory
{
    /// <summary>
    /// Asynchronously creates and returns a <see cref="Stream"/> instance.
    /// </summary>
    /// <param name="key">A key to identify the created <see cref="Stream"/>>.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation, with a <see cref="Stream"/> as the result.</returns>
    public ValueTask<Stream> CreateStreamAsync(object key);
}
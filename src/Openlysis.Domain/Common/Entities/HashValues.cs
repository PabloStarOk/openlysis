namespace Openlysis.Domain.Common.Entities;

// TODO: Next commit: Improve efficiency in the creation of hash service. 

/// <summary>
/// Represents a set of hash values for a given entity.
/// </summary>
public sealed class HashValues : IEquatable<HashValues>
{
    /// <summary>
    /// Gets the MD5 hash value.
    /// </summary>
    public string Md5 { get; }

    /// <summary>
    /// Gets the SHA-1 hash value.
    /// </summary>
    public string Sha1 { get; }

    /// <summary>
    /// Gets the SHA-256 hash value.
    /// </summary>
    public string Sha256 { get; }

    /// <summary>
    /// Gets the SHA-512 hash value.
    /// </summary>
    public string Sha512 { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HashValues"/> class with the specified hash values.
    /// </summary>
    /// <param name="md5">The MD5 hash value.</param>
    /// <param name="sha1">The SHA-1 hash value.</param>
    /// <param name="sha256">The SHA-256 hash value.</param>
    /// <param name="sha512">The SHA-512 hash value.</param>
    private HashValues(
        string md5,
        string sha1,
        string sha256,
        string sha512)
    {
        Md5 = md5;
        Sha1 = sha1;
        Sha256 = sha256;
        Sha512 = sha512;
    }

#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="HashValues"/> class
    /// to be used by EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor is required by EF Core and should not be used directly in application code.
    /// </remarks>
    private HashValues()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="HashValues"/> class with the specified hash values.
    /// </summary>
    /// <param name="md5">The MD5 hash value.</param>
    /// <param name="sha1">The SHA-1 hash value.</param>
    /// <param name="sha256">The SHA-256 hash value.</param>
    /// <param name="sha512">The SHA-512 hash value.</param>
    /// <returns>A new <see cref="HashValues"/> instance.</returns>
    public static HashValues Create(
        string md5,
        string sha1,
        string sha256,
        string sha512)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(md5);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha1);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha512);

        return new HashValues(
            md5,
            sha1,
            sha256,
            sha512);
    }

    public static bool operator ==(HashValues left, HashValues right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(HashValues left, HashValues right)
    {
        return !Equals(left, right);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is HashValues entity
            && Sha256.Equals(entity.Sha256, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public bool Equals(HashValues? other)
    {
        return Equals((object?)other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Sha256.GetHashCode();
    }
}

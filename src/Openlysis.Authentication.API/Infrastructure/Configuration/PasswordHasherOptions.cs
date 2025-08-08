namespace Openlysis.Authentication.API.Infrastructure.Configuration;

/// <summary>
/// Options for configuring password hashing parameters.
/// </summary>
internal sealed record PasswordHasherOptions
{
    /// <summary>
    /// The configuration section name for password hashing options.
    /// </summary>
    public const string SectionName = "PasswordHashing";

    /// <summary>
    /// The default size of the hash in bytes.
    /// </summary>
    public const int DefaultHashSizeBytes = 32;

    /// <summary>
    /// The default size of the salt in bytes.
    /// </summary>
    public const int DefaultSaltSizeBytes = 16;

    /// <summary>
    /// The default memory size in kibibytes used by the hashing algorithm.
    /// </summary>
    public const int DefaultMemorySizeKibibytes = 19456;

    /// <summary>
    /// The default number of iterations for the hashing algorithm.
    /// </summary>
    public const int DefaultIterations = 2;

    /// <summary>
    /// The default degree of parallelism for the hashing algorithm.
    /// </summary>
    public const int DefaultParallelismDegree = 1;

    /// <summary>
    /// Gets or sets the hash size in bytes.
    /// </summary>
    public int HashSizeBytes { get; set; } = DefaultHashSizeBytes;

    /// <summary>
    /// Gets or sets the salt size in bytes.
    /// </summary>
    public int SaltSizeBytes { get; set; } = DefaultSaltSizeBytes;

    /// <summary>
    /// Gets or sets the memory size in kibibytes used by the hashing algorithm.
    /// </summary>
    public int MemorySizeKibibytes { get; set; } = DefaultMemorySizeKibibytes;

    /// <summary>
    /// Gets or sets the number of iterations for the hashing algorithm.
    /// </summary>
    public int Iterations { get; set; } = DefaultIterations;

    /// <summary>
    /// Gets or sets the degree of parallelism for the hashing algorithm.
    /// </summary>
    public int ParallelismDegree { get; set; } = DefaultParallelismDegree;
}
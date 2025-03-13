using Microsoft.EntityFrameworkCore;

using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.URLs;
using Openlysis.Infrastructure.Persistence.Configurations;

namespace Openlysis.Infrastructure.Persistence;

/// <summary>
/// Database context for analyses.
/// </summary>
public class AnalysesDbContext : DbContext
{
    /// <summary>
    /// Gets the content hash sets of the database.
    /// </summary>
    public DbSet<ContentHashSet> ContentHashSets { get; init; } = null!;

    /// <summary>
    /// Gets the file analyses set of the database.
    /// </summary>
    public DbSet<FileMultiAnalysis> FileMultiAnalyses { get; init; } = null!;

    /// <summary>
    /// Gets the URL analyses set of the database.
    /// </summary>
    public DbSet<UrlMultiAnalysis> UrlMultiAnalyses { get; init; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysesDbContext"/> class.
    /// </summary>
    /// <param name="options">Options of the database context.</param>
    public AnalysesDbContext(DbContextOptions<AnalysesDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ContentHashSetConfiguration());
        modelBuilder.ApplyConfiguration(new FileAnalysisConfiguration());
        modelBuilder.ApplyConfiguration(new UrlMultiAnalysisConfiguration());
    }
}
using Microsoft.EntityFrameworkCore;

using Openlysis.Domain.FileAnalyses;
using Openlysis.Infrastructure.Persistence.Configurations;

namespace Openlysis.Infrastructure.Persistence;

/// <summary>
/// Database context for analyses.
/// </summary>
public class AnalysesDbContext : DbContext
{
    /// <summary>
    /// Gets the file analyses set of the database.
    /// </summary>
    public DbSet<FileMultiAnalysis> FileAnalyses { get; init; } = null!;

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
        modelBuilder.ApplyConfiguration(new FileAnalysisConfiguration());
    }
}
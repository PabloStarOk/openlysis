using Microsoft.EntityFrameworkCore;

using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Persistence.Configurations;
using Openlysis.Infrastructure.Persistence.Configurations.Files;
using Openlysis.Infrastructure.Persistence.Configurations.Phones;
using Openlysis.Infrastructure.Persistence.Configurations.URLs;

namespace Openlysis.Infrastructure.Persistence;

/// <summary>
/// Database context for analyses.
/// </summary>
public class ApplicationDbContext : DbContext
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
    /// Gets the URL service analyses set of the database.
    /// </summary>
    public DbSet<UrlServiceAnalysis> UrlServiceAnalyses { get; init; } = null!;

    /// <summary>
    /// Gets a database set of <see cref="PhoneMultiValidation"/>.
    /// </summary>
    public DbSet<PhoneMultiValidation> PhoneMultiValidations { get; init; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">Options of the database context.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ContentHashSetConfiguration());
        modelBuilder.ApplyConfiguration(new FileAnalysisConfiguration());
        modelBuilder.ApplyConfiguration(new UrlServiceAnalysisConfiguration());
        modelBuilder.ApplyConfiguration(new UrlMultiAnalysisConfiguration());
        modelBuilder.ApplyConfiguration(new PhoneMultiValidationConfiguration());
    }
}
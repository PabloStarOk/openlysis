using Microsoft.EntityFrameworkCore;

using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Persistence.Configurations.Common;
using Openlysis.Infrastructure.Persistence.Configurations.EmailAddresses;
using Openlysis.Infrastructure.Persistence.Configurations.Files;
using Openlysis.Infrastructure.Persistence.Configurations.Messages;
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
    public DbSet<HashValues> ContentHashSets { get; init; } = null!;

    /// <summary>
    /// Gets the file analyses set of the database.
    /// </summary>
    public DbSet<FileMultiAnalysis> FileMultiAnalyses { get; init; } = null!;

    /// <summary>
    /// Gets the file service analyses set of the database.
    /// </summary>
    public DbSet<FileServiceAnalysis> FileServiceAnalyses { get; init; } = null!;

    /// <summary>
    /// Gets the URL analyses set of the database.
    /// </summary>
    public DbSet<UrlMultiAnalysis> UrlMultiAnalyses { get; init; } = null!;

    /// <summary>
    /// Gets the URL service analyses set of the database.
    /// </summary>
    public DbSet<UrlServiceAnalysis> UrlServiceAnalyses { get; init; } = null!;

    /// <summary>
    /// Gets a database set of <see cref="PhoneMultiReputation"/>.
    /// </summary>
    public DbSet<PhoneMultiReputation> PhoneMultiReputations { get; init; } = null!;

    /// <summary>
    /// Gets a database set of <see cref="EmailAddressMultiReputation"/>.
    /// </summary>
    public DbSet<EmailAddressMultiReputation> EmailAddressMultiReputations { get; init; } = null!;

    /// <summary>
    /// Gets a database set of <see cref="MessageAnalysis"/>.
    /// </summary>
    public DbSet<MessageAnalysis> MessageAnalyses { get; init; } = null!;

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
        modelBuilder.ApplyConfiguration(new FileServiceAnalysisConfiguration());
        modelBuilder.ApplyConfiguration(new FileMultiAnalysisConfiguration());
        modelBuilder.ApplyConfiguration(new UrlServiceAnalysisConfiguration());
        modelBuilder.ApplyConfiguration(new UrlMultiAnalysisConfiguration());
        modelBuilder.ApplyConfiguration(new PhoneMultiReputationConfiguration());
        modelBuilder.ApplyConfiguration(new EmailAddressMultiReputationConfiguration());
        modelBuilder.ApplyConfiguration(new MessageAnalysisConfiguration());
    }
}
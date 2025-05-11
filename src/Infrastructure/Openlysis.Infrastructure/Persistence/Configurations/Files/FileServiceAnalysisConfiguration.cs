using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.Files.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Configurations.Files;

/// <summary>
/// Provides configuration for the <see cref="FileServiceAnalysis"/> entity in the database.
/// Implements the <see cref="IEntityTypeConfiguration{TEntity}"/> interface to configure the entity.
/// </summary>
public class FileServiceAnalysisConfiguration : IEntityTypeConfiguration<FileServiceAnalysis>
{
    private const string SmallintType = "smallint";
    private const string VarcharType = "varchar";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileServiceAnalysis> builder)
    {
        ConfigureServiceAnalyses(builder);
        builder.OwnsMany(s => s.Reports, ConfigureReportEntity);
    }

    /// <summary>
    /// Configures the properties and relationships for the <see cref="FileServiceAnalysis"/> entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="EntityTypeBuilder{TEntity}"/> used to configure the entity.
    /// </param>
    private static void ConfigureServiceAnalyses(
        EntityTypeBuilder<FileServiceAnalysis> builder)
    {
        builder.ToTable("file_service_analyses");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("file_service_analysis_id")
            .HasColumnType(VarcharType)
            .HasMaxLength(200)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.ToString(),
                dbValue => ComposedServiceAnalysisId.Parse(dbValue));

        builder.Property(s => s.ServiceName)
            .HasColumnName("service_name")
            .HasColumnType(VarcharType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(u => u.Error)
            .HasColumnName("error")
            .HasColumnType(VarcharType)
            .HasMaxLength(250);
    }

    /// <summary>
    /// Configures the properties and relationships for the <see cref="Report"/> entity
    /// as an owned navigation property of <see cref="FileServiceAnalysis"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}"/> used to configure the owned entity.
    /// </param>
    private static void ConfigureReportEntity(
        OwnedNavigationBuilder<FileServiceAnalysis, Report> builder)
    {
        builder.ToTable("reports");

        builder.WithOwner().HasForeignKey("file_service_analysis_id");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("report_id")
            .HasColumnType(VarcharType)
            .HasMaxLength(200)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => ReportId.Create(dbValue));

        builder.Property(r => r.Verdict)
            .HasColumnName("verdict")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(r => r.ThreatZone)
            .HasColumnName("threat_zone")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(r => r.ThreatScore)
            .HasColumnName("threat_score")
            .HasColumnType("real");
    }
}
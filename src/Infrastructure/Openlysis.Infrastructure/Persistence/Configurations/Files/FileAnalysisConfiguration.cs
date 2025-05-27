using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.Files.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Configurations.Files;

/// <summary>
/// Provides configuration for the <see cref="FileAnalysis"/> entity in the database.
/// Implements the <see cref="IEntityTypeConfiguration{TEntity}"/> interface to configure the entity.
/// </summary>
public class FileAnalysisConfiguration : IEntityTypeConfiguration<FileAnalysis>
{
    private const string SmallintType = "smallint";
    private const string VarcharType = "varchar";
    private const string RealType = "real";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileAnalysis> builder)
    {
        ConfigureServiceAnalyses(builder);
        builder.OwnsMany(s => s.Reports, ConfigureReportEntity);
    }

    /// <summary>
    /// Configures the properties and relationships for the <see cref="FileAnalysis"/> entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="EntityTypeBuilder{TEntity}"/> used to configure the entity.
    /// </param>
    private static void ConfigureServiceAnalyses(
        EntityTypeBuilder<FileAnalysis> builder)
    {
        builder.ToTable("file_analyses");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("file_analysis_id")
            .HasColumnType(VarcharType)
            .HasMaxLength(200)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.ToString(),
                dbValue => ComposedAnalysisId.Parse(dbValue));

        builder.Property(s => s.ServiceName)
            .HasColumnName("service_name")
            .HasColumnType(VarcharType)
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(s => s.State, stateBuilder =>
        {
            stateBuilder.Property(s => s.Status)
                .HasColumnName("status")
                .HasColumnType(SmallintType)
                .IsRequired();

            stateBuilder.Property(s => s.Verdict)
                .HasColumnName("verdict")
                .HasColumnType(SmallintType)
                .IsRequired();

            stateBuilder.Property(s => s.ThreatZone)
                .HasColumnName("threat_zone")
                .HasColumnType(SmallintType)
                .IsRequired();
        });

        builder.OwnsOne(s => s.ThreatScore, tsBuilder =>
        {
            tsBuilder.Ignore(t => t.NormalizedValue);

            tsBuilder.Property(t => t.RawValue)
                .HasColumnName("raw_threat_score")
                .HasColumnType(RealType);

            tsBuilder.Property(t => t.MaxPossibleRawValue)
                .HasColumnName("max_possible_threat_score")
                .HasColumnType(RealType);
        });
    }

    /// <summary>
    /// Configures the properties and relationships for the <see cref="FileReport"/> entity
    /// as an owned navigation property of <see cref="FileAnalysis"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}"/> used to configure the owned entity.
    /// </param>
    private static void ConfigureReportEntity(
        OwnedNavigationBuilder<FileAnalysis, FileReport> builder)
    {
        builder.ToTable("reports");

        builder.WithOwner().HasForeignKey("file_analysis_id");

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

        builder.OwnsOne(r => r.ThreatScore, tsBuilder =>
        {
            tsBuilder.Ignore(t => t.NormalizedValue);

            tsBuilder.Property(t => t.RawValue)
                .HasColumnName("raw_threat_score")
                .HasColumnType(RealType);

            tsBuilder.Property(t => t.MaxPossibleRawValue)
                .HasColumnName("max_possible_threat_score")
                .HasColumnType(RealType);
        });
    }
}
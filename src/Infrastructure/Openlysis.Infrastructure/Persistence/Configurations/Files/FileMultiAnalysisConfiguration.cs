using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.Files;

/// <summary>
/// Configuration for the <see cref="FileMultiAnalysis"/> entity in the database.
/// </summary>
public class FileMultiAnalysisConfiguration : IEntityTypeConfiguration<FileMultiAnalysis>
{
    private const string SmallintType = "smallint";
    private const string VarcharType = "varchar";
    private const string RealType = "real";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileMultiAnalysis> builder)
    {
        ConfigureFileMultiAnalysesTable(builder);
        builder.OwnsMany(a => a.Analyses, ConfigureAnalyses);
    }

    /// <summary>
    /// Configures the FileMultiAnalyses table.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    private static void ConfigureFileMultiAnalysesTable(EntityTypeBuilder<FileMultiAnalysis> builder)
    {
        builder.ToTable("file_multi_analyses");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("file_multi_analysis_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Property(u => u.IsPrivate)
            .HasColumnName("is_private")
            .HasColumnType("boolean")
            .IsRequired();

        builder.Property(f => f.StartedDate)
            .HasColumnName("started_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.OwnsOne(f => f.State, stateBuilder =>
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

        builder.Property(f => f.AverageThreatScore)
            .HasColumnName("average_threat_score")
            .HasColumnType("real");

        builder.OwnsOne(
            f => f.FileMetadata, metadataBuilder =>
            {
                metadataBuilder.Property(m => m.Name)
                    .HasColumnName("file_name")
                    .HasColumnType("varchar")
                    .HasMaxLength(100)
                    .IsRequired();

                metadataBuilder.Property(m => m.Size)
                    .HasColumnName("size")
                    .HasColumnType("bigint")
                    .IsRequired();

                metadataBuilder.Property(m => m.ContentType)
                    .HasColumnName("content_type")
                    .HasColumnType("text")
                    .IsRequired();
            });

        builder.Property(u => u.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.HasOne(f => f.DataHashValues)
            .WithMany()
            .HasForeignKey("sha256")
            .IsRequired();

        builder.Navigation(f => f.DataHashValues)
            .AutoInclude();

        builder.Navigation(f => f.Analyses)
            .AutoInclude();

        builder.Ignore(f => f.AllReports);
        builder.Ignore(f => f.ReportsAmount);
    }

    /// <summary>
    /// Configures the owned collection of <see cref="FileAnalysis"/> entities for a <see cref="FileMultiAnalysis"/>.
    /// </summary>
    private static void ConfigureAnalyses(
        OwnedNavigationBuilder<FileMultiAnalysis, FileAnalysis> builder)
    {
        builder.ToTable("file_analyses");

        builder.WithOwner().HasForeignKey("file_multi_analysis_id");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("file_analysis_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Property(s => s.ExternalId)
            .HasColumnName("external_id")
            .HasColumnType("text")
            .IsRequired()
            .HasConversion(
                id => id.ToString(),
                dbValue => ExternalAnalysisId.Parse(dbValue));

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

        builder.OwnsMany(s => s.Reports, ConfigureReports);
    }

    /// <summary>
    /// Configures the owned collection of <see cref="FileReport"/> entities for a <see cref="FileAnalysis"/>.
    /// </summary>
    private static void ConfigureReports(
        OwnedNavigationBuilder<FileAnalysis, FileReport> builder)
    {
        builder.ToTable("file_reports");

        builder.WithOwner().HasForeignKey("file_analysis_id");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("file_report_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Property(r => r.ExternalId)
            .HasColumnName("external_id")
            .HasColumnType(VarcharType)
            .HasMaxLength(200)
            .IsRequired();

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
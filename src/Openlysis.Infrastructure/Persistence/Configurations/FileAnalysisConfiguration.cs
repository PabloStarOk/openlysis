using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the <see cref="FileMultiAnalysis"/> entity in the database.
/// </summary>
public class FileAnalysisConfiguration : IEntityTypeConfiguration<FileMultiAnalysis>
{
    private const string VarcharType = "VARCHAR";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileMultiAnalysis> builder)
    {
        ConfigureFileMultiAnalysesTable(builder);
        ConfigureContentHashSetsTable(builder);
        ConfigureServiceFileAnalysesTable(builder);
    }

    /// <summary>
    /// Configures the FileMultiAnalyses table.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    private static void ConfigureFileMultiAnalysesTable(EntityTypeBuilder<FileMultiAnalysis> builder)
    {
        builder.ToTable("FileMultiAnalyses");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("FileMultiAnalysisId")
            .HasColumnType(VarcharType)
            .HasMaxLength(36)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => FileMultiAnalysisId.Create(dbValue));

        builder.Property(f => f.StartedDate)
            .HasColumnName("StartedDate")
            .HasColumnType("DATETIME2")
            .IsRequired();

        builder.Property(f => f.AverageVerdict)
            .HasColumnName("AverageVerdict")
            .HasColumnType(VarcharType)
            .HasMaxLength(10)
            .IsRequired()
            .HasConversion(
                verdict => verdict.ToString(),
                dbValue => Enum.Parse<Verdict>(dbValue, true));

        builder.Property(f => f.AverageThreatZone)
            .HasColumnName("AverageThreatZone")
            .HasColumnType(VarcharType)
            .HasMaxLength(6)
            .IsRequired()
            .HasConversion(
                zone => zone.ToString(),
                dbValue => Enum.Parse<ThreatZone>(dbValue, true));

        builder.Property(f => f.Status)
            .HasColumnName("Status")
            .HasColumnType(VarcharType)
            .HasMaxLength(8)
            .IsRequired()
            .HasConversion(
                status => status.ToString(),
                dbValue => Enum.Parse<AnalysisStatus>(dbValue, true));

        builder.OwnsOne(
            f => f.FileMetadata, metadataBuilder =>
            {
                metadataBuilder.Property(m => m.Name)
                    .HasColumnName("FileName")
                    .HasColumnType("NVARCHAR")
                    .HasMaxLength(100)
                    .IsRequired();

                metadataBuilder.Property(m => m.Size)
                    .HasColumnName("Size")
                    .HasColumnType("BIGINT")
                    .IsRequired();

                metadataBuilder.Property(m => m.ContentType)
                    .HasColumnName("ContentType")
                    .HasColumnType(VarcharType)
                    .HasMaxLength(20)
                    .IsRequired();
            });

        // Shadow property for referencing ContentHashSet.
        builder.Property<string>("Sha256")
            .HasColumnName("Sha256")
            .HasColumnType(VarcharType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Ignore(f => f.AllReports);
        builder.Ignore(f => f.ReportsAmount);
    }

    /// <summary>
    /// Configures the ContentHashSets table.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    private static void ConfigureContentHashSetsTable(EntityTypeBuilder<FileMultiAnalysis> builder)
    {
        builder.OwnsOne(f => f.ContentHashSet, hashBuilder =>
        {
            hashBuilder.ToTable("ContentHashSets");
            hashBuilder.HasKey(h => h.Sha256);

            hashBuilder.WithOwner().HasPrincipalKey("Sha256");

            hashBuilder.Property(h => h.Sha256)
                .HasColumnName("Sha256")
                .HasColumnType(VarcharType)
                .HasMaxLength(64)
                .IsRequired()
                .ValueGeneratedNever();

            hashBuilder.Property(h => h.Md5)
                .HasColumnName("Md5")
                .HasColumnType(VarcharType)
                .HasMaxLength(32)
                .IsRequired();

            hashBuilder.Property(h => h.Sha1)
                .HasColumnName("Sha1")
                .HasColumnType(VarcharType)
                .HasMaxLength(40)
                .IsRequired();

            hashBuilder.Property(h => h.Sha512)
                .HasColumnName("Sha512")
                .HasColumnType(VarcharType)
                .HasMaxLength(128)
                .IsRequired();
        });
    }

    /// <summary>
    /// Configures the ServiceFileAnalyses table.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    private static void ConfigureServiceFileAnalysesTable(EntityTypeBuilder<FileMultiAnalysis> builder)
    {
        builder.OwnsMany(f => f.ServiceFileAnalyses, analysesBuilder =>
        {
            analysesBuilder.ToTable("ServiceFileAnalyses");
            analysesBuilder.WithOwner().HasForeignKey("FileMultiAnalysisId");

            analysesBuilder.HasKey(s => s.Id);

            analysesBuilder.Property(s => s.Id)
                .HasColumnName("ServiceFileAnalysisId")
                .HasColumnType(VarcharType)
                .HasMaxLength(100)
                .IsRequired()
                .ValueGeneratedNever()
                .HasConversion(
                    id => id.Value,
                    dbValue => ServiceFileAnalysisId.Create(dbValue));

            analysesBuilder.Property(s => s.ServiceName)
                .HasColumnName("ServiceName")
                .HasColumnType("NVARCHAR")
                .HasMaxLength(30)
                .IsRequired();

            analysesBuilder.Property(s => s.Status)
                .HasColumnName("Status")
                .HasColumnType(VarcharType)
                .HasMaxLength(8)
                .IsRequired()
                .HasConversion(
                    status => status.ToString(),
                    dbValue => Enum.Parse<AnalysisStatus>(dbValue, true));

            analysesBuilder.OwnsMany(s => s.Reports, ConfigureReportsTable);
        });
    }

    /// <summary>
    /// Configures the Reports table.
    /// </summary>
    /// <param name="reportBuilder">The builder to be used to configure the entity.</param>
    private static void ConfigureReportsTable(OwnedNavigationBuilder<ServiceFileAnalysis, Report> reportBuilder)
    {
        reportBuilder.ToTable("Reports");
        reportBuilder.HasKey(r => r.Id);
        reportBuilder.WithOwner().HasForeignKey("ServiceFileAnalysisId");

        reportBuilder.Property(r => r.Id)
            .HasColumnName("ReportId")
            .HasColumnType(VarcharType)
            .HasMaxLength(100)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => ReportId.Create(dbValue));

        reportBuilder.Property(r => r.Verdict)
            .HasColumnName("Verdict")
            .HasColumnType(VarcharType)
            .HasMaxLength(10)
            .IsRequired()
            .HasConversion(
                verdict => verdict.ToString(),
                dbValue => Enum.Parse<Verdict>(dbValue));

        reportBuilder.Property(r => r.ThreatZone)
            .HasColumnName("ThreatZone")
            .HasColumnType(VarcharType)
            .HasMaxLength(6)
            .IsRequired()
            .HasConversion(
                zone => zone.ToString(),
                dbValue => Enum.Parse<ThreatZone>(dbValue));

        reportBuilder.Property(r => r.ThreatLevel)
            .HasColumnName("ThreatLevel")
            .HasColumnType("FLOAT")
            .HasMaxLength(25);
    }
}
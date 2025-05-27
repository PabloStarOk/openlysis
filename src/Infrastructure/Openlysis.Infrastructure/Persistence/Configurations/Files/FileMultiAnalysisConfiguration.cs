using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Configurations.Files;

/// <summary>
/// Configuration for the <see cref="FileMultiAnalysis"/> entity in the database.
/// </summary>
public class FileMultiAnalysisConfiguration : IEntityTypeConfiguration<FileMultiAnalysis>
{
    private const string SmallintType = "smallint";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileMultiAnalysis> builder)
    {
        ConfigureFileMultiAnalysesTable(builder);
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
                dbValue => GlobalId.Parse(dbValue));

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
            .HasColumnType("varchar")
            .HasMaxLength(450)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => UserId.Create(dbValue));

        builder.HasOne(f => f.DataHashValues)
            .WithMany()
            .HasForeignKey("sha256")
            .IsRequired();

        builder.HasMany(u => u.Analyses)
            .WithMany()
            .UsingEntity(
                "file_multi_service_analysis_links",
                r => r.HasOne(typeof(FileAnalysis)).WithMany().HasForeignKey("file_analysis_id"),
                l => l.HasOne(typeof(FileMultiAnalysis)).WithMany().HasForeignKey("file_multi_analysis_id"),
                joinEntity =>
                {
                    joinEntity.HasKey("file_multi_analysis_id", "file_analysis_id");
                });

        builder.Navigation(f => f.DataHashValues)
            .AutoInclude();

        builder.Navigation(f => f.Analyses)
            .AutoInclude();

        builder.Ignore(f => f.AllReports);
        builder.Ignore(f => f.ReportsAmount);
    }
}
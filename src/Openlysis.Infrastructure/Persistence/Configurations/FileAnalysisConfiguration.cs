using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the <see cref="FileAnalysis"/> entity in the database.
/// </summary>
public class FileAnalysisConfiguration : IEntityTypeConfiguration<FileAnalysis>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileAnalysis> builder)
    {
        builder.ToTable("FileAnalyses");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasColumnName("FileAnalysisId")
            .HasColumnType("VARCHAR")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => FileAnalysisId.Create(dbValue));

        builder.Property(f => f.LastScanDate)
            .HasColumnName("LastScanDate")
            .HasColumnType("DATETIME2");

        builder.Property(f => f.Verdict)
            .HasColumnName("Verdict")
            .HasColumnType("VARCHAR")
            .HasMaxLength(10)
            .HasConversion(
                verdict => verdict.ToString(),
                dbValue => Enum.Parse<Verdict>(dbValue, true));

        builder.OwnsOne(f => f.Metadata, metadataBuilder =>
        {
            metadataBuilder.Property(i => i.Name)
                .HasColumnName("FileName");
            metadataBuilder.Property(i => i.ContentType)
                .HasColumnName("ContentType");
            metadataBuilder.Property(i => i.Size)
                .HasColumnName("Size");

            metadataBuilder.OwnsOne(
                f => f.ContentHashSet, hashBuilder =>
                {
                    hashBuilder.Property(h => h.Md5)
                        .HasColumnName("Md5");
                    hashBuilder.Property(h => h.Sha1)
                        .HasColumnName("Sha1");
                    hashBuilder.Property(h => h.Sha256)
                        .HasColumnName("Sha256");
                    hashBuilder.Property(h => h.Sha512)
                        .HasColumnName("Sha512");
                });
        });

        builder.Ignore(f => f.Reports);
    }
}
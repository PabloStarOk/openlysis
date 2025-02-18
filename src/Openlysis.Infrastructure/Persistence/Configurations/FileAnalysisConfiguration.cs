using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the <see cref="FileMultiAnalysis"/> entity in the database.
/// </summary>
public class FileAnalysisConfiguration : IEntityTypeConfiguration<FileMultiAnalysis>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileMultiAnalysis> builder)
    {
        builder.ToTable("FileAnalyses");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasColumnName("FileMultiAnalysisId")
            .HasColumnType("VARCHAR")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => FileMultiAnalysisId.Create(dbValue));

        builder.Property(f => f.StartedDate)
            .HasColumnName("StartedDate")
            .HasColumnType("DATETIME2");

        builder.Property(f => f.AverageVerdict)
            .HasColumnName("AverageVerdict")
            .HasColumnType("VARCHAR")
            .HasMaxLength(10)
            .HasConversion(
                verdict => verdict.ToString(),
                dbValue => Enum.Parse<Verdict>(dbValue, true));

        builder.OwnsOne(f => f.Metadata, metadataBuilder =>
        {
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
    }
}
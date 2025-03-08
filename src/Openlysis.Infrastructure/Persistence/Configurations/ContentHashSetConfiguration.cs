using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.Hash;

namespace Openlysis.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration class for the ContentHashSet entity.
/// Implements the IEntityTypeConfiguration interface to configure the ContentHashSet entity.
/// </summary>
public class ContentHashSetConfiguration : IEntityTypeConfiguration<ContentHashSet>
{
    private const string VarcharType = "VARCHAR";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ContentHashSet> builder)
    {
        builder.ToTable("ContentHashSets");
        builder.HasKey(h => h.Sha256);

        builder.Property(h => h.Sha256)
            .HasColumnName("Sha256")
            .HasColumnType(VarcharType)
            .HasMaxLength(64)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(h => h.Md5)
            .HasColumnName("Md5")
            .HasColumnType(VarcharType)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(h => h.Sha1)
            .HasColumnName("Sha1")
            .HasColumnType(VarcharType)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(h => h.Sha512)
            .HasColumnName("Sha512")
            .HasColumnType(VarcharType)
            .HasMaxLength(128)
            .IsRequired();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Configuration class for the <see cref="HashValues"/> entity.
/// </summary>
public class ContentHashSetConfiguration : IEntityTypeConfiguration<HashValues>
{
    private const string CharType = "char";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<HashValues> builder)
    {
        builder.ToTable("hash_values");
        builder.HasKey(h => h.Sha256);

        builder.Property(h => h.Sha256)
            .HasColumnName("sha256")
            .HasColumnType(CharType)
            .HasMaxLength(64)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(h => h.Md5)
            .HasColumnName("md5")
            .HasColumnType(CharType)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(h => h.Sha1)
            .HasColumnName("sha1")
            .HasColumnType(CharType)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(h => h.Sha512)
            .HasColumnName("sha512")
            .HasColumnType(CharType)
            .HasMaxLength(128)
            .IsRequired();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Configuration class for the <see cref="HashValues"/> entity.
/// </summary>
public class HashValuesConfiguration : IEntityTypeConfiguration<HashValues>
{
    private const string ByteArrayType = "bytea";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<HashValues> builder)
    {
        builder.ToTable("hash_values");
        builder.HasKey(h => h.Sha256);

        builder.Property(h => h.Sha256)
            .HasColumnName("sha256")
            .HasColumnType(ByteArrayType)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                value => Convert.FromHexString(value),
                dbValue => Convert.ToHexString(dbValue));

        builder.Property(h => h.Md5)
            .HasColumnName("md5")
            .HasColumnType(ByteArrayType)
            .IsRequired()
            .HasConversion(
                value => Convert.FromHexString(value),
                dbValue => Convert.ToHexString(dbValue));

        builder.Property(h => h.Sha1)
            .HasColumnName("sha1")
            .HasColumnType(ByteArrayType)
            .IsRequired()
            .HasConversion(
                value => Convert.FromHexString(value),
                dbValue => Convert.ToHexString(dbValue));

        builder.Property(h => h.Sha512)
            .HasColumnName("sha512")
            .HasColumnType(ByteArrayType)
            .IsRequired()
            .HasConversion(
                value => Convert.FromHexString(value),
                dbValue => Convert.ToHexString(dbValue));
    }
}
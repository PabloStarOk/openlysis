using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration class for the <see cref="UrlServiceAnalysis"/> entity.
/// Implements the <see cref="IEntityTypeConfiguration{TEntity}"/> interface to configure the <see cref="UrlServiceAnalysis"/> entity.
/// </summary>
public class UrlServiceAnalysisConfiguration : IEntityTypeConfiguration<UrlServiceAnalysis>
{
    private const string NvarcharType = "NVARCHAR";
    private const string TinyIntType = "TINYINT";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UrlServiceAnalysis> builder)
    {
        builder.ToTable("UrlServiceAnalyses");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("UrlServiceAnalysisId")
            .HasColumnType(NvarcharType)
            .HasMaxLength(100)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.ToString(),
                id => ComposedServiceAnalysisId.Create(id));

        builder.Property(u => u.ServiceName)
            .HasColumnName("ServiceName")
            .HasColumnType(NvarcharType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasColumnName("Status")
            .HasColumnType(TinyIntType)
            .IsRequired();

        builder.Property(u => u.Verdict)
            .HasColumnName("Verdict")
            .HasColumnType(TinyIntType)
            .IsRequired();

        builder.Property(u => u.ThreatZone)
            .HasColumnName("ThreatZone")
            .HasColumnType(TinyIntType)
            .IsRequired();

        builder.Property(u => u.ThreatScore)
            .HasColumnName("ThreatScore")
            .HasColumnType("FLOAT")
            .HasMaxLength(25);
    }
}
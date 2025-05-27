using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.URLs;

/// <summary>
/// Configuration class for the <see cref="UrlAnalysis"/> entity.
/// Implements the <see cref="IEntityTypeConfiguration{TEntity}"/> interface to configure the <see cref="UrlAnalysis"/> entity.
/// </summary>
public class UrlAnalysisConfiguration : IEntityTypeConfiguration<UrlAnalysis>
{
    private const string SmallintType = "smallint";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UrlAnalysis> builder)
    {
        builder.ToTable("url_analyses");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("url_analysis_id")
            .HasColumnType("varchar")
            .HasMaxLength(100)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.ToString(),
                id => ComposedAnalysisId.Parse(id));

        builder.Property(u => u.ServiceName)
            .HasColumnName("service_name")
            .HasColumnType("varchar")
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(u => u.State, stateBuilder =>
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

        builder.OwnsOne(r => r.ThreatScore, tsBuilder =>
        {
            tsBuilder.Ignore(t => t.NormalizedValue);

            tsBuilder.Property(t => t.RawValue)
                .HasColumnName("raw_threat_score")
                .HasColumnType("real");

            tsBuilder.Property(t => t.MaxPossibleRawValue)
                .HasColumnName("max_possible_threat_score")
                .HasColumnType("real");
        });
    }
}
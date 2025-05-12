using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.URLs;

/// <summary>
/// Configuration class for the <see cref="UrlServiceAnalysis"/> entity.
/// Implements the <see cref="IEntityTypeConfiguration{TEntity}"/> interface to configure the <see cref="UrlServiceAnalysis"/> entity.
/// </summary>
public class UrlServiceAnalysisConfiguration : IEntityTypeConfiguration<UrlServiceAnalysis>
{
    private const string SmallintType = "smallint";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UrlServiceAnalysis> builder)
    {
        builder.ToTable("url_service_analyses");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("url_service_analysis_id")
            .HasColumnType("varchar")
            .HasMaxLength(100)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.ToString(),
                id => ComposedServiceAnalysisId.Parse(id));

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

        builder.Property(u => u.ThreatScore)
            .HasColumnName("threat_score")
            .HasColumnType("real");

        builder.Property(u => u.Error)
            .HasColumnName("error")
            .HasColumnType("varchar")
            .HasMaxLength(250);
    }
}
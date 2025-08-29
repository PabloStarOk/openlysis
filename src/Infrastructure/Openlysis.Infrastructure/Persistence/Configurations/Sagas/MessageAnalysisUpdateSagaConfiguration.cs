using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Communication.Sagas.Messages;

namespace Openlysis.Infrastructure.Persistence.Configurations.Sagas;

/// <summary>
/// Entity Framework configuration for the <see cref="MessageAnalysisUpdateSaga"/> entity.
/// </summary>
internal class MessageAnalysisUpdateSagaConfiguration : IEntityTypeConfiguration<MessageAnalysisUpdateSaga>
{
    private const string UuidType = "uuid";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<MessageAnalysisUpdateSaga> builder)
    {
        builder.ToTable("message_analysis_update_sagas");

        builder.HasKey(s => s.CorrelationId);

        builder.Property(s => s.CorrelationId)
            .HasColumnName("correlation_id")
            .HasColumnType(UuidType)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(s => s.CurrentState)
            .HasColumnName("current_state")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(s => s.MessageAnalysisId)
            .HasColumnName("message_analysis_id")
            .HasColumnType(UuidType)
            .IsRequired()
            .HasConversion(
                value => value.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        var deferredIdsComparer = new ValueComparer<HashSet<GlobalId>>(
            (c1, c2) => ReferenceEquals(c1, c2) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, HashCode.Combine),
            c => c.ToHashSet());

        builder.Property(s => s.DeferredFileUpdates)
            .HasColumnName("deferred_file_updates")
            .HasColumnType($"{UuidType}[]")
            .IsRequired()
            .HasConversion(
                value => value.Select(v => v.Value).ToArray(),
                dbValue => dbValue.Select(v => GlobalId.Parse(v.ToString())).ToHashSet(),
                deferredIdsComparer);

        builder.Property(s => s.DeferredUrlUpdates)
            .HasColumnName("deferred_url_updates")
            .HasColumnType($"{UuidType}[]")
            .IsRequired()
            .HasConversion(
                value => value.Select(v => v.Value).ToArray(),
                dbValue => dbValue.Select(v => GlobalId.Parse(v.ToString())).ToHashSet(),
                deferredIdsComparer);
    }
}
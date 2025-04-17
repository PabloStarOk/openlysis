using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.ValueObjects;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.Messages;

/// <summary>
/// Configuration class for the <see cref="MessageAnalysis"/> entity.
/// Implements the <see cref="IEntityTypeConfiguration{TEntity}"/> interface to configure the entity's schema.
/// </summary>
public class MessageAnalysisConfiguration : IEntityTypeConfiguration<MessageAnalysis>
{
    private const string SmallintType = "smallint";
    private const string VarcharType = "varchar";
    private const string MessageAnalysisIdColumnName = "message_analysis_id";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<MessageAnalysis> builder)
    {
        ConfigureMessageAnalysis(builder);
        builder.OwnsMany(f => f.AttachedFilesResults, ConfigureAttachedFileResults);
        builder.OwnsMany(f => f.DetectedUrlsResults, ConfigureDetectedUrlResults);
        builder.OwnsMany(f => f.DetectedEmailAddressesResults, bld => ConfigureDetectedStringResults(
            bld,
            tableName: "detected_email_address_results",
            idColumnName: "detected_email_address_results_id",
            valueColumnName: "email_address",
            resultIdColumnName: "email_address_multi_reputation_id"));
        builder.OwnsMany(f => f.DetectedPhoneNumbersResults, bld => ConfigureDetectedStringResults(
            bld,
            tableName: "detected_phone_number_results",
            idColumnName: "detected_phone_number_results_id",
            valueColumnName: "phone_number",
            resultIdColumnName: "phone_multi_reputation_id"));
    }

    /// <summary>
    /// Configures the schema for the <see cref="MessageAnalysis"/> entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="EntityTypeBuilder{TEntity}"/> used to configure the entity.
    /// </param>
    private static void ConfigureMessageAnalysis(
        EntityTypeBuilder<MessageAnalysis> builder)
    {
        builder.ToTable("message_analysis");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName(MessageAnalysisIdColumnName)
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.Property(m => m.StartedDate)
            .HasColumnName("started_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.OwnsOne(m => m.Message, messageBuilder =>
        {
            messageBuilder.Property(m => m.Type)
                .HasColumnName("message_type")
                .HasColumnType(SmallintType)
                .IsRequired();

            messageBuilder.Property(m => m.Sender)
                .HasColumnName("message_sender")
                .HasColumnType(VarcharType)
                .HasMaxLength(254)
                .IsRequired();

            messageBuilder.Property(m => m.Subject)
                .HasColumnName("message_subject")
                .HasColumnType(VarcharType)
                .HasMaxLength(998);

            messageBuilder.Property(m => m.Content)
                .HasColumnName("message_content")
                .HasColumnType("text")
                .IsRequired();

            messageBuilder
                .HasOne(m => m.MessageHashSet)
                .WithMany()
                .HasForeignKey("sha256")
                .IsRequired();

            messageBuilder.Property("sha256")
                .HasColumnName("sha256")
                .IsRequired();

            messageBuilder
                .Navigation(m => m.MessageHashSet)
                .AutoInclude();
        });

        builder.OwnsOne(m => m.State, stateBuilder =>
        {
           stateBuilder.Property(m => m.Status)
               .HasColumnName("status")
               .HasColumnType(SmallintType)
               .IsRequired();

           stateBuilder.Property(m => m.Verdict)
               .HasColumnName("verdict")
               .HasColumnType(SmallintType)
               .IsRequired();

           stateBuilder.Property(m => m.ThreatZone)
               .HasColumnName("threat_zone")
               .HasColumnType(SmallintType)
               .IsRequired();
        });

        builder.Navigation(m => m.Message)
            .AutoInclude();

        builder.Navigation(m => m.State)
            .AutoInclude();
    }

    /// <summary>
    /// Configures the schema for the `AttachedFileResults` owned collection of the `MessageAnalysis` entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity, TDependentEntity}"/> used to configure the owned collection.
    /// </param>
    private static void ConfigureAttachedFileResults(
        OwnedNavigationBuilder<MessageAnalysis, DataAssessmentResult<FileMetadata>> builder)
    {
        builder.ToTable("attached_file_results");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("attached_file_result_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.Property(a => a.Type)
            .HasColumnName("data_type")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.OwnsOne(a => a.Value, metadataBuilder =>
        {
            metadataBuilder.Property(m => m.Name)
                .HasColumnName("file_name")
                .HasColumnType(VarcharType)
                .HasMaxLength(100)
                .IsRequired();

            metadataBuilder.Property(m => m.Size)
                .HasColumnName("size")
                .HasColumnType("bigint")
                .IsRequired();

            metadataBuilder.Property(m => m.ContentType)
                .HasColumnName("content_type")
                .HasColumnType(VarcharType)
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.Property(a => a.ResultId)
            .HasColumnName("file_multi_analysis_id")
            .HasColumnType("uuid")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.WithOwner()
            .HasForeignKey(MessageAnalysisIdColumnName);
    }

    /// <summary>
    /// Configures the schema for the `DetectedUrlResults` owned collection of the `MessageAnalysis` entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity, TDependentEntity}"/> used to configure the owned collection.
    /// </param>
    private static void ConfigureDetectedUrlResults(
        OwnedNavigationBuilder<MessageAnalysis, DataAssessmentResult<Uri>> builder)
    {
        builder.ToTable("detected_url_results");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("detected_url_results_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.Property(a => a.Type)
            .HasColumnName("data_type")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(u => u.Value)
            .HasColumnName("url")
            .HasColumnType(VarcharType)
            .HasMaxLength(2083)
            .IsRequired()
            .HasConversion(
                u => u.AbsoluteUri,
                dbValue => new Uri(dbValue));

        builder.Property(a => a.ResultId)
            .HasColumnName("url_multi_analysis_id")
            .HasColumnType("uuid")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.WithOwner()
            .HasForeignKey(MessageAnalysisIdColumnName);
    }

    /// <summary>
    /// Configures the schema for a collection of detected string results owned by the `MessageAnalysis` entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity, TDependentEntity}"/> used to configure the owned collection.
    /// </param>
    /// <param name="tableName">
    /// The name of the database table for the detected string results.
    /// </param>
    /// <param name="idColumnName">
    /// The name of the column representing the unique identifier for each detected string result.
    /// </param>
    /// <param name="valueColumnName">
    /// The name of the column representing the detected string value.
    /// </param>
    /// <param name="resultIdColumnName">
    /// The name of the column representing the result identifier for the detected string.
    /// </param>
    private static void ConfigureDetectedStringResults(
        OwnedNavigationBuilder<MessageAnalysis, DataAssessmentResult<string>> builder,
        string tableName,
        string idColumnName,
        string valueColumnName,
        string resultIdColumnName)
    {
        builder.ToTable(tableName);

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName(idColumnName)
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.Property(a => a.Type)
            .HasColumnName("data_type")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(u => u.Value)
            .HasColumnName(valueColumnName)
            .HasColumnType(VarcharType)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(a => a.ResultId)
            .HasColumnName(resultIdColumnName)
            .HasColumnType("uuid")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.WithOwner()
            .HasForeignKey(MessageAnalysisIdColumnName);
    }
}
using System.Net.Mail;

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
        builder.OwnsMany(d => d.AttachedFilesResults, ConfigureFileAttachedResults);
        builder.OwnsMany(d => d.DetectedUrlsResults, ConfigureDetectedUrlResults);
        builder.OwnsMany(d => d.DetectedEmailAddressesResults, ConfigureDetectedEmailAddressesResults);
        builder.OwnsMany(d => d.DetectedPhoneNumbersResults, ConfigureDetectedPhoneNumberResults);
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
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Property(m => m.IsPrivate)
            .HasColumnName("is_private")
            .HasColumnType("boolean")
            .IsRequired();

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
                .HasOne(m => m.MessageHashValues)
                .WithMany()
                .HasForeignKey("sha256")
                .IsRequired();

            messageBuilder.Property("sha256")
                .HasColumnName("sha256")
                .IsRequired();

            messageBuilder
                .Navigation(m => m.MessageHashValues)
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

        builder.Property(u => u.UserId)
            .HasColumnName("user_id")
            .HasColumnType("varchar")
            .HasMaxLength(450)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Navigation(m => m.Message)
            .AutoInclude();

        builder.Navigation(m => m.State)
            .AutoInclude();
    }

    /// <summary>
    /// Configures the schema for a detected data result of type <typeparamref name="TDataType"/>.
    /// </summary>
    /// <typeparam name="TDataType">The type of the data being assessed.</typeparam>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}"/> used to configure the owned navigation property.
    /// </param>
    /// <param name="tableName">The name of the database table for the detected data result.</param>
    /// <param name="idColumnName">The name of the column representing the unique identifier of the detected data result.</param>
    /// <param name="resultIdColumnName">The name of the column representing the result identifier of the detected data result.</param>
    /// <param name="configureValueColumnName">
    /// An action to configure the value column of the detected data result.
    /// </param>
    private static void ConfigureDetectedDataResult<TDataType>(
        OwnedNavigationBuilder<MessageAnalysis, DataAssessmentResult<TDataType>> builder,
        string tableName,
        string idColumnName,
        string resultIdColumnName,
        Action<OwnedNavigationBuilder<MessageAnalysis, DataAssessmentResult<TDataType>>> configureValueColumnName)
        where TDataType : notnull
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
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Property(a => a.Type)
            .HasColumnName("data_type")
            .HasColumnType(SmallintType)
            .IsRequired();

        configureValueColumnName(builder);

        builder.Property(a => a.ResultId)
            .HasColumnName(resultIdColumnName)
            .HasColumnType("uuid")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.WithOwner()
            .HasForeignKey(MessageAnalysisIdColumnName);
    }

    /// <summary>
    /// Configures the schema for the attached file results of the <see cref="MessageAnalysis"/> entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}"/> used to configure the owned navigation property.
    /// </param>
    private static void ConfigureFileAttachedResults(
        OwnedNavigationBuilder<MessageAnalysis, DataAssessmentResult<FileMetadata>> builder)
    {
        ConfigureDetectedDataResult(
            builder,
            tableName: "attached_file_results",
            idColumnName: "attached_file_result_id",
            resultIdColumnName: "file_multi_analysis_id",
            valueBuilder =>
                valueBuilder.OwnsOne(
                    a => a.Value, metadataBuilder =>
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
                            .HasColumnType("text")
                            .IsRequired();
                    }));
    }

    /// <summary>
    /// Configures the schema for the detected URL results of the <see cref="MessageAnalysis"/> entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}"/> used to configure the owned navigation property.
    /// </param>
    private static void ConfigureDetectedUrlResults(
        OwnedNavigationBuilder<MessageAnalysis, DataAssessmentResult<Uri>> builder)
    {
        ConfigureDetectedDataResult(
            builder,
            tableName: "detected_url_results",
            idColumnName: "detected_url_results_id",
            resultIdColumnName: "url_multi_analysis_id",
            valueBuilder => valueBuilder.Property(u => u.Value)
                .HasColumnName("url")
                .HasColumnType(VarcharType)
                .HasMaxLength(2083)
                .IsRequired()
                .HasConversion(
                    u => u.AbsoluteUri,
                    dbValue => new Uri(dbValue)));
    }

    /// <summary>
    /// Configures the schema for the detected email address results of the <see cref="MessageAnalysis"/> entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}"/> used to configure the owned navigation property.
    /// </param>
    private static void ConfigureDetectedEmailAddressesResults(
        OwnedNavigationBuilder<MessageAnalysis, DataAssessmentResult<MailAddress>> builder)
    {
        ConfigureDetectedDataResult(
            builder,
            tableName: "detected_email_address_results",
            idColumnName: "detected_email_address_results_id",
            resultIdColumnName: "email_address_multi_reputation_id",
            configureValueColumnName: valueBuilder => valueBuilder.Property(u => u.Value)
                .HasColumnName("email_address")
                .HasColumnType(VarcharType)
                .HasMaxLength(254)
                .IsRequired()
                .HasConversion(
                    emailAddress => emailAddress.Address,
                    dbValue => new MailAddress(dbValue)));
    }

    /// <summary>
    /// Configures the schema for the detected phone number results of the <see cref="MessageAnalysis"/> entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}"/> used to configure the owned navigation property.
    /// </param>
    private static void ConfigureDetectedPhoneNumberResults(
        OwnedNavigationBuilder<MessageAnalysis, DataAssessmentResult<string>> builder)
    {
        ConfigureDetectedDataResult(
            builder,
            tableName: "detected_phone_number_results",
            idColumnName: "detected_phone_number_results_id",
            resultIdColumnName: "phone_multi_reputation_id",
            valueBuilder => valueBuilder.Property(u => u.Value)
                .HasColumnName("phone_number")
                .HasColumnType(VarcharType)
                .HasMaxLength(15)
                .IsRequired());
    }
}
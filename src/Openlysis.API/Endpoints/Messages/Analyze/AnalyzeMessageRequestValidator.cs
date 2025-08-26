using FastEndpoints;

using FluentValidation;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

using Openlysis.API.Configuration.Options;
using Openlysis.Application.Common.Models;

namespace Openlysis.API.Endpoints.Messages.Analyze;

/// <summary>
/// Validator for the <see cref="AnalyzeMessageRequest"/> class.
/// Ensures that the required fields in the request are properly validated.
/// </summary>
public sealed class AnalyzeMessageRequestValidator : Validator<AnalyzeMessageRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeMessageRequestValidator"/> class.
    /// </summary>
    /// <param name="messageAnalysisOptions">
    /// An instance of <see cref="IOptionsMonitor{TOptions}"/> for monitoring changes to
    /// <see cref="MessageAnalysisOptions"/> configuration.
    /// </param>
    /// <param name="formOptions">An instance of <see cref="IOptions{FormOptions}"/> for accessing form configuration options.</param>
    public AnalyzeMessageRequestValidator(
        IOptions<MessageAnalysisOptions> messageAnalysisOptions,
        IOptions<FormOptions> formOptions)
    {
        RuleFor(x => x.MessageType)
            .NotNull()
            .WithMessage("Type of the message must be provided.");

        RuleFor(x => x.Sender)
            .NotEmpty()
            .WithMessage("Sender of the message must be provided.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content of the message must be provided.");

        RuleFor(x => x.CountryCode)
            .Length(2)
            .WithMessage("Country code must be in ISO 1366 alpha-2 format.");

        RuleFor(x => x.AttachedFiles)
            .Must(AttachedFilesWithUniqueNames)
            .WithMessage("All attached files must have unique names.")
            .Must(NotAttachedFilesZeroWithLength)
            .WithMessage("All files must have length at least higher than zero.")
            .Must(a => AttachedFilesNotExceedCountLimit(a, messageAnalysisOptions.Value))
            .WithMessage($"The number of attached files must not exceed the limit of {messageAnalysisOptions.Value.MaxAttachedFiles}.")
            .Must(a => AttachedFilesNotExceedSizeLimit(a, formOptions.Value))
            .WithMessage($"Each attached file must not exceed the maximum allowed size of {formOptions.Value.MultipartBodyLengthLimit} bytes.");

        RuleFor(x => x.AttachedFilesPasswords)
            .Must(NotEmptyPasswords)
            .WithMessage("Each password provided must not be empty.")
            .Must(MatchAttachedFilesPasswords)
            .WithMessage("Each password must correspond to a valid attached file name.");
    }

    /// <summary>
    /// Verifies that all attached files in the collection have a length greater than zero.
    /// If the collection is null or empty, the validation passes.
    /// </summary>
    /// <param name="attachedFiles">The collection of attached files to validate.</param>
    /// <returns>
    /// True if all files in the collection have a length greater than zero or if the collection is null/empty; otherwise, false.
    /// </returns>
    private static bool NotAttachedFilesZeroWithLength(ProcessedFile[] attachedFiles)
    {
        return attachedFiles.All(file => file.Metadata.Size > 0);
    }

    /// <summary>
    /// Validates that the number of attached files does not exceed the maximum limit specified
    /// in the file upload options.
    /// </summary>
    /// <param name="attachedFiles">The collection of attached files to validate.</param>
    /// <param name="messageAnalysisOptions">
    /// An instance of <see cref="IOptionsMonitor{TOptions}"/> for monitoring changes to
    /// <see cref="MessageAnalysisOptions"/> configuration.
    /// </param>
    /// <returns>
    /// True if the number of attached files is less than or equal to the maximum limit; otherwise, false.
    /// </returns>
    private static bool AttachedFilesNotExceedCountLimit(
        ProcessedFile[] attachedFiles,
        MessageAnalysisOptions messageAnalysisOptions)
    {
        return attachedFiles.Length <= messageAnalysisOptions.MaxAttachedFiles;
    }

    /// <summary>
    /// Validates that the size of each attached file does not exceed the multipart body length limit
    /// specified in the provided <see cref="FormOptions"/>.
    /// </summary>
    /// <param name="attachedFiles">The collection of attached files to validate.</param>
    /// <param name="formOptions">The form options containing the multipart body length limit.</param>
    /// <returns>
    /// True if all attached files are within the size limit; otherwise, false.
    /// </returns>
    private static bool AttachedFilesNotExceedSizeLimit(
        ProcessedFile[] attachedFiles,
        FormOptions formOptions)
    {
        return attachedFiles.All(f => f.Metadata.Size <= formOptions.MultipartBodyLengthLimit);
    }

    /// <summary>
    /// Checks if all attached files have unique names.
    /// Returns true if the collection is null or all file names are unique; otherwise, false.
    /// </summary>
    private static bool AttachedFilesWithUniqueNames(ProcessedFile[] attachedFiles)
    {
        return attachedFiles
            .Select(f => f.Metadata.Name)
            .Distinct()
            .Count() == attachedFiles.Length;
    }

    /// <summary>
    /// Validates that all passwords in the provided dictionary are non-empty.
    /// If the dictionary is null, the validation passes.
    /// </summary>
    /// <param name="passwords">A dictionary where keys are file names and values are their respective passwords.</param>
    /// <returns>
    /// True if all passwords are non-empty or if the dictionary is null; otherwise, false.
    /// </returns>
    private static bool NotEmptyPasswords(Dictionary<string, string> passwords)
    {
        return passwords.Values.All(p => !string.IsNullOrEmpty(p));
    }

    /// <summary>
    /// Validates that each password in the provided dictionary corresponds to a valid attached file name
    /// in the request's attached files collection.
    /// If either the attached files or the passwords dictionary is null, the validation passes.
    /// </summary>
    /// <param name="request">The request containing the attached files to validate against.</param>
    /// <param name="passwords">A dictionary where keys are file names and values are their respective passwords.</param>
    /// <returns>
    /// True if each password corresponds to a valid attached file name or if either collection is null; otherwise, false.
    /// </returns>
    private static bool MatchAttachedFilesPasswords(
        AnalyzeMessageRequest request,
        Dictionary<string, string> passwords)
    {
        if (request.AttachedFiles.Length is 0 || passwords.Count is 0)
        {
            return true;
        }

        ProcessedFile[] distinctAttachedFiles = request.AttachedFiles
            .DistinctBy(r => r.Metadata.Name)
            .ToArray();

        return passwords.Keys
            .All(k => distinctAttachedFiles
                .SingleOrDefault(f => f.Metadata.Name == k) is not null);
    }
}
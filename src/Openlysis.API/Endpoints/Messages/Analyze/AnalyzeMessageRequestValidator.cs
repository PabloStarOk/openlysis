using FastEndpoints;

using FluentValidation;

using Microsoft.Extensions.Options;

using Openlysis.API.Configuration.Options;

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
    /// <param name="fileUploadOptions">
    /// An instance of <see cref="IOptionsMonitor{TOptions}"/> for monitoring changes to
    /// <see cref="FileUploadOptions"/> configuration.
    /// </param>
    public AnalyzeMessageRequestValidator(
        IOptionsMonitor<FileUploadOptions> fileUploadOptions)
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
            .Must(NotAttachedFilesZeroWithLength)
            .WithMessage("All files must have length at least higher than zero.")
            .Must(a => AttachedFilesNotExceedLimit(a, fileUploadOptions))
            .WithMessage($"The number of attached files must not exceed the limit of {fileUploadOptions.CurrentValue.MaxFileUploadsLimit}.");

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
    private static bool NotAttachedFilesZeroWithLength(
        IFormFileCollection? attachedFiles)
    {
        if (attachedFiles is null)
        {
            return true;
        }

        return attachedFiles.Count is 0
            || attachedFiles.All(file => file.Length > 0);
    }

    /// <summary>
    /// Validates that the number of attached files does not exceed the maximum limit specified
    /// in the file upload options.
    /// </summary>
    /// <param name="attachedFiles">The collection of attached files to validate.</param>
    /// <param name="fileUploadOptions">
    /// An instance of <see cref="IOptionsMonitor{TOptions}"/> for monitoring changes to
    /// <see cref="FileUploadOptions"/> configuration.
    /// </param>
    /// <returns>
    /// True if the number of attached files is less than or equal to the maximum limit; otherwise, false.
    /// </returns>
    private static bool AttachedFilesNotExceedLimit(
        IFormFileCollection? attachedFiles,
        IOptionsMonitor<FileUploadOptions> fileUploadOptions)
    {
        return attachedFiles is null || attachedFiles.Count
            <= fileUploadOptions.CurrentValue.MaxFileUploadsLimit;
    }

    /// <summary>
    /// Validates that all passwords in the provided dictionary are non-empty.
    /// If the dictionary is null, the validation passes.
    /// </summary>
    /// <param name="passwords">A dictionary where keys are file names and values are their respective passwords.</param>
    /// <returns>
    /// True if all passwords are non-empty or if the dictionary is null; otherwise, false.
    /// </returns>
    private static bool NotEmptyPasswords(
        Dictionary<string, string>? passwords)
    {
        return passwords is null
            || passwords.Values.All(p => !string.IsNullOrEmpty(p));
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
        Dictionary<string, string>? passwords)
    {
        if (request.AttachedFiles is null
            || passwords is null)
        {
            return true;
        }

        return passwords.Keys
            .All(k => request.AttachedFiles
                .SingleOrDefault(f => f.FileName == k) is not null);
    }
}
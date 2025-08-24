using System.Buffers;
using System.Text.Json;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

using Openlysis.API.Endpoints.Messages.Analyze;
using Openlysis.API.Services.Abstractions;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Models;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.API.Services.Implementations;

/// <summary>
/// Binds and parses multipart requests for <see cref="AnalyzeMessageRequest"/>.
/// </summary>
internal sealed class AnalyzeMessageMultipartRequestBinder : MultipartRequestBinder<AnalyzeMessageRequest>
{
    private readonly List<ProcessedFile> _attachedFiles = [];
    private MessageType? _messageType;
    private string _sender = string.Empty;
    private string _content = string.Empty;
    private string? _subject;
    private Dictionary<string, string> _attachedFilesPasswords = [];
    private string? _countryCode;
    private bool _isPrivate = AnalyzeMessageRequest.DefaultIsPrivate;
    private bool _reanalyze = AnalyzeMessageRequest.DefaultReanalyze;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeMessageMultipartRequestBinder"/> class.
    /// </summary>
    /// <param name="fileStorageContext">The file storage context for handling file operations.</param>
    /// <param name="memoryPool">The memory pool used for buffering multipart data.</param>
    /// <param name="formOptions">The form options for multipart parsing.</param>
    public AnalyzeMessageMultipartRequestBinder(
        IFileStorageContext fileStorageContext,
        MemoryPool<byte> memoryPool,
        IOptions<FormOptions> formOptions)
        : base(fileStorageContext, memoryPool, formOptions)
    {
    }

    /// <inheritdoc/>
    protected override async Task ParseFormSectionAsync(FormMultipartSection section, CancellationToken cancellationToken)
    {
        if (IsSectionNameEqual(section, nameof(AnalyzeMessageRequest.MessageType)))
        {
            string valueString = await GetSectionValueAsync(section, cancellationToken);
            if (Enum.TryParse(valueString, ignoreCase: true, out MessageType value))
            {
                _messageType = value;
            }
        }

        if (IsSectionNameEqual(section, nameof(AnalyzeMessageRequest.Sender)))
        {
            _sender = await GetSectionValueAsync(section, cancellationToken);
        }

        if (IsSectionNameEqual(section, nameof(AnalyzeMessageRequest.Subject)))
        {
            _subject = await GetSectionValueAsync(section, cancellationToken);
        }

        if (IsSectionNameEqual(section, nameof(AnalyzeMessageRequest.Content)))
        {
            _content = await GetSectionValueAsync(section, cancellationToken);
        }

        if (IsSectionNameEqual(section, nameof(AnalyzeMessageRequest.AttachedFilesPasswords)))
        {
            string valueString = await GetSectionValueAsync(section, cancellationToken);
            var passwords = JsonSerializer.Deserialize<Dictionary<string, string>>(
                valueString,
                SerializerOptions);
            if (passwords is not null)
            {
                _attachedFilesPasswords = passwords;
            }
        }

        if (IsSectionNameEqual(section, nameof(AnalyzeMessageRequest.IsPrivate)))
        {
            _isPrivate = await ParseSectionAsBoolAsync(section, cancellationToken) ?? _isPrivate;
        }

        if (IsSectionNameEqual(section, nameof(AnalyzeMessageRequest.Reanalyze)))
        {
            _reanalyze = await ParseSectionAsBoolAsync(section, cancellationToken) ?? _reanalyze;
        }

        if (IsSectionNameEqual(section, nameof(AnalyzeMessageRequest.CountryCode)))
        {
            _countryCode = await GetSectionValueAsync(section, cancellationToken);
        }
    }

    /// <inheritdoc/>
    protected override async Task ParseFileSectionAsync(FileMultipartSection section, CancellationToken cancellationToken)
    {
        if (!IsSectionNameEqual(section, nameof(AnalyzeMessageRequest.AttachedFiles))
            || section.FileStream is null)
        {
            return;
        }

        ProcessedFile processedFile = await FileStorageContext.ProcessAsync(
            section.FileName,
            section.Section.ContentType,
            section.FileStream,
            cancellationToken);
        _attachedFiles.Add(processedFile);
    }

    /// <inheritdoc/>
    protected override AnalyzeMessageRequest CreateRequest()
    {
        return new AnalyzeMessageRequest(
            UserId,
            _messageType,
            _sender,
            _content,
            _subject,
            _attachedFiles.ToArray(),
            _attachedFilesPasswords,
            _countryCode,
            _isPrivate,
            _reanalyze);
    }
}
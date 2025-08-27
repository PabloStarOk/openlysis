using System.Buffers;
using System.Text;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

using Openlysis.API.Endpoints.Files.Analyze;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Models;

namespace Openlysis.API.Binders;

/// <summary>
/// Parses multipart requests for analyzing files, extracting relevant fields and processing the uploaded file.
/// </summary>
internal sealed class AnalyzeFileMultipartRequestBinder : MultipartRequestBinder<AnalyzeFileRequest>
{
    private ProcessedFile _processedFile = null!;
    private bool _fileProcessed;
    private string _filePassword = string.Empty;
    private bool? _isPrivate;
    private bool? _reanalyze;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileMultipartRequestBinder"/> class.
    /// </summary>
    /// <param name="fileStorageContext">The file storage context for handling file operations.</param>
    /// <param name="memoryPool">The memory pool used for buffer management.</param>
    /// <param name="stringBuilderPool">The string builder pool for efficient string operations.</param>
    /// <param name="formOptions">The form options for multipart request limits and settings.</param>
    public AnalyzeFileMultipartRequestBinder(
        IFileStorageContext fileStorageContext,
        MemoryPool<byte> memoryPool,
        ObjectPool<StringBuilder> stringBuilderPool,
        IOptions<FormOptions> formOptions)
        : base(fileStorageContext, memoryPool, stringBuilderPool, formOptions)
    {
    }

    /// <inheritdoc/>
    protected override async Task ParseFormSectionAsync(FormMultipartSection section, CancellationToken cancellationToken)
    {
        if (IsSectionNameEqual(section, nameof(AnalyzeFileRequest.Password)))
        {
            _filePassword = await section.GetValueAsync(cancellationToken);
        }

        if (IsSectionNameEqual(section, nameof(AnalyzeFileRequest.IsPrivate)))
        {
            _isPrivate = await ParseSectionAsBoolAsync(section, cancellationToken);
        }

        if (IsSectionNameEqual(section, nameof(AnalyzeFileRequest.Reanalyze)))
        {
            _reanalyze = await ParseSectionAsBoolAsync(section, cancellationToken);
        }
    }

    /// <inheritdoc/>
    protected override async Task ParseFileSectionAsync(FileMultipartSection section, CancellationToken cancellationToken)
    {
        if (!IsSectionNameEqual(section, nameof(AnalyzeFileRequest.File))
            || _fileProcessed
            || section.FileStream is null)
        {
                return;
        }

        _processedFile = await FileStorageContext.ProcessAsync(
            section.FileName,
            section.Section.ContentType,
            section.FileStream,
            cancellationToken);
        _fileProcessed = true;
    }

    /// <inheritdoc/>
    protected override AnalyzeFileRequest CreateRequest()
    {
        return new AnalyzeFileRequest(
            UserId,
            _processedFile,
            _filePassword,
            _isPrivate ?? AnalyzeFileRequest.DefaultIsPrivate,
            _reanalyze ?? AnalyzeFileRequest.DefaultReanalyze);
    }
}
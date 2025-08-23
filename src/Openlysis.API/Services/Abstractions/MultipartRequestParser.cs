using System.Buffers;
using System.Net.Http.Headers;
using System.Text;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Abstractions.Services;

using MultipartSection = FastEndpoints.MultipartSection;

namespace Openlysis.API.Services.Abstractions;

/// <summary>
/// Abstract base class for parsing multipart requests into a strongly-typed <typeparamref name="TRequest"/> allowing file streaming.
/// </summary>
/// <typeparam name="TRequest">The type of request to be created from multipart data.</typeparam>
public abstract class MultipartRequestParser<TRequest>
    where TRequest : class
{
    private const int SectionReadBufferSize = 16384;
    private const int InsecureUtf7EncodingCodePage = 65000;

    /// <summary>
    /// Gets the file storage context used for handling file operations in multipart requests.
    /// </summary>
    protected IFileStorageContext FileStorageContext { get; }

    private readonly MemoryPool<byte> _memoryPool;
    private readonly IOptions<FormOptions> _formOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipartRequestParser{TRequest}"/> class.
    /// </summary>
    /// <param name="fileStorageContext">The file storage context for handling file operations.</param>
    /// <param name="memoryPool">The memory pool used for buffer management.</param>
    /// <param name="formOptions">The form options for multipart request limits and settings.</param>
    protected MultipartRequestParser(
        IFileStorageContext fileStorageContext,
        MemoryPool<byte> memoryPool,
        IOptions<FormOptions> formOptions)
    {
        _memoryPool = memoryPool;
        _formOptions = formOptions;
        FileStorageContext = fileStorageContext;
    }

    /// <summary>
    /// Parses the provided multipart sections asynchronously and constructs a strongly-typed <typeparamref name="TRequest"/>.
    /// </summary>
    /// <param name="sections">An async enumerable of multipart sections to parse.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The constructed <typeparamref name="TRequest"/> from the parsed sections.</returns>
    public async Task<TRequest> ParseAsync(
        IAsyncEnumerable<MultipartSection> sections,
        CancellationToken cancellationToken)
    {
        int sectionsRead = 0;

        await foreach (var section in sections.WithCancellation(cancellationToken))
        {
            sectionsRead++;
            if (sectionsRead > _formOptions.Value.ValueCountLimit)
            {
                throw new InvalidDataException("Form entries count limit exceeded.");
            }

            if (section.IsFormSection)
            {
                await ParseFormSectionAsync(section.FormSection, cancellationToken);
            }

            if (section.IsFileSection)
            {
                await ParseFileSectionAsync(section.FileSection, cancellationToken);
            }
        }

        return CreateRequest();
    }

    /// <summary>
    /// Parses a form section asynchronously.
    /// </summary>
    /// <param name="section">The form multipart section to parse.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected abstract Task ParseFormSectionAsync(FormMultipartSection section, CancellationToken cancellationToken);

    /// <summary>
    /// Parses a file section asynchronously.
    /// </summary>
    /// <param name="section">The file multipart section to parse.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected abstract Task ParseFileSectionAsync(FileMultipartSection section, CancellationToken cancellationToken);

    /// <summary>
    /// Creates and returns the strongly-typed request object from parsed data.
    /// </summary>
    /// <returns>The constructed <typeparamref name="TRequest"/>.</returns>
    protected abstract TRequest CreateRequest();

    /// <summary>
    /// Parses the value of a form section as a nullable boolean.
    /// </summary>
    /// <param name="section">The form multipart section to parse.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The parsed boolean value, or null if parsing fails.</returns>
    protected async Task<bool?> ParseSectionAsBoolAsync(
        FormMultipartSection section,
        CancellationToken cancellationToken)
    {
        var stringValue = await GetSectionValueAsync(section, cancellationToken);
        return bool.TryParse(stringValue, out bool result) ? result : null;
    }

    /// <summary>
    /// Checks if the name of a file section matches the specified name (case-insensitive).
    /// </summary>
    /// <param name="section">The file multipart section.</param>
    /// <param name="name">The name to compare.</param>
    /// <returns>True if the names are equal; otherwise, false.</returns>
    protected bool IsSectionNameEqual(FileMultipartSection section, string name)
    {
        return StringComparer.InvariantCultureIgnoreCase.Equals(section.Name, name);
    }

    /// <summary>
    /// Checks if the name of a form section matches the specified name (case-insensitive).
    /// </summary>
    /// <param name="section">The form multipart section.</param>
    /// <param name="name">The name to compare.</param>
    /// <returns>True if the names are equal; otherwise, false.</returns>
    protected bool IsSectionNameEqual(FormMultipartSection section, string name)
    {
        return StringComparer.InvariantCultureIgnoreCase.Equals(section.Name, name);
    }

    /// <summary>
    /// Asynchronously reads the value of a form multipart section as a string, validating against <see cref="FormOptions"/> limits.
    /// </summary>
    /// <param name="formSection">The form multipart section to read from.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The decoded string value of the section, or throws if validation fails.</returns>
    /// <exception cref="InvalidDataException">Thrown when the section value exceeds allowed length.</exception>
    private async Task<string> GetSectionValueAsync(
        FormMultipartSection formSection,
        CancellationToken cancellationToken)
    {
        var body = formSection.Section.Body;

        _ = MediaTypeHeaderValue.TryParse(formSection.Section.ContentType, out var sectionMediaType);

        Console.WriteLine(sectionMediaType?.CharSet);
        var streamEncoding = sectionMediaType?.CharSet is not null
            ? Encoding.GetEncoding(sectionMediaType.CharSet)
            : Encoding.UTF8;

        if (streamEncoding.CodePage is InsecureUtf7EncodingCodePage)
        {
            streamEncoding = Encoding.UTF8;
        }

        var stringBuilder = new StringBuilder();

        using var memoryOwner = _memoryPool.Rent(SectionReadBufferSize);
        Memory<byte> buffer = memoryOwner.Memory;

        int totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await body.ReadAsync(buffer, cancellationToken)) > 0)
        {
            totalBytesRead += bytesRead;
            if (totalBytesRead > _formOptions.Value.ValueLengthLimit)
            {
                throw new InvalidDataException("Section value exceeds allowed length.");
            }

            var bytes = buffer[..bytesRead];
            stringBuilder.Append(streamEncoding.GetString(bytes.Span));
        }

        return stringBuilder.ToString();
    }
}
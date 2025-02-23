using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using ErrorOr;

using Filescan.Client.Abstractions;
using Filescan.Client.Constants.Common;
using Filescan.Client.Constants.Endpoints;
using Filescan.Client.Models.Common;
using Filescan.Client.Models.Options;
using Filescan.Client.Models.Requests;

using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Filescan.Client.Services;

/// <summary>
/// Client to scan files.
/// </summary>
public sealed class FileScanner : IFileScannerService, IDisposable
{
    private readonly ApiOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ModelParser<FilescanError, JsonElement> _errorParser;
    private readonly ModelParser<ServiceFileAnalysisId, JsonElement> _analysisIdParser;
    private readonly ModelParser<ServiceFileAnalysis, JsonElement> _analysisParser;
    private readonly bool _disposeHttpClient;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileScanner"/> class.
    /// </summary>
    /// <param name="httpClient">An <see cref="HttpClient"/> to send requests.</param>
    /// <param name="options">The API options containing the API key.</param>
    /// <param name="errorParser">A parser to create <see cref="FilescanError"/> objects from <see cref="HttpResponseMessage"/>.</param>
    /// <param name="analysisIdParser">A parser to create <see cref="ServiceFileAnalysisId"/> objects from <see cref="HttpResponseMessage"/>.</param>
    /// <param name="analysisParser">A parser to create <see cref="ServiceFileAnalysis"/> objects from <see cref="HttpResponseMessage"/>.</param>
    /// <param name="disposeHttpClient">If the given <see cref="HttpClient"/> should be disposed along with this class.</param>
    public FileScanner(
        ApiOptions options,
        HttpClient httpClient,
        ModelParser<FilescanError, JsonElement> errorParser,
        ModelParser<ServiceFileAnalysisId, JsonElement> analysisIdParser,
        ModelParser<ServiceFileAnalysis, JsonElement> analysisParser,
        bool disposeHttpClient = false)
    {
        _options = options;
        _httpClient = httpClient;
        _errorParser = errorParser;
        _analysisIdParser = analysisIdParser;
        _analysisParser = analysisParser;
        _disposeHttpClient = disposeHttpClient;
        ConfigureHttpClient();
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<ServiceFileAnalysisId>> UploadAsync(ScanRequest scanRequest, CancellationToken cancellationToken = default)
    {
        var formDataContent = new MultipartFormDataContent();

        // Add file
        var fileStreamContent = new StreamContent(scanRequest.FileData);
        fileStreamContent.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse(scanRequest.FileMimeType);
        formDataContent.Add(fileStreamContent, ScanFieldNames.File, scanRequest.FileName);

        // Add not required fields
        using StringContent descriptionContent = AddStringContent(formDataContent, scanRequest.Description, ScanFieldNames.Description);
        var tags = string.Join('|', scanRequest.Tags ?? []);
        using StringContent tagsContent = AddStringContent(formDataContent, tags, ScanFieldNames.Tags);
        using StringContent propagateTagsContent = AddStringContent(formDataContent, scanRequest.PropagateTags, ScanFieldNames.PropagateTags);
        using StringContent passwordContent = AddStringContent(formDataContent, scanRequest.Password, ScanFieldNames.Password);
        using StringContent privateFileContent = AddStringContent(formDataContent, scanRequest.IsPrivateFile, ScanFieldNames.IsPrivateFile);
        using StringContent privateReportContent = AddStringContent(formDataContent, scanRequest.IsPrivateReport, ScanFieldNames.IsPrivateReport);
        using StringContent skipWhitelistedContent = AddStringContent(formDataContent, scanRequest.SkipWhiteListed, ScanFieldNames.SkipWhiteListed);
        using StringContent scanProfileContent = AddStringContent(formDataContent, scanRequest.ScanProfile, ScanFieldNames.ScanProfile);
        using StringContent rapidModeContent = AddStringContent(formDataContent, scanRequest.Options?.RapidMode, ScanFieldNames.RapidMode);
        using StringContent earlyTerminationContent = AddStringContent(formDataContent, scanRequest.Options?.EarlyTermination, ScanFieldNames.EarlyTermination);
        using StringContent osintContent = AddStringContent(formDataContent, scanRequest.Options?.Osint, ScanFieldNames.Osint);
        using StringContent extendedOsintContent = AddStringContent(formDataContent, scanRequest.Options?.ExtendedOsint, ScanFieldNames.ExtendedOsint);
        using StringContent extractedFilesOsintContent = AddStringContent(formDataContent, scanRequest.Options?.ExtractedFilesOsint, ScanFieldNames.ExtractedFilesOsint);
        using StringContent visualizationContent = AddStringContent(formDataContent, scanRequest.Options?.Visualization, ScanFieldNames.Visualization);
        using StringContent filesDownloadContent = AddStringContent(formDataContent, scanRequest.Options?.FilesDownload, ScanFieldNames.FilesDownload);
        using StringContent resolveDomainsContent = AddStringContent(formDataContent, scanRequest.Options?.ResolveDomains, ScanFieldNames.ResolveDomains);
        using StringContent inputFileYaraContent = AddStringContent(formDataContent, scanRequest.Options?.InputFileYara, ScanFieldNames.InputFileYara);
        using StringContent extractedFilesYaraContent = AddStringContent(formDataContent, scanRequest.Options?.ExtractedFilesYara, ScanFieldNames.ExtractedFilesYara);
        using StringContent whoisContent = AddStringContent(formDataContent, scanRequest.Options?.WhoIs, ScanFieldNames.Whois);
        using StringContent ipsMetaContent = AddStringContent(formDataContent, scanRequest.Options?.IpsMeta, ScanFieldNames.IpsMeta);
        using StringContent imagesOcrContent = AddStringContent(formDataContent, scanRequest.Options?.ImagesOcr, ScanFieldNames.ImagesOcr);
        using StringContent certificatesContent = AddStringContent(formDataContent, scanRequest.Options?.Certificates, ScanFieldNames.Certificates);
        using StringContent urlAnalysisContent = AddStringContent(formDataContent, scanRequest.Options?.UrlAnalysis, ScanFieldNames.UrlAnalysis);
        using StringContent extractStringsContent = AddStringContent(formDataContent, scanRequest.Options?.ExtractStrings, ScanFieldNames.ExtractStrings);
        using StringContent ocrQrContent = AddStringContent(formDataContent, scanRequest.Options?.OcrQr, ScanFieldNames.OcrQr);
        using StringContent phishingDetectionContent = AddStringContent(formDataContent, scanRequest.Options?.PhishingDetection, ScanFieldNames.PhishingDetection);

        HttpResponseMessage response = await _httpClient.PostAsync(Addresses.ScanFile, formDataContent, cancellationToken).ConfigureAwait(false);
        return await GetResultAsync(_analysisIdParser, response, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<ServiceFileAnalysis>> GetScanAsync(GetScanRequest getScanRequest, CancellationToken cancellationToken = default)
    {
        var uriBuilder = new UriBuilder(Addresses.BaseAddress)
        {
            Path = string.Format(Addresses.GetScan, getScanRequest.FlowId),
        };
        var paramsBuilder = new StringBuilder();

        if (getScanRequest.Filters?.Length > 0)
        {
            paramsBuilder.Append($"filter={string.Join(',', getScanRequest.Filters)}");
        }

        if (getScanRequest.Sorting?.Length > 0)
        {
            paramsBuilder.Append($"&sorting={string.Join(',', getScanRequest.Sorting)}");
        }

        if (getScanRequest.OtherQueryParams?.Length > 0)
        {
            paramsBuilder.Append($"&other={string.Join(',', getScanRequest.OtherQueryParams)}");
        }

        uriBuilder.Query = paramsBuilder.ToString();
        Uri requestUri = uriBuilder.Uri;
        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        return await GetResultAsync(_analysisParser, response, cancellationToken);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Adds a content part to a multipart form data content.
    /// </summary>
    /// <param name="multipartContent">The multipart form data content to which the part will be added.</param>
    /// <param name="objToAdd">The object to be added as a content part. If null, an empty string will be used.</param>
    /// <param name="fieldName">The name of the field for the content part.</param>
    /// <returns>The created <see cref="HttpContent"/> representing the added content part so that it can be disposed with a using statement.</returns>
    private static StringContent AddStringContent(MultipartFormDataContent multipartContent, object? objToAdd, string fieldName)
    {
        string value = objToAdd?.ToString() ?? string.Empty;
        var skipWhitelistedContent = new StringContent(value);
        if (!string.IsNullOrWhiteSpace(value))
        {
            multipartContent.Add(skipWhitelistedContent, fieldName);
        }

        return skipWhitelistedContent;
    }

    /// <summary>
    /// Reads the response body as a JSON element.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the JSON element.</returns>
    private static async Task<JsonElement> GetResponseBodyJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var jsonDocument = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return jsonDocument.RootElement.Clone();
    }

    /// <summary>
    /// Processes the HTTP response and parses the result into a model.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to parse.</typeparam>
    /// <param name="parser">The parser to use for parsing the response.</param>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the parsed model and any error information.</returns>
    private async Task<ErrorOr<TModel>> GetResultAsync<TModel>(
        ModelParser<TModel, JsonElement> parser,
        HttpResponseMessage response,
        CancellationToken cancellationToken)
        where TModel : notnull
    {
        JsonElement rootElement = await GetResponseBodyJsonAsync(response, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return parser.Parse(rootElement);
        }

        FilescanError filescanError = _errorParser.Parse(rootElement);
        List<Error> errors = [];

        if (!filescanError.IsValidationError)
        {
            errors.Add(Error.Failure(description: filescanError.Detail));
            return errors;
        }

        errors.AddRange(filescanError.ValidationErrors
            .Select(validationError =>
                {
                    var metadata = new Dictionary<string, object>
                    {
                        { "type", validationError.Type },
                        { "location", validationError.Location },
                    };

                    return Error.Failure(
                        description: validationError.Message,
                        metadata: metadata);
                }));

        return errors;
    }

    /// <summary>
    /// Configures the http client.
    /// </summary>
    private void ConfigureHttpClient()
    {
        _httpClient.DefaultRequestVersion = _options.HttpVersion;
        _httpClient.Timeout = _options.TimeoutPerRequest;
        _httpClient.BaseAddress = new Uri(Addresses.BaseAddress);
        _httpClient.DefaultRequestHeaders.Add(HeaderNames.ApiKey, _options.ApiKey);
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="FileScanner"/> class.
    /// </summary>
    /// <param name="disposing">A boolean value indicating whether the method is called from the Dispose method (true) or from a finalizer (false).</param>
    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing && _disposeHttpClient)
        {
            _httpClient.Dispose();
        }

        _isDisposed = true;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="FileScanner"/> class.
    /// </summary>
    ~FileScanner()
    {
        Dispose(disposing: false);
    }
}
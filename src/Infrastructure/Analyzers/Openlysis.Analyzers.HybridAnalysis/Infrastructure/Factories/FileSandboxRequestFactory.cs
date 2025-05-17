using System.Net.Http.Headers;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Common;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration.Common;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Factories;

/// <summary>
/// Factory for creating Hybrid Analysis API requests for file submission.
/// Prepares multipart form data with file content, environment settings, and other required parameters.
/// </summary>
internal class FileSandboxRequestFactory : IRequestFactory
{
    private const string FileBodyParamName = "file";
    private const string EnvironmentIdBodyParamName = "environment_id";
    private const string PasswordBodyParamName = "document_password";
    private const string AntiEvasionBodyParamName = "experimental_anti_evasion";

    private readonly HybridAnalyzerOptions _analyzerOptions;
    private readonly AnalyzeFileRequest _request;
    private readonly SandboxEnvironment _osEnvironment;
    private readonly string _mimeType;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSandboxRequestFactory"/> class.
    /// </summary>
    /// <param name="analyzerOptions">The options containing configuration for hybrid analysis.</param>
    /// <param name="request">The file analysis request containing file data and metadata.</param>
    /// <param name="osEnvironment">The sandbox environment configuration to use for analysis.</param>
    /// <param name="mimeType">The MIME type of the file to be analyzed.</param>
    internal FileSandboxRequestFactory(
        HybridAnalyzerOptions analyzerOptions,
        AnalyzeFileRequest request,
        SandboxEnvironment osEnvironment,
        string mimeType)
    {
        _analyzerOptions = analyzerOptions;
        _request = request;
        _osEnvironment = osEnvironment;
        _mimeType = mimeType;
    }

    /// <inheritdoc/>
    public HybridAnalysisAnalyzeRequest Create()
    {
        var content = new MultipartFormDataContent();

        AddFileToContent(content);
        AddPasswordToContent(content);
        AddEnvironmentIdToContent(content);
        AddAntiEvasionToContent(content);

        return new HybridAnalysisAnalyzeRequest(
            Addresses.SandboxSubmitFileEndpoint,
            content);
    }

    /// <summary>
    /// Adds the file data to the multipart form content with appropriate content type.
    /// </summary>
    /// <param name="content">The multipart form data content to which the file will be added.</param>
    private void AddFileToContent(MultipartFormDataContent content)
    {
        var fileContent = new StreamContent(_request.FileData);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(_mimeType);
        content.Add(fileContent, FileBodyParamName, _request.FileName);
    }

    /// <summary>
    /// Adds the password for protected documents to the multipart form content.
    /// Encrypts and transmits the password specified in the file request.
    /// </summary>
    /// <param name="content">The multipart form data content to which the password will be added.</param>
    private void AddPasswordToContent(MultipartFormDataContent content)
    {
        if (string.IsNullOrEmpty(_request.FilePassword))
        {
            return;
        }

        var passwordContent = new StringContent(_request.FilePassword);
        content.Add(passwordContent, PasswordBodyParamName);
    }

    /// <summary>
    /// Adds the sandbox environment ID to the multipart form content.
    /// Uses the default sandbox environment from analyzer options.
    /// </summary>
    /// <param name="content">The multipart form data content to which the environment ID will be added.</param>
    private void AddEnvironmentIdToContent(MultipartFormDataContent content)
    {
        string environmentIdString = _osEnvironment.ToString("D");
        var environmentContent = new StringContent(environmentIdString);
        content.Add(environmentContent, EnvironmentIdBodyParamName);
    }

    /// <summary>
    /// Adds experimental anti-evasion settings to the multipart form content.
    /// Enables advanced detection capabilities for evasive malware.
    /// </summary>
    /// <param name="content">The multipart form data content to which the anti-evasion setting will be added.</param>
    private void AddAntiEvasionToContent(MultipartFormDataContent content)
    {
        bool useAntiEvasion = _analyzerOptions.UseExperimentalAntiEvasion;
        var antiEvasionContent = new StringContent(useAntiEvasion.ToString());
        content.Add(antiEvasionContent, AntiEvasionBodyParamName);
    }
}
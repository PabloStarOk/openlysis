namespace Openlysis.Analyzers.Filescan.Core.Models.Objects;

/// <summary>
/// Represents the options for a scan.
/// </summary>
/// <param name="RapidMode">Indicates if the scan should be in rapid mode.</param>
/// <param name="EarlyTermination">Terminates scan execution once a conclusive verdict is detected.</param>
/// <param name="Osint">Indicates if OSINT (Open Source Intelligence) should be included.</param>
/// <param name="ExtendedOsint">Indicates if extended OSINT should be included.</param>
/// <param name="ExtractedFilesOsint">Indicates if OSINT should be performed on extracted files.</param>
/// <param name="Visualization">Indicates if visualization should be enabled.</param>
/// <param name="FilesDownload">Indicates if files should be downloaded.</param>
/// <param name="ResolveDomains">Indicates if domains should be resolved.</param>
/// <param name="InputFileYara">Indicates if YARA rules should be applied to the input file.</param>
/// <param name="ExtractedFilesYara">Indicates if YARA rules should be applied to extracted files.</param>
/// <param name="WhoIs">Indicates if WHOIS information should be retrieved.</param>
/// <param name="IpsMeta">Indicates if IP metadata should be retrieved.</param>
/// <param name="ImagesOcr">Indicates if OCR (Optical Character Recognition) should be performed on images.</param>
/// <param name="Certificates">Indicates if certificates should be extracted.</param>
/// <param name="UrlAnalysis">Indicates if domain and URL analysis (e.g. geolocation) should be performed.</param>
/// <param name="ExtractStrings">Indicates if string extraction should be enabled.</param>
/// <param name="OcrQr">Indicates if OCR should be used to extract and process QR codes.</param>
/// <param name="PhishingDetection">Indicates if phishing detection should be performed for URLs, emails, and HTML pages.</param>
public record ScanOptions(
    bool RapidMode,
    bool EarlyTermination,
    bool Osint,
    bool ExtendedOsint,
    bool ExtractedFilesOsint,
    bool Visualization,
    bool FilesDownload,
    bool ResolveDomains,
    bool InputFileYara,
    bool ExtractedFilesYara,
    bool WhoIs,
    bool IpsMeta,
    bool ImagesOcr,
    bool Certificates,
    bool UrlAnalysis,
    bool ExtractStrings,
    bool OcrQr,
    bool PhishingDetection)
{
    /// <summary>
    /// Gets a <see cref="ScanOptions"/> instance with all options set to true.
    /// </summary>
    public static ScanOptions True { get; } = new (
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true);
}

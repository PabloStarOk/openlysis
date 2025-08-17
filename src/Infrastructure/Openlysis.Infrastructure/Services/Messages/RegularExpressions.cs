using System.Text.RegularExpressions;

namespace Openlysis.Infrastructure.Services.Messages;

/// <summary>
/// A static partial class containing predefined regular expressions and utility methods for working with them.
/// </summary>
internal static partial class RegularExpressions
{
    [GeneratedRegex(
        @"(?i)\b(?:
        (?:                                      
            (?:https?|ftp|ssh|sftp|gopher|ws|wss)://
            |(?:geo|mailto|tel|sms|data|blob|magnet|urn|news|about|info):
        )
        [^\s<>""']+
        |
        (?<!@)(?:
            (?:www\.|[\w-]+\.)*[\w-]+(?:\.[\w-]+)+
            |\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}
        )
        (?::\d+)?
        (?:/[\w~%@!$&'()*+,;=:./?-]*)?
    )",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace)]
    public static partial Regex Uri();

    [GeneratedRegex(
        @"(?xi)\b[\p{L}\p{N}.!#$%&'*+\/=?^_`{|}~-]+(?<!:\/.\*)@(?!.*\.\.)(?:(?!-)[\p{L}\p{N}-]{1,63}(?<!-)\.)+[\p{L}]{2,63}\b",
        RegexOptions.IgnorePatternWhitespace)]
    public static partial Regex EmailAddress();

    [GeneratedRegex(
        @"(?xi)(?<!\S)(?:\+?(\d{1,3})[\s-]?)?(?:(\d{2,5}[\s-]?){2,5}| \d{3}[\s-]?\d{3}[\s-]?\d{4}| \b(?:911|112|999)\b)(?!\S)",
        RegexOptions.IgnorePatternWhitespace)]
    public static partial Regex PhoneNumber();

    /// <summary>
    /// Retrieves all defined regular expressions, excluding the specified one.
    /// </summary>
    /// <param name="regex">The regular expression to exclude from the result.</param>
    /// <returns>An enumerable collection of regular expressions excluding the specified one.</returns>
    public static IEnumerable<Regex> GetAllExcluding(Regex regex)
    {
        HashSet<Regex> regexes = [
            Uri(),
            EmailAddress(),
            PhoneNumber(),
            UnwantedProtocol(),
            WindowsPath(),
            UnixAbsolutePath(),
            UnixRelativePath(),
            KnownFile(),
            FileExtensionOnly(),
            Ipv4(),
            Ipv6(),
            Version(),
            Currency(),
            Date(),
            Uuid(),
            DataSize(),
        ];
        return regexes.Where(r => r != regex);
    }

    /// <summary>
    /// Matches unwanted protocols such as geo, mailto, tel, and others.
    /// </summary>
    [GeneratedRegex(@"^(?i)(?:geo|mailto|tel|news|magnet|gopher|about|info|file|ftp|sms|blob|data|urn|ws|wss):")]
    private static partial Regex UnwantedProtocol();

    /// <summary>
    /// Matches Windows file paths (e.g., C:\path\to\file).
    /// </summary>
    [GeneratedRegex(@"^[A-Za-z]:\\(?:[^\\\/:*?""<>|\r\n]+\\)*[^\\\/:*?""<>|\r\n]+$")]
    private static partial Regex WindowsPath();

    /// <summary>
    /// Matches Unix absolute file paths (e.g., /path/to/file).
    /// </summary>
    [GeneratedRegex(@"^/(?:[\w@%&\-\./=+]+)$")]
    private static partial Regex UnixAbsolutePath();

    /// <summary>
    /// Matches Unix relative file paths (e.g., ./file or ../file).
    /// </summary>
    [GeneratedRegex(@"^(?:\.\./|\./)[\w@%&\-\./=+]+$")]
    private static partial Regex UnixRelativePath();

    /// <summary>
    /// Matches known file types such as PDF, DOCX, XLSX, TXT, JPG, and PNG.
    /// </summary>
    [GeneratedRegex(
        @"(?<!/)(?<!\w)\b\w+\.(?:pdf|docx?|xlsx?|txt|jpg|png)\b(?!/)(?![-@~$&+=])",
        RegexOptions.IgnoreCase)]
    private static partial Regex KnownFile();

    /// <summary>
    /// Matches file extensions such as EXE, DLL, BAT, CMD, and others.
    /// </summary>
    [GeneratedRegex(@"^(?i)[\w-]+\.(?:exe|dll|bat|cmd|ps1|html?|php|asp(?:x)?|jsp|pdf|docx?|xlsx?|pptx?)[.,;:]?$")]
    private static partial Regex FileExtensionOnly();

    /// <summary>
    /// Matches IPv4 addresses (e.g., 192.168.0.1).
    /// </summary>
    [GeneratedRegex(@"^(?:\d{1,3}\.){3}\d{1,3}[.,;:]?$")]
    private static partial Regex Ipv4();

    /// <summary>
    /// Matches IPv6 addresses (e.g., 2001:0db8:85a3:0000:0000:8a2e:0370:7334).
    /// </summary>
    [GeneratedRegex(@"^(?:[0-9A-Fa-f]{1,4}:){7}[0-9A-Fa-f]{1,4}[.,;:]?$")]
    private static partial Regex Ipv6();

    /// <summary>
    /// Matches version numbers (e.g., v1.0.0 or 1.0.0).
    /// </summary>
    [GeneratedRegex(@"^(?!\d{1,3}(?:\.\d{1,3}){3}$)[vV]?\d+(?:\.\d+)+(?:[.,;:]?)$")]
    private static partial Regex Version();

    /// <summary>
    /// Matches currency values (e.g., $1,000.00).
    /// </summary>
    [GeneratedRegex(@"(?<!(?:https?://|www\.|/))\B\$\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{2})?\b")]
    private static partial Regex Currency();

    /// <summary>
    /// Matches dates in the format YYYY-MM-DD.
    /// </summary>
    [GeneratedRegex(@"(?<!/)\b\d{4}-\d{2}-\d{2}\b(?!/)")]
    private static partial Regex Date();

    /// <summary>
    /// Matches UUIDs (e.g., 123e4567-e89b-12d3-a456-426614174000).
    /// </summary>
    [GeneratedRegex(
        @"^(?:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89ABab][0-9a-fA-F]{3}-[0-9a-fA-F]{12})$",
        RegexOptions.IgnoreCase)]
    private static partial Regex Uuid();

    /// <summary>
    /// Matches data size strings such as "10 MB", "1.5 GiB", "100 Bytes", etc.
    /// </summary>
    [GeneratedRegex(@"\b\d+(\.\d+)?\s*(B|Bytes?|KB|KiB|MB|MiB|GB|GiB|TB|TiB|PB|PiB|EB|EiB|ZB|ZiB|YB|YiB)\b", RegexOptions.IgnoreCase)]
    private static partial Regex DataSize();
}
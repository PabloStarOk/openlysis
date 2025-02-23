using System.Text.Json;

using Filescan.Client.Abstractions;
using Filescan.Client.Models.Common;

namespace Filescan.Client.Services.Parsers;

/// <summary>
/// Creates <see cref="FilescanError"/> objects from <see cref="JsonElement"/>.
/// </summary>
public class FilescanErrorParser : ModelParser<FilescanError, JsonElement>
{
    /// <inheritdoc/>
    public override FilescanError Parse(JsonElement rootElement)
    {
        if (!rootElement.TryGetProperty("detail", out JsonElement detailElement))
        {
            return FilescanError.Empty;
        }

        string detail = string.Empty;
        bool isValidationError = false;
        ValidationError[] validationErrors = [];
        if (detailElement.ValueKind is not JsonValueKind.String)
        {
            isValidationError = true;
            validationErrors = ParseValidationErrors(detailElement);
        }
        else
        {
            detail = detailElement.GetString() ?? string.Empty;
        }

        return new FilescanError(isValidationError, detail, validationErrors);
    }

    /// <summary>
    /// Parses validation errors from a JSON element.
    /// </summary>
    /// <param name="element">The JSON element containing validation errors.</param>
    /// <returns>An array of <see cref="ValidationError"/> objects.</returns>
    private static ValidationError[] ParseValidationErrors(JsonElement element)
    {
        if (element.ValueKind is not JsonValueKind.Array)
        {
            return [];
        }

        using var enumerator = element.EnumerateArray();
        var validationErrors = new List<ValidationError>(element.GetArrayLength());
        while (enumerator.MoveNext())
        {
            JsonElement errorElement = enumerator.Current;
            string type = GetStringOrEmpty("type", element);
            string message = GetStringOrEmpty("msg", element);
            string[] location = errorElement.GetProperty("loc")
                .EnumerateArray()
                .Select(locElem => locElem.GetString() ?? string.Empty)
                .ToArray();

            var error = new ValidationError(type, message, location);
            validationErrors.Add(error);
        }

        return validationErrors.ToArray();
    }
}
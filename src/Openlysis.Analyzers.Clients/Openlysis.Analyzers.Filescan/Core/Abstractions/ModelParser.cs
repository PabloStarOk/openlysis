using System.Text.Json;

namespace Openlysis.Analyzers.Filescan.Core.Abstractions;

/// <summary>
/// Interface for parsing JSON elements to models.
/// </summary>
/// <typeparam name="TModel">The type of the model to parse to.</typeparam>
/// <typeparam name="TJsonElement">The type of the JSON element to parse from.</typeparam>
public abstract class ModelParser<TModel, TJsonElement>
    where TModel : notnull
    where TJsonElement : notnull
{
    /// <summary>
    /// Parses the given JSON element to an instance of <typeparamref name="TModel"/>.
    /// </summary>
    /// <param name="rootElement">The root JSON element to parse.</param>
    /// <returns>An instance of <typeparamref name="TModel"/>.</returns>
    public abstract TModel Parse(TJsonElement rootElement);

    /// <summary>
    /// Retrieves the value of a specified property as a string, or an empty string if the property does not exist.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="element">The JSON element containing the property.</param>
    /// <returns>The string value of the property, or an empty string if the property does not exist.</returns>
    protected static string GetStringOrEmpty(string propertyName, JsonElement element)
    {
        string value = string.Empty;

        if (element.TryGetProperty(propertyName, out JsonElement propertyElement))
        {
            value = propertyElement.GetString() ?? string.Empty;
        }

        return value;
    }

    /// <summary>
    /// Retrieves the value of a specified property as a boolean, or false if the property does not exist or is null.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="element">The JSON element containing the property.</param>
    /// <returns>The boolean value of the property, or false if the property does not exist or is null.</returns>
    protected static bool GetBoolOrFalse(string propertyName, JsonElement element)
    {
        if (element.TryGetProperty(propertyName, out JsonElement boolElement)
            && boolElement.ValueKind is not JsonValueKind.Null)
        {
            return boolElement.GetBoolean();
        }

        return false;
    }

    /// <summary>
    /// Retrieves the value of a specified property as a nullable boolean.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="element">The JSON element containing the property.</param>
    /// <returns>The boolean value of the property, or null if the property does not exist or is not a boolean.</returns>
    protected static bool? GetBoolOrNull(string propertyName, JsonElement element)
    {
        if (element.TryGetProperty(propertyName, out JsonElement propertyElement) &&
            propertyElement is { ValueKind: JsonValueKind.False or JsonValueKind.True })
        {
            return propertyElement.GetBoolean();
        }

        return null;
    }

    /// <summary>
    /// Retrieves the value of a specified property as an integer.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="element">The JSON element containing the property.</param>
    /// <returns>The integer value of the property, or -1 if the property does not exist.</returns>
    protected static int GetInt32OrNegative(string propertyName, JsonElement element)
    {
        if (element.TryGetProperty(propertyName, out JsonElement propertyElement)
            && propertyElement.TryGetInt32(out int result))
        {
            return result;
        }

        return -1;
    }

    /// <summary>
    /// Converts a JSON element to an array of strings.
    /// </summary>
    /// <param name="element">The JSON element to convert.</param>
    /// <returns>An array of strings extracted from the JSON element.</returns>
    protected static string[] GetStringArray(JsonElement element)
    {
        if (element.ValueKind is not JsonValueKind.Array)
        {
            return [];
        }

        var strings = new List<string>(element.GetArrayLength());
        using IEnumerator<JsonElement> enumerator = element.EnumerateArray();
        while (enumerator.MoveNext())
        {
            strings.Add(enumerator.Current.GetString() ?? string.Empty);
        }

        return strings.ToArray();
    }
}
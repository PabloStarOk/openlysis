using NJsonSchema.Generation;

namespace Openlysis.API.Documentation;

/// <summary>
/// Generates schema names by removing the "Dto" suffix from type names.
/// </summary>
internal sealed class SchemaNameGenerator : ISchemaNameGenerator
{
    private const string DtoSuffix = "Dto";

    /// <inheritdoc/>
    public string Generate(Type type)
    {
        return type.Name.Replace(DtoSuffix, string.Empty);
    }
}
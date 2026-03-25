using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arcadia.FormBuilder.Schema;

/// <summary>
/// Deserializes a JSON string into a <see cref="FormSchema"/>.
/// Supports both Arcadia Controls native format and a subset of JSON Schema draft-07.
/// </summary>
public static class SchemaParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly JsonSerializerOptions _indentedOptions = new(Options)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions _compactOptions = new(Options)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Parses a JSON string into a FormSchema.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The deserialized FormSchema.</returns>
    public static FormSchema Parse(string json)
    {
        return JsonSerializer.Deserialize<FormSchema>(json, Options)
            ?? throw new JsonException("Failed to parse FormSchema from JSON.");
    }

    /// <summary>
    /// Serializes a FormSchema to a JSON string.
    /// </summary>
    /// <param name="schema">The schema to serialize.</param>
    /// <param name="indented">Whether to format with indentation.</param>
    public static string ToJson(FormSchema schema, bool indented = true)
    {
        var options = indented ? _indentedOptions : _compactOptions;

        return JsonSerializer.Serialize(schema, options);
    }

    /// <summary>
    /// Attempts to parse a JSON string, returning success/failure instead of throwing.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="schema">The parsed schema, or null on failure.</param>
    /// <returns>True if parsing succeeded.</returns>
    public static bool TryParse(string json, out FormSchema? schema)
    {
        try
        {
            schema = Parse(json);
            return true;
        }
        catch
        {
            schema = null;
            return false;
        }
    }
}

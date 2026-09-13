using System.Text;
using System.Text.Json;
using DevWindowUtility.Models;

namespace DevWindowUtility.Services;

public sealed class JsonResult
{
    public required bool Success { get; init; }
    public string Text { get; init; } = "";
    public string Message { get; init; } = "";
    public IReadOnlyList<JsonNodeItem> Tree { get; init; } = [];
}

public sealed class JsonService
{
    private static readonly JsonDocumentOptions ParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public JsonResult Format(string text) => Transform(text, indented: true);

    public JsonResult Minify(string text) => Transform(text, indented: false);

    public JsonResult Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new JsonResult { Success = false, Message = "Paste or open JSON first." };
        }

        try
        {
            using var document = JsonDocument.Parse(text, ParseOptions);
            var tree = new[] { BuildNode("root", document.RootElement) };
            return new JsonResult
            {
                Success = true,
                Text = text,
                Message = "JSON is valid.",
                Tree = tree
            };
        }
        catch (JsonException ex)
        {
            return new JsonResult { Success = false, Message = FormatError(ex) };
        }
    }

    private static JsonResult Transform(string text, bool indented)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new JsonResult { Success = false, Message = "Paste or open JSON first." };
        }

        try
        {
            using var document = JsonDocument.Parse(text, ParseOptions);
            var output = JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = indented
            });
            return new JsonResult
            {
                Success = true,
                Text = output,
                Message = indented ? "Formatted JSON." : "Minified JSON.",
                Tree = [BuildNode("root", document.RootElement)]
            };
        }
        catch (JsonException ex)
        {
            return new JsonResult { Success = false, Message = FormatError(ex) };
        }
    }

    private static JsonNodeItem BuildNode(string name, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var properties = element.EnumerateObject()
                    .Select(property => BuildNode(property.Name, property.Value))
                    .ToList();
                return new JsonNodeItem(name, $"{{ {properties.Count} }}", "Object", properties);
            case JsonValueKind.Array:
                var items = element.EnumerateArray()
                    .Select((child, index) => BuildNode($"[{index}]", child))
                    .ToList();
                return new JsonNodeItem(name, $"[ {items.Count} ]", "Array", items);
            case JsonValueKind.String:
                return new JsonNodeItem(name, Truncate(element.GetString() ?? ""), "String", []);
            case JsonValueKind.Number:
                return new JsonNodeItem(name, element.GetRawText(), "Number", []);
            case JsonValueKind.True:
            case JsonValueKind.False:
                return new JsonNodeItem(name, element.GetRawText(), "Boolean", []);
            case JsonValueKind.Null:
                return new JsonNodeItem(name, "null", "Null", []);
            default:
                return new JsonNodeItem(name, element.GetRawText(), element.ValueKind.ToString(), []);
        }
    }

    private static string Truncate(string value) =>
        value.Length <= 80 ? value : value[..77] + "...";

    private static string FormatError(JsonException ex)
    {
        var location = new StringBuilder("Invalid JSON");
        if (ex.LineNumber is { } line)
        {
            location.Append($" at line {line + 1}");
        }

        if (ex.BytePositionInLine is { } column)
        {
            location.Append($", column {column + 1}");
        }

        return $"{location}: {ex.Message}";
    }
}

namespace DevWindowUtility.Models;

public sealed class JsonNodeItem
{
    public JsonNodeItem(string name, string preview, string kind, IReadOnlyList<JsonNodeItem> children)
    {
        Name = name;
        Preview = preview;
        Kind = kind;
        Children = children;
        IsExpanded = kind is "Object" or "Array" && children.Count is > 0 and <= 30;
    }

    public string Name { get; }
    public string Preview { get; }
    public string Kind { get; }
    public IReadOnlyList<JsonNodeItem> Children { get; }
    public bool IsExpanded { get; set; }
}

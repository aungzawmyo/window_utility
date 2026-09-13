namespace DevWindowUtility.Models;

public sealed record FolderShortcut(string Name, string FullPath);

public sealed record FolderGroup(string Title, IReadOnlyList<FolderShortcut> Items);

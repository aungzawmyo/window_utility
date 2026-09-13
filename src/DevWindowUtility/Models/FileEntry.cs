namespace DevWindowUtility.Models;

public sealed record FileEntry(
    string Name,
    string FullPath,
    bool IsDirectory,
    string Kind,
    string SizeText,
    string ModifiedText);

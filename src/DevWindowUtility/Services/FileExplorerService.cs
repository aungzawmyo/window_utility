using System.Diagnostics;
using System.IO;
using DevWindowUtility.Models;

namespace DevWindowUtility.Services;

public sealed class FileExplorerService
{
    public string Expand(string path)
    {
        var trimmed = path.Trim();
        if (trimmed is "~" or "~/")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        if (trimmed.StartsWith("~/", StringComparison.Ordinal) || trimmed.StartsWith("~\\", StringComparison.Ordinal))
        {
            trimmed = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                trimmed[2..]);
        }

        return Environment.ExpandEnvironmentVariables(trimmed);
    }

    public IReadOnlyList<FileEntry> List(string path)
    {
        var directory = new DirectoryInfo(path);
        if (!directory.Exists)
        {
            throw new DirectoryNotFoundException($"Folder not found: {path}");
        }

        var entries = new List<FileEntry>();

        foreach (var folder in SafeEnumerate(() => directory.EnumerateDirectories()))
        {
            entries.Add(ToEntry(folder.Name, folder.FullName, true, folder.LastWriteTime, null));
        }

        foreach (var file in SafeEnumerate(() => directory.EnumerateFiles()))
        {
            entries.Add(ToEntry(file.Name, file.FullName, false, file.LastWriteTime, file.Length));
        }

        return entries
            .OrderByDescending(e => e.IsDirectory)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public string? GetParent(string path)
    {
        try
        {
            return Directory.GetParent(path)?.FullName;
        }
        catch
        {
            return null;
        }
    }

    public void OpenExternal(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private static IEnumerable<T> SafeEnumerate<T>(Func<IEnumerable<T>> source)
    {
        try
        {
            return source().ToList();
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
    }

    private static FileEntry ToEntry(string name, string fullPath, bool isDirectory, DateTime modified, long? size) =>
        new(
            name,
            fullPath,
            isDirectory,
            isDirectory ? "Folder" : (Path.GetExtension(name).TrimStart('.') is { Length: > 0 } ext ? ext.ToLowerInvariant() : "file"),
            isDirectory ? "" : FormatSize(size ?? 0),
            modified.ToString("yyyy-MM-dd HH:mm"));

    private static string FormatSize(long bytes) =>
        bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024d:0.#} KB",
            < 1024L * 1024 * 1024 => $"{bytes / 1024d / 1024d:0.#} MB",
            _ => $"{bytes / 1024d / 1024d / 1024d:0.#} GB"
        };
}

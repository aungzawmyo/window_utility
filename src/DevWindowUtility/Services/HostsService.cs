using System.IO;
using System.Text;
using DevWindowUtility.Models;

namespace DevWindowUtility.Services;

public sealed class HostsService
{
    private readonly string _path;
    private bool _backedUp;

    public HostsService()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts"))
    {
    }

    public HostsService(string path)
    {
        _path = path;
    }

    public string FilePath => _path;

    public IReadOnlyList<HostsEntry> Load()
    {
        if (!File.Exists(_path))
        {
            return [];
        }

        return File.ReadAllLines(_path, Encoding.Default)
            .Select(ParseLine)
            .ToList();
    }

    public void Save(IEnumerable<HostsEntry> entries)
    {
        BackupOnce();

        var text = string.Join(Environment.NewLine, entries.Select(ToLine));
        if (!text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            text += Environment.NewLine;
        }

        var temp = _path + ".tmp";
        File.WriteAllText(temp, text, Encoding.Default);
        File.Copy(temp, _path, overwrite: true);
        File.Delete(temp);
    }

    public HostsEntry Create(string address, string hosts) =>
        new()
        {
            Enabled = true,
            Address = address.Trim(),
            Hosts = hosts.Trim(),
            Comment = "",
            IsCommentOnly = false,
            Raw = ""
        };

    private void BackupOnce()
    {
        if (_backedUp || !File.Exists(_path))
        {
            return;
        }

        File.Copy(_path, _path + ".bak", overwrite: true);
        _backedUp = true;
    }

    private static HostsEntry ParseLine(string line)
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return new HostsEntry { Raw = line, IsCommentOnly = true };
        }

        var enabled = !trimmed.StartsWith('#');
        var working = enabled ? trimmed : trimmed.TrimStart('#').TrimStart();

        var comment = "";
        var hash = working.IndexOf('#');
        if (hash >= 0)
        {
            comment = working[(hash + 1)..].Trim();
            working = working[..hash].Trim();
        }

        var parts = working.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return new HostsEntry
            {
                Raw = line,
                IsCommentOnly = true,
                Comment = trimmed.TrimStart('#').Trim()
            };
        }

        return new HostsEntry
        {
            Raw = line,
            IsCommentOnly = false,
            Enabled = enabled,
            Address = parts[0],
            Hosts = string.Join(" ", parts.Skip(1)),
            Comment = comment
        };
    }

    private static string ToLine(HostsEntry entry)
    {
        if (entry.IsCommentOnly)
        {
            return entry.Raw;
        }

        var line = $"{entry.Address}\t{entry.Hosts}";
        if (!string.IsNullOrWhiteSpace(entry.Comment))
        {
            line += $"\t# {entry.Comment}";
        }

        return entry.Enabled ? line : "# " + line;
    }
}

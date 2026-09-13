using System.IO;
using System.Text.Json;

namespace DevWindowUtility.Services;

public sealed class FavoriteStore
{
    private readonly string _path;

    public FavoriteStore()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(root, "DevHelper");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "favorites.json");

        var legacy = Path.Combine(root, "DevWindowUtility", "favorites.json");
        if (!File.Exists(_path) && File.Exists(legacy))
        {
            File.Copy(legacy, _path);
        }
    }

    public IReadOnlyList<string> Load()
    {
        if (!File.Exists(_path))
        {
            var seeded = Seed().Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            Save(seeded);
            return seeded;
        }

        try
        {
            var json = File.ReadAllText(_path);
            var paths = JsonSerializer.Deserialize<List<string>>(json) ?? [];
            return paths
                .Where(path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<string> paths)
    {
        var unique = paths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var json = JsonSerializer.Serialize(unique, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }

    private static IEnumerable<string> Seed()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        yield return Path.Combine(home, "Downloads");
        yield return home;

        var workspace = @"D:\WS_NEW";
        if (Directory.Exists(workspace))
        {
            yield return workspace;
        }
    }
}

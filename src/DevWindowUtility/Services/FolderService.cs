using System.Diagnostics;
using System.IO;
using DevWindowUtility.Models;

namespace DevWindowUtility.Services;

public sealed class FolderService
{
    public IReadOnlyList<FolderGroup> GetGroups()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var groups = new[]
        {
            new FolderGroup("User", Existing(
                ("User profile", home),
                ("Desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
                ("Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                ("Downloads", Path.Combine(home, "Downloads")))),
            new FolderGroup("AppData", Existing(
                ("Roaming", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
                ("Local", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),
                ("LocalLow", Path.Combine(home, "AppData", "LocalLow")))),
            new FolderGroup("System", Existing(
                ("Temp", Path.GetTempPath()),
                ("Windows", Environment.GetFolderPath(Environment.SpecialFolder.Windows)),
                ("Program Files", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)),
                ("Program Files (x86)", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)))),
            new FolderGroup("Dev", Existing(
                (".ssh", Path.Combine(home, ".ssh")),
                (".cursor", Path.Combine(home, ".cursor"))))
        };

        return groups.Where(g => g.Items.Count > 0).ToList();
    }

    public void Open(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = path,
            UseShellExecute = true
        });
    }

    private static IReadOnlyList<FolderShortcut> Existing(params (string Name, string FullPath)[] items) =>
        items
            .Where(item => !string.IsNullOrWhiteSpace(item.FullPath) && Directory.Exists(item.FullPath))
            .Select(item => new FolderShortcut(item.Name, item.FullPath))
            .ToList();
}

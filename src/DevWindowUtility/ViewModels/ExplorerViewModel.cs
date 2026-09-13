using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevWindowUtility.Models;
using DevWindowUtility.Services;

namespace DevWindowUtility.ViewModels;

public partial class ExplorerViewModel : ObservableObject
{
    private readonly FileExplorerService _explorer;
    private readonly FavoriteStore _favoritesStore;
    private readonly IFileDialogService _dialogs;
    private readonly IDialogService _alerts;
    private IReadOnlyList<FileEntry> _all = [];

    public ExplorerViewModel(
        FileExplorerService explorer,
        FavoriteStore favoritesStore,
        IFileDialogService dialogs,
        IDialogService alerts)
    {
        _explorer = explorer;
        _favoritesStore = favoritesStore;
        _dialogs = dialogs;
        _alerts = alerts;

        foreach (var path in _favoritesStore.Load())
        {
            Favorites.Add(ToFavorite(path));
        }

        CurrentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Navigate(CurrentPath);
    }

    public event Action<string>? OpenInNotepadRequested;
    public event Action<string>? OpenInJsonRequested;

    [ObservableProperty]
    private string _currentPath = "";

    [ObservableProperty]
    private string _filter = "";

    [ObservableProperty]
    private string _status = "";

    [ObservableProperty]
    private FileEntry? _selectedEntry;

    [ObservableProperty]
    private FolderShortcut? _selectedFavorite;

    public ObservableCollection<FileEntry> Entries { get; } = [];
    public ObservableCollection<FolderShortcut> Favorites { get; } = [];

    partial void OnFilterChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void Go() => Navigate(CurrentPath);

    [RelayCommand]
    private void Up()
    {
        var parent = _explorer.GetParent(CurrentPath);
        if (parent is not null)
        {
            Navigate(parent);
        }
    }

    [RelayCommand]
    private void Refresh() => Navigate(CurrentPath);

    [RelayCommand]
    private void OpenSelected()
    {
        if (SelectedEntry is null)
        {
            return;
        }

        if (SelectedEntry.IsDirectory)
        {
            Navigate(SelectedEntry.FullPath);
            return;
        }

        try
        {
            _explorer.OpenExternal(SelectedEntry.FullPath);
            Status = $"Opened {SelectedEntry.Name}";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    [RelayCommand]
    private void OpenInExplorer()
    {
        var path = SelectedEntry?.FullPath ?? CurrentPath;
        try
        {
            _explorer.OpenExternal(path);
            Status = $"Opened in Explorer: {path}";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    [RelayCommand]
    private void OpenInNotepad()
    {
        if (SelectedEntry is null || SelectedEntry.IsDirectory)
        {
            Status = "Select a file to open in Notepad.";
            return;
        }

        OpenInNotepadRequested?.Invoke(SelectedEntry.FullPath);
    }

    [RelayCommand]
    private void OpenInJson()
    {
        if (SelectedEntry is null || SelectedEntry.IsDirectory)
        {
            Status = "Select a JSON file first.";
            return;
        }

        OpenInJsonRequested?.Invoke(SelectedEntry.FullPath);
    }

    [RelayCommand]
    private void CopyPath()
    {
        var path = SelectedEntry?.FullPath ?? CurrentPath;
        System.Windows.Clipboard.SetText(path);
        Status = $"Copied {path}";
    }

    [RelayCommand]
    private void OpenFavorite()
    {
        if (SelectedFavorite is not null)
        {
            Navigate(SelectedFavorite.FullPath);
        }
    }

    [RelayCommand]
    private void PinCurrent() => AddFavoritePath(CurrentPath);

    [RelayCommand]
    private void AddFavorite()
    {
        var path = _dialogs.PickFolder();
        if (path is not null)
        {
            AddFavoritePath(path);
        }
    }

    [RelayCommand]
    private void RemoveFavorite()
    {
        if (SelectedFavorite is null)
        {
            return;
        }

        Favorites.Remove(SelectedFavorite);
        PersistFavorites();
        Status = $"Removed favorite {SelectedFavorite.Name}.";
        SelectedFavorite = null;
    }

    private void Navigate(string path)
    {
        try
        {
            var expanded = _explorer.Expand(path);
            _all = _explorer.List(expanded);
            CurrentPath = Path.GetFullPath(expanded);
            ApplyFilter();
            Status = $"{Entries.Count} item(s) in {CurrentPath}";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
            _alerts.Error(ex.Message, "Explorer");
        }
    }

    private void ApplyFilter()
    {
        Entries.Clear();
        var query = string.IsNullOrWhiteSpace(Filter)
            ? _all
            : _all.Where(e => e.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase));

        foreach (var entry in query)
        {
            Entries.Add(entry);
        }
    }

    private void AddFavoritePath(string path)
    {
        try
        {
            var full = Path.GetFullPath(_explorer.Expand(path));
            if (!Directory.Exists(full))
            {
                Status = "That folder does not exist.";
                return;
            }

            if (Favorites.Any(f => f.FullPath.Equals(full, StringComparison.OrdinalIgnoreCase)))
            {
                Status = "Already in favorites.";
                return;
            }

            var favorite = ToFavorite(full);
            Favorites.Add(favorite);
            PersistFavorites();
            SelectedFavorite = favorite;
            Status = $"Pinned {favorite.Name}.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    private void PersistFavorites() =>
        _favoritesStore.Save(Favorites.Select(f => f.FullPath));

    private static FolderShortcut ToFavorite(string path) =>
        new(Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            is { Length: > 0 } name
            ? name
            : path, path);
}

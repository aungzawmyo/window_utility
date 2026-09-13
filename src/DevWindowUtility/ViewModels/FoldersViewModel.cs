using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevWindowUtility.Models;
using DevWindowUtility.Services;

namespace DevWindowUtility.ViewModels;

public partial class FoldersViewModel : ObservableObject
{
    private readonly FolderService _folders;

    public FoldersViewModel(FolderService folders)
    {
        _folders = folders;
        Groups = _folders.GetGroups();
        Status = Groups.Count == 0
            ? "No common folders were found."
            : "Click a folder to open it in Explorer.";
    }

    public IReadOnlyList<FolderGroup> Groups { get; }

    [ObservableProperty]
    private string _status = "";

    [RelayCommand]
    private void Open(FolderShortcut? shortcut)
    {
        if (shortcut is null)
        {
            return;
        }

        try
        {
            _folders.Open(shortcut.FullPath);
            Status = $"Opened {shortcut.FullPath}";
        }
        catch (Exception ex)
        {
            Status = $"Could not open {shortcut.FullPath}: {ex.Message}";
        }
    }
}

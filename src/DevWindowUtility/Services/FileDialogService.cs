using Microsoft.Win32;

namespace DevWindowUtility.Services;

public sealed class FileDialogService : IFileDialogService
{
    public string? OpenFile(string filter)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            CheckFileExists = true
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveFile(string filter, string? fileName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = fileName ?? ""
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? PickFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select a folder",
            Multiselect = false
        };
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }
}

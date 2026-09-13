using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevWindowUtility.Services;

namespace DevWindowUtility.ViewModels;

public partial class NotepadViewModel : ObservableObject
{
    public const string FileFilter =
        "Text files (*.txt;*.md;*.log;*.json;*.xml;*.cs;*.js;*.ts;*.css;*.html)|*.txt;*.md;*.log;*.json;*.xml;*.cs;*.js;*.ts;*.css;*.html|All files (*.*)|*.*";

    private readonly TextFileService _files;
    private readonly IFileDialogService _dialogs;
    private readonly IDialogService _alerts;
    private bool _loading;

    public NotepadViewModel(TextFileService files, IFileDialogService dialogs, IDialogService alerts)
    {
        _files = files;
        _dialogs = dialogs;
        _alerts = alerts;
        UpdateTitle();
    }

    [ObservableProperty]
    private string _content = "";

    [ObservableProperty]
    private string _filePath = "";

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private bool _wrapText = true;

    [ObservableProperty]
    private string _title = "Untitled";

    [ObservableProperty]
    private string _status = "New note.";

    partial void OnContentChanged(string value)
    {
        if (_loading)
        {
            return;
        }

        IsDirty = true;
        UpdateTitle();
        Status = $"{value.Length} character(s).";
    }

    public void LoadFromPath(string path)
    {
        if (!ConfirmDiscard())
        {
            return;
        }

        try
        {
            _loading = true;
            Content = _files.Read(path);
            FilePath = path;
            IsDirty = false;
            UpdateTitle();
            Status = $"Opened {path}";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
            _alerts.Error(ex.Message, "Open failed");
        }
        finally
        {
            _loading = false;
        }
    }

    [RelayCommand]
    private void New()
    {
        if (!ConfirmDiscard())
        {
            return;
        }

        _loading = true;
        Content = "";
        FilePath = "";
        IsDirty = false;
        _loading = false;
        UpdateTitle();
        Status = "New note.";
    }

    [RelayCommand]
    private void Open()
    {
        var path = _dialogs.OpenFile(FileFilter);
        if (path is not null)
        {
            LoadFromPath(path);
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            SaveAs();
            return;
        }

        SaveTo(FilePath);
    }

    [RelayCommand]
    private void SaveAs()
    {
        var suggested = string.IsNullOrWhiteSpace(FilePath) ? "note.txt" : Path.GetFileName(FilePath);
        var path = _dialogs.SaveFile(FileFilter, suggested);
        if (path is not null)
        {
            SaveTo(path);
        }
    }

    private void SaveTo(string path)
    {
        try
        {
            _files.Write(path, Content);
            FilePath = path;
            IsDirty = false;
            UpdateTitle();
            Status = $"Saved {path}";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
            _alerts.Error(ex.Message, "Save failed");
        }
    }

    private bool ConfirmDiscard() =>
        !IsDirty || _alerts.Confirm("Discard unsaved changes?", "Unsaved note");

    private void UpdateTitle()
    {
        var name = string.IsNullOrWhiteSpace(FilePath) ? "Untitled" : Path.GetFileName(FilePath);
        Title = IsDirty ? $"{name} *" : name;
    }
}

using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevWindowUtility.Models;
using DevWindowUtility.Services;

namespace DevWindowUtility.ViewModels;

public partial class JsonViewModel : ObservableObject
{
    public const string FileFilter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

    private readonly JsonService _json;
    private readonly TextFileService _files;
    private readonly IFileDialogService _dialogs;
    private readonly IDialogService _alerts;

    public JsonViewModel(JsonService json, TextFileService files, IFileDialogService dialogs, IDialogService alerts)
    {
        _json = json;
        _files = files;
        _dialogs = dialogs;
        _alerts = alerts;
    }

    [ObservableProperty]
    private string _text = "";

    [ObservableProperty]
    private string _filePath = "";

    [ObservableProperty]
    private string _status = "Paste JSON, then format or validate.";

    public ObservableCollection<JsonNodeItem> Tree { get; } = [];

    public void LoadFromPath(string path)
    {
        try
        {
            Text = _files.Read(path);
            FilePath = path;
            Apply(_json.Validate(Text), replaceText: false);
        }
        catch (Exception ex)
        {
            Status = ex.Message;
            _alerts.Error(ex.Message, "Open JSON failed");
        }
    }

    [RelayCommand]
    private void Format() => Apply(_json.Format(Text), replaceText: true);

    [RelayCommand]
    private void Minify() => Apply(_json.Minify(Text), replaceText: true);

    [RelayCommand]
    private void Validate() => Apply(_json.Validate(Text), replaceText: false);

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
        var path = string.IsNullOrWhiteSpace(FilePath)
            ? _dialogs.SaveFile(FileFilter, "data.json")
            : FilePath;
        if (path is null)
        {
            return;
        }

        SaveTo(path);
    }

    [RelayCommand]
    private void SaveAs()
    {
        var suggested = string.IsNullOrWhiteSpace(FilePath) ? "data.json" : Path.GetFileName(FilePath);
        var path = _dialogs.SaveFile(FileFilter, suggested);
        if (path is not null)
        {
            SaveTo(path);
        }
    }

    [RelayCommand]
    private void Copy()
    {
        if (string.IsNullOrEmpty(Text))
        {
            Status = "Nothing to copy.";
            return;
        }

        Clipboard.SetText(Text);
        Status = "Copied JSON to the clipboard.";
    }

    [RelayCommand]
    private void Clear()
    {
        Text = "";
        FilePath = "";
        Tree.Clear();
        Status = "Cleared.";
    }

    private void SaveTo(string path)
    {
        try
        {
            _files.Write(path, Text);
            FilePath = path;
            Status = $"Saved {path}";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
            _alerts.Error(ex.Message, "Save JSON failed");
        }
    }

    private void Apply(JsonResult result, bool replaceText)
    {
        Status = result.Message;
        Tree.Clear();
        if (!result.Success)
        {
            return;
        }

        if (replaceText)
        {
            Text = result.Text;
        }

        foreach (var node in result.Tree)
        {
            Tree.Add(node);
        }
    }
}

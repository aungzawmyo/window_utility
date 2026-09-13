using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevWindowUtility.Models;
using DevWindowUtility.Services;

namespace DevWindowUtility.ViewModels;

public partial class HostsViewModel : ObservableObject
{
    private readonly HostsService _hosts;
    private readonly ElevationService _elevation;
    private readonly IDialogService _dialogs;
    private List<HostsEntry> _all = [];

    public HostsViewModel(HostsService hosts, ElevationService elevation, IDialogService dialogs)
    {
        _hosts = hosts;
        _elevation = elevation;
        _dialogs = dialogs;
        IsAdministrator = _elevation.IsAdministrator();
        AdminStatus = IsAdministrator
            ? "Running as administrator. Hosts file can be saved."
            : "Not elevated. Saving hosts requires administrator rights.";
        Reload();
    }

    [ObservableProperty]
    private bool _isAdministrator;

    [ObservableProperty]
    private string _adminStatus = "";

    [ObservableProperty]
    private string _status = "";

    [ObservableProperty]
    private string _newAddress = "127.0.0.1";

    [ObservableProperty]
    private string _newHost = "";

    [ObservableProperty]
    private HostsEntry? _selectedEntry;

    public string HostsPath => _hosts.FilePath;

    public ObservableCollection<HostsEntry> Entries { get; } = [];

    [RelayCommand]
    private void Reload()
    {
        try
        {
            _all = _hosts.Load().ToList();
            ShowEditable();
            Status = $"Loaded {Entries.Count} host entr(ies) from {HostsPath}.";
        }
        catch (Exception ex)
        {
            Status = $"Failed to read hosts file: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanToggle))]
    private void Toggle()
    {
        if (SelectedEntry is null || SelectedEntry.IsCommentOnly)
        {
            return;
        }

        SelectedEntry.Enabled = !SelectedEntry.Enabled;
        Status = SelectedEntry.Enabled
            ? $"Enabled {SelectedEntry.Hosts}."
            : $"Disabled {SelectedEntry.Hosts}.";
    }

    [RelayCommand]
    private void Add()
    {
        if (string.IsNullOrWhiteSpace(NewAddress) || string.IsNullOrWhiteSpace(NewHost))
        {
            Status = "Address and hostname are required.";
            return;
        }

        var entry = _hosts.Create(NewAddress, NewHost);
        _all.Add(entry);
        Entries.Add(entry);
        SelectedEntry = entry;
        NewHost = "";
        Status = $"Added {entry.Address} {entry.Hosts}. Save to write the file.";
    }

    [RelayCommand(CanExecute = nameof(CanToggle))]
    private void Delete()
    {
        if (SelectedEntry is null)
        {
            return;
        }

        if (!_dialogs.Confirm($"Remove {SelectedEntry.Address} {SelectedEntry.Hosts} from the list? Save to write the file.", "Confirm delete"))
        {
            return;
        }

        _all.Remove(SelectedEntry);
        Entries.Remove(SelectedEntry);
        SelectedEntry = null;
        Status = "Entry removed from the list. Save to write the file.";
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            _hosts.Save(_all);
            Status = $"Saved hosts file. Backup: {HostsPath}.bak";
        }
        catch (UnauthorizedAccessException)
        {
            Status = "Access denied writing hosts. Restart as administrator.";
            _dialogs.Error("Windows blocked writing the hosts file. Restart as administrator and save again.", "Save failed");
        }
        catch (Exception ex)
        {
            Status = $"Failed to save hosts: {ex.Message}";
            _dialogs.Error(ex.Message, "Save failed");
        }
    }

    [RelayCommand]
    private void RestartAsAdministrator()
    {
        try
        {
            _elevation.RestartAsAdministrator();
        }
        catch (Exception ex)
        {
            _dialogs.Error(ex.Message, "Elevation failed");
        }
    }

    partial void OnSelectedEntryChanged(HostsEntry? value)
    {
        ToggleCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }

    private bool CanToggle() => SelectedEntry is { IsCommentOnly: false };

    private void ShowEditable()
    {
        Entries.Clear();
        foreach (var entry in _all.Where(e => !e.IsCommentOnly))
        {
            Entries.Add(entry);
        }
    }
}

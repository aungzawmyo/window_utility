using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevWindowUtility.Models;
using DevWindowUtility.Services;

namespace DevWindowUtility.ViewModels;

public partial class ProcessesViewModel : ObservableObject
{
    private readonly ProcessService _processes;
    private readonly IDialogService _dialogs;
    private IReadOnlyList<ProcessItem> _all = [];

    public ProcessesViewModel(ProcessService processes, IDialogService dialogs)
    {
        _processes = processes;
        _dialogs = dialogs;
        Refresh();
    }

    [ObservableProperty]
    private string _search = "";

    [ObservableProperty]
    private string _status = "Search by name or PID, then kill with confirmation.";

    [ObservableProperty]
    private ProcessItem? _selectedProcess;

    public ObservableCollection<ProcessItem> Processes { get; } = [];

    partial void OnSearchChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void Refresh()
    {
        try
        {
            _all = _processes.GetProcesses();
            ApplyFilter();
            Status = $"Showing {Processes.Count} of {_all.Count} process(es).";
        }
        catch (Exception ex)
        {
            Status = $"Failed to list processes: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanKillSelected))]
    private void KillSelected()
    {
        if (SelectedProcess is null)
        {
            return;
        }

        KillItems([SelectedProcess], $"{SelectedProcess.Name} (PID {SelectedProcess.Pid})");
    }

    [RelayCommand]
    private void KillByName()
    {
        var name = Search.Trim();
        if (string.IsNullOrWhiteSpace(name) && SelectedProcess is not null)
        {
            name = SelectedProcess.Name;
        }

        if (string.IsNullOrWhiteSpace(name) || int.TryParse(name, out _))
        {
            Status = "Enter a process name (not a PID) to kill all matching processes.";
            return;
        }

        var matches = _processes.FindByName(name);
        KillItems(matches, $"all processes named '{name}'");
    }

    partial void OnSelectedProcessChanged(ProcessItem? value) => KillSelectedCommand.NotifyCanExecuteChanged();

    private bool CanKillSelected() => SelectedProcess is not null;

    private void KillItems(IReadOnlyList<ProcessItem> items, string reason)
    {
        if (items.Count == 0)
        {
            Status = $"No running process matches {reason}.";
            return;
        }

        var summary = string.Join(Environment.NewLine, items.Select(p => $"{p.Name} (PID {p.Pid})"));
        if (!_dialogs.Confirm($"Kill {reason}?\n\n{summary}", "Confirm kill"))
        {
            return;
        }

        foreach (var item in items)
        {
            var result = _processes.KillByPid(item.Pid);
            Status = result.Message;
            if (!result.Success)
            {
                _dialogs.Error(result.Message, "Kill failed");
            }
        }

        Refresh();
    }

    private void ApplyFilter()
    {
        Processes.Clear();
        var query = string.IsNullOrWhiteSpace(Search)
            ? _all
            : _all.Where(MatchesSearch);

        foreach (var process in query)
        {
            Processes.Add(process);
        }

        Status = $"Showing {Processes.Count} of {_all.Count} process(es).";
    }

    private bool MatchesSearch(ProcessItem process) =>
        process.Name.Contains(Search, StringComparison.OrdinalIgnoreCase)
        || process.Pid.ToString().Contains(Search, StringComparison.OrdinalIgnoreCase)
        || process.WindowTitle.Contains(Search, StringComparison.OrdinalIgnoreCase);
}

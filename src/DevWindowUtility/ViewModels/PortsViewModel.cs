using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevWindowUtility.Models;
using DevWindowUtility.Services;

namespace DevWindowUtility.ViewModels;

public partial class PortsViewModel : ObservableObject
{
    private readonly PortService _ports;
    private readonly ProcessService _processes;
    private readonly IDialogService _dialogs;
    private IReadOnlyList<ListeningPort> _all = [];

    public PortsViewModel(PortService ports, ProcessService processes, IDialogService dialogs)
    {
        _ports = ports;
        _processes = processes;
        _dialogs = dialogs;
        Refresh();
    }

    [ObservableProperty]
    private string _portInput = "";

    [ObservableProperty]
    private string _filter = "";

    [ObservableProperty]
    private string _status = "Enter a port number or refresh the listener list.";

    [ObservableProperty]
    private ListeningPort? _selectedPort;

    public ObservableCollection<ListeningPort> Ports { get; } = [];

    partial void OnFilterChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void Refresh()
    {
        try
        {
            _all = _ports.GetListeningPorts();
            ApplyFilter();
            Status = $"Showing {Ports.Count} listening TCP port(s).";
        }
        catch (Exception ex)
        {
            Status = $"Failed to read TCP table: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CheckPort()
    {
        if (!TryParsePort(out var port))
        {
            Status = "Enter a valid port (1-65535).";
            return;
        }

        var matches = _ports.FindByPort(port);
        Status = matches.Count == 0
            ? $"Port {port} is free (no TCP listener)."
            : string.Join(
                Environment.NewLine,
                matches.Select(m => $"Port {port} is in use by {m.ProcessName} (PID {m.Pid}) on {m.Address}."));

        Filter = port.ToString();
        _all = _ports.GetListeningPorts();
        ApplyFilter();
    }

    [RelayCommand]
    private void KillByPort()
    {
        if (!TryParsePort(out var port))
        {
            Status = "Enter a valid port (1-65535).";
            return;
        }

        KillMatches(_ports.FindByPort(port), $"using port {port}");
    }

    [RelayCommand(CanExecute = nameof(CanKillSelected))]
    private void KillSelected()
    {
        if (SelectedPort is null)
        {
            return;
        }

        KillMatches([SelectedPort], $"on {SelectedPort.Address}:{SelectedPort.Port}");
    }

    partial void OnSelectedPortChanged(ListeningPort? value) => KillSelectedCommand.NotifyCanExecuteChanged();

    private bool CanKillSelected() => SelectedPort is not null;

    private void KillMatches(IReadOnlyList<ListeningPort> matches, string reason)
    {
        if (matches.Count == 0)
        {
            Status = $"Nothing listening {reason}.";
            return;
        }

        var unique = matches.DistinctBy(m => m.Pid).ToList();
        var summary = string.Join(Environment.NewLine, unique.Select(m => $"{m.ProcessName} (PID {m.Pid})"));
        if (!_dialogs.Confirm($"Kill process(es) {reason}?\n\n{summary}", "Confirm kill"))
        {
            return;
        }

        foreach (var match in unique)
        {
            var result = _processes.KillByPid(match.Pid);
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
        Ports.Clear();
        var query = string.IsNullOrWhiteSpace(Filter)
            ? _all
            : _all.Where(MatchesFilter);

        foreach (var port in query)
        {
            Ports.Add(port);
        }
    }

    private bool MatchesFilter(ListeningPort port) =>
        port.Port.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase)
        || port.ProcessName.Contains(Filter, StringComparison.OrdinalIgnoreCase)
        || port.Address.Contains(Filter, StringComparison.OrdinalIgnoreCase)
        || port.Pid.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase)
        || port.Protocol.Contains(Filter, StringComparison.OrdinalIgnoreCase);

    private bool TryParsePort(out int port)
    {
        if (int.TryParse(PortInput.Trim(), out port) && port is >= 1 and <= 65535)
        {
            return true;
        }

        port = 0;
        return false;
    }
}

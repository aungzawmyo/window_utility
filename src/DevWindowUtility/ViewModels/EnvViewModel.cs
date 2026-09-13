using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevWindowUtility.Models;
using DevWindowUtility.Services;

namespace DevWindowUtility.ViewModels;

public partial class EnvViewModel : ObservableObject
{
    private readonly EnvService _env;
    private IReadOnlyList<EnvVariable> _all = [];

    public EnvViewModel(EnvService env)
    {
        _env = env;
        Scopes = Enum.GetValues<EnvironmentVariableTarget>();
        SelectedScope = EnvironmentVariableTarget.Process;
        Refresh();
    }

    public EnvironmentVariableTarget[] Scopes { get; }

    [ObservableProperty]
    private EnvironmentVariableTarget _selectedScope;

    [ObservableProperty]
    private string _filter = "";

    [ObservableProperty]
    private string _status = "Environment variables are read-only in this version.";

    [ObservableProperty]
    private EnvVariable? _selectedVariable;

    public ObservableCollection<EnvVariable> Variables { get; } = [];

    partial void OnSelectedScopeChanged(EnvironmentVariableTarget value) => Refresh();

    partial void OnFilterChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void Refresh()
    {
        try
        {
            _all = _env.Get(SelectedScope);
            ApplyFilter();
            Status = $"Showing {Variables.Count} {SelectedScope} variable(s).";
        }
        catch (Exception ex)
        {
            Status = $"Failed to read environment variables: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void CopyName()
    {
        if (SelectedVariable is null)
        {
            return;
        }

        Clipboard.SetText(SelectedVariable.Name);
        Status = $"Copied name: {SelectedVariable.Name}";
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void CopyValue()
    {
        if (SelectedVariable is null)
        {
            return;
        }

        Clipboard.SetText(SelectedVariable.Value);
        Status = $"Copied value of {SelectedVariable.Name}.";
    }

    partial void OnSelectedVariableChanged(EnvVariable? value)
    {
        CopyNameCommand.NotifyCanExecuteChanged();
        CopyValueCommand.NotifyCanExecuteChanged();
    }

    private bool HasSelection() => SelectedVariable is not null;

    private void ApplyFilter()
    {
        Variables.Clear();
        var query = string.IsNullOrWhiteSpace(Filter)
            ? _all
            : _all.Where(v =>
                v.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                || v.Value.Contains(Filter, StringComparison.OrdinalIgnoreCase));

        foreach (var variable in query)
        {
            Variables.Add(variable);
        }
    }
}

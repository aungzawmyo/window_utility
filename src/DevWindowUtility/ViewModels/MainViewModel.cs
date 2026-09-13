using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevWindowUtility.Models;
using DevWindowUtility.Services;

namespace DevWindowUtility.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        var dialogs = new DialogService();
        var processes = new ProcessService();
        var ports = new PortService();
        var hosts = new HostsService();
        var elevation = new ElevationService();
        var env = new EnvService();
        var folders = new FolderService();
        var files = new TextFileService();
        var fileDialogs = new FileDialogService();
        var json = new JsonService();
        var explorer = new FileExplorerService();
        var favorites = new FavoriteStore();

        Ports = new PortsViewModel(ports, processes, dialogs);
        Processes = new ProcessesViewModel(processes, dialogs);
        Hosts = new HostsViewModel(hosts, elevation, dialogs);
        Environment = new EnvViewModel(env);
        Folders = new FoldersViewModel(folders);
        Json = new JsonViewModel(json, files, fileDialogs, dialogs);
        Notepad = new NotepadViewModel(files, fileDialogs, dialogs);
        Explorer = new ExplorerViewModel(explorer, favorites, fileDialogs, dialogs);

        NavItems =
        [
            new NavItem("Ports", Ports),
            new NavItem("Processes", Processes),
            new NavItem("Hosts", Hosts),
            new NavItem("Environment", Environment),
            new NavItem("Folders", Folders),
            new NavItem("Explorer", Explorer),
            new NavItem("JSON", Json),
            new NavItem("Notepad", Notepad)
        ];

        Explorer.OpenInNotepadRequested += path =>
        {
            Notepad.LoadFromPath(path);
            SelectedNav = NavItems.First(item => item.Page == Notepad);
        };
        Explorer.OpenInJsonRequested += path =>
        {
            Json.LoadFromPath(path);
            SelectedNav = NavItems.First(item => item.Page == Json);
        };

        SelectedNav = NavItems[0];
    }

    public PortsViewModel Ports { get; }
    public ProcessesViewModel Processes { get; }
    public HostsViewModel Hosts { get; }
    public EnvViewModel Environment { get; }
    public FoldersViewModel Folders { get; }
    public JsonViewModel Json { get; }
    public NotepadViewModel Notepad { get; }
    public ExplorerViewModel Explorer { get; }

    public ObservableCollection<NavItem> NavItems { get; }

    [ObservableProperty]
    private NavItem? _selectedNav;

    [ObservableProperty]
    private object? _currentPage;

    partial void OnSelectedNavChanged(NavItem? value)
    {
        if (value is not null)
        {
            CurrentPage = value.Page;
        }
    }
}

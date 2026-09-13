using CommunityToolkit.Mvvm.ComponentModel;

namespace DevWindowUtility.Models;

public partial class HostsEntry : ObservableObject
{
    [ObservableProperty]
    private bool _enabled;

    [ObservableProperty]
    private string _address = "";

    [ObservableProperty]
    private string _hosts = "";

    [ObservableProperty]
    private string _comment = "";

    public bool IsCommentOnly { get; set; }

    public string Raw { get; set; } = "";
}

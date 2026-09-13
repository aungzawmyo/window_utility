namespace DevWindowUtility.Services;

public interface IFileDialogService
{
    string? OpenFile(string filter);
    string? SaveFile(string filter, string? fileName);
    string? PickFolder();
}

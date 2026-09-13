namespace DevWindowUtility.Services;

public interface IDialogService
{
    bool Confirm(string message, string title);
    void Info(string message, string title);
    void Error(string message, string title);
}

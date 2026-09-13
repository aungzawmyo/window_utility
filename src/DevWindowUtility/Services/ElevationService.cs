using System.Diagnostics;
using System.Security.Principal;
using System.Windows;

namespace DevWindowUtility.Services;

public sealed class ElevationService
{
    public bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public void RestartAsAdministrator()
    {
        var exe = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exe))
        {
            throw new InvalidOperationException("Could not locate the application path.");
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = true,
            Verb = "runas"
        });

        Application.Current.Shutdown();
    }
}

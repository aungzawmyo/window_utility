using System.ComponentModel;
using System.Diagnostics;
using DevWindowUtility.Models;

namespace DevWindowUtility.Services;

public sealed class KillResult
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
}

public sealed class ProcessService
{
    public IReadOnlyList<ProcessItem> GetProcesses()
    {
        var items = new List<ProcessItem>();
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                items.Add(new ProcessItem(
                    process.ProcessName,
                    process.Id,
                    Math.Round(process.WorkingSet64 / 1024d / 1024d, 1),
                    SafeMainWindowTitle(process)));
            }
            catch (InvalidOperationException)
            {
                // Process exited while we were reading it.
            }
            catch (Win32Exception)
            {
                // Some system processes refuse WorkingSet/title queries.
                items.Add(new ProcessItem(process.ProcessName, process.Id, 0, ""));
            }
            finally
            {
                process.Dispose();
            }
        }

        return items
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(p => p.Pid)
            .ToList();
    }

    public KillResult KillByPid(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            var name = process.ProcessName;
            process.Kill(entireProcessTree: false);
            process.WaitForExit(4000);
            return new KillResult
            {
                Success = true,
                Message = $"Killed {name} (PID {pid})."
            };
        }
        catch (ArgumentException)
        {
            return new KillResult
            {
                Success = false,
                Message = $"Process {pid} is no longer running."
            };
        }
        catch (Win32Exception ex)
        {
            return new KillResult
            {
                Success = false,
                Message = $"Access denied killing PID {pid}. Restart as administrator and try again. ({ex.Message})"
            };
        }
        catch (InvalidOperationException ex)
        {
            return new KillResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public IReadOnlyList<ProcessItem> FindByName(string name) =>
        GetProcesses()
            .Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

    private static string SafeMainWindowTitle(Process process)
    {
        try
        {
            return process.MainWindowTitle;
        }
        catch
        {
            return "";
        }
    }
}

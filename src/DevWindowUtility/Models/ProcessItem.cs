namespace DevWindowUtility.Models;

public sealed record ProcessItem(
    string Name,
    int Pid,
    double MemoryMb,
    string WindowTitle);

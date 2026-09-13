namespace DevWindowUtility.Models;

public sealed record ListeningPort(
    string Protocol,
    string Address,
    int Port,
    int Pid,
    string ProcessName);

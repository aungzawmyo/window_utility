using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using DevWindowUtility.Models;

namespace DevWindowUtility.Services;

public sealed class PortService
{
    private const int AfInet = 2;
    private const int AfInet6 = 23;

    private enum TcpTableClass
    {
        BasicListener,
        BasicConnections,
        BasicAll,
        OwnerPidListener,
        OwnerPidConnections,
        OwnerPidAll,
        OwnerModuleListener,
        OwnerModuleConnections,
        OwnerModuleAll
    }

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(
        IntPtr tcpTable,
        ref int tcpTableLength,
        bool sort,
        int ipVersion,
        TcpTableClass tableClass,
        uint reserved);

    [StructLayout(LayoutKind.Sequential)]
    private struct MibTcpRowOwnerPid
    {
        public uint State;
        public uint LocalAddr;
        public uint LocalPort;
        public uint RemoteAddr;
        public uint RemotePort;
        public uint OwningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MibTcp6RowOwnerPid
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] LocalAddr;
        public uint LocalScopeId;
        public uint LocalPort;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] RemoteAddr;
        public uint RemoteScopeId;
        public uint RemotePort;
        public uint State;
        public uint OwningPid;
    }

    public IReadOnlyList<ListeningPort> GetListeningPorts()
    {
        var names = new Dictionary<int, string>();
        var rows = new List<ListeningPort>();
        rows.AddRange(ReadIpv4Listeners(names));
        rows.AddRange(ReadIpv6Listeners(names));
        return rows
            .OrderBy(r => r.Port)
            .ThenBy(r => r.Address, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<ListeningPort> FindByPort(int port) =>
        GetListeningPorts().Where(p => p.Port == port).ToList();

    private List<ListeningPort> ReadIpv4Listeners(Dictionary<int, string> names)
    {
        var rows = ReadTable<MibTcpRowOwnerPid>(AfInet);
        var result = new List<ListeningPort>(rows.Count);
        foreach (var row in rows)
        {
            var pid = (int)row.OwningPid;
            result.Add(new ListeningPort(
                "TCP",
                new IPAddress(row.LocalAddr).ToString(),
                ToPort(row.LocalPort),
                pid,
                ResolveName(pid, names)));
        }

        return result;
    }

    private List<ListeningPort> ReadIpv6Listeners(Dictionary<int, string> names)
    {
        var rows = ReadTable<MibTcp6RowOwnerPid>(AfInet6);
        var result = new List<ListeningPort>(rows.Count);
        foreach (var row in rows)
        {
            var pid = (int)row.OwningPid;
            var address = new IPAddress(row.LocalAddr ?? []).ToString();
            result.Add(new ListeningPort(
                "TCP",
                address,
                ToPort(row.LocalPort),
                pid,
                ResolveName(pid, names)));
        }

        return result;
    }

    private static List<T> ReadTable<T>(int ipVersion)
        where T : struct
    {
        var result = new List<T>();
        var size = 0;
        GetExtendedTcpTable(IntPtr.Zero, ref size, true, ipVersion, TcpTableClass.OwnerPidListener, 0);
        if (size <= 0)
        {
            return result;
        }

        var buffer = Marshal.AllocHGlobal(size);
        try
        {
            var status = GetExtendedTcpTable(buffer, ref size, true, ipVersion, TcpTableClass.OwnerPidListener, 0);
            if (status != 0)
            {
                return result;
            }

            var count = Marshal.ReadInt32(buffer);
            var rowSize = Marshal.SizeOf<T>();
            var firstRow = IntPtr.Add(buffer, sizeof(int));
            for (var i = 0; i < count; i++)
            {
                var row = Marshal.PtrToStructure<T>(IntPtr.Add(firstRow, i * rowSize));
                result.Add(row);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return result;
    }

    private static int ToPort(uint networkPort) =>
        (int)(((networkPort & 0xFF) << 8) | ((networkPort >> 8) & 0xFF));

    private static string ResolveName(int pid, Dictionary<int, string> cache)
    {
        if (cache.TryGetValue(pid, out var name))
        {
            return name;
        }

        try
        {
            using var process = Process.GetProcessById(pid);
            name = process.ProcessName;
        }
        catch
        {
            name = pid is 0 or 4 ? "System" : "(unknown)";
        }

        cache[pid] = name;
        return name;
    }
}

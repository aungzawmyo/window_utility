# Technical notes

Implementation details for **Dev Helper** (`src/DevWindowUtility`).

## Architecture

Single WPF window. `MainWindow` binds to `MainViewModel`. The sidebar `ListBox` selects a `NavItem`. `CurrentPage` is the selected view model; `App.xaml` `DataTemplate`s map each view model to its `UserControl`.

```text
MainWindow
  └── MainViewModel
        ├── PortsViewModel      → PortsPage
        ├── ProcessesViewModel  → ProcessesPage
        ├── HostsViewModel      → HostsPage
        ├── EnvViewModel        → EnvPage
        ├── FoldersViewModel    → FoldersPage
        ├── ExplorerViewModel   → ExplorerPage
        ├── JsonViewModel       → JsonPage
        └── NotepadViewModel    → NotepadPage
```

Services are constructed in `MainViewModel` (no DI container). Explorer raises `OpenInNotepadRequested` / `OpenInJsonRequested`; `MainViewModel` loads the file and switches `SelectedNav`.

## Project settings

| Setting | Value |
| --- | --- |
| TFM | `net8.0-windows` |
| UI | WPF |
| MVVM | CommunityToolkit.Mvvm 8.4.2 |
| Manifest | `asInvoker` (`app.manifest`) |
| Icon | `Assets/app.ico` |
| Assembly name | `DevHelper` |
| Root namespace | `DevWindowUtility` |

## Ports

`PortService` calls `GetExtendedTcpTable` in `iphlpapi.dll` with `TCP_TABLE_OWNER_PID_LISTENER` for IPv4 (`AF_INET`) and IPv6 (`AF_INET6`).

- Port numbers are converted from network byte order (`low byte << 8 \| next byte`).
- PID → name uses `Process.GetProcessById` with a small cache. PID `0` / `4` display as `System`.
- Kill goes through `ProcessService.KillByPid` (`Process.Kill`, then `WaitForExit(4000)`).
- `Win32Exception` is surfaced as access denied + restart as administrator.

UDP is out of scope.

## Processes

`Process.GetProcesses()` mapped to `ProcessItem` (name, PID, working set MB, main window title). Title and memory reads are guarded; some system processes are listed with `0` MB.

Kill-by-name matches `ProcessName` case-insensitively. Search is a client-side filter on the last snapshot.

## Hosts

Path: `%SystemRoot%\System32\drivers\etc\hosts` (via `SpecialFolder.System`).

- Lines are parsed into address, hostnames, comment, and enabled (leading `#`).
- Comment-only / blank lines stay in the in-memory list and are written back unchanged.
- The grid shows only real entries. Toggle / grid checkbox flips `Enabled`.
- Save: copy to `hosts.bak` once per process, write `hosts.tmp`, then overwrite `hosts`. Encoding is `Encoding.Default` (system ANSI), matching typical hosts files.
- `UnauthorizedAccessException` shows the elevate banner and **Restart as administrator** (`ProcessStartInfo.Verb = runas` on `Environment.ProcessPath`, then shutdown).

## Environment

`Environment.GetEnvironmentVariables(Process | User | Machine)`. Read-only. Copy uses `Clipboard.SetText`.

## Folders vs Explorer

`FolderService` opens known paths in `explorer.exe` if they exist.

`FileExplorerService` lists the current directory (folders first), expands `%VARS%` and `~`, and opens items with `UseShellExecute = true`. Unauthorized children are skipped.

`FavoriteStore` persists a JSON string array at `%AppData%\DevHelper\favorites.json`. First run seeds Desktop, Documents, Downloads, the user profile, and `D:\WS_NEW` when present. Legacy `%AppData%\DevWindowUtility\favorites.json` is copied if the new file is missing.

## JSON

`System.Text.Json` with `CommentHandling = Skip` and `AllowTrailingCommas`.

- Format / minify: parse, then `JsonSerializer.Serialize` with or without indent.
- Validate builds a `JsonNodeItem` tree (object / array / scalar). Long strings are truncated in the tree preview.
- Errors include line and column from `JsonException`.

## Notepad and files

`TextFileService` reads and writes UTF-8 without BOM. Maximum size is 8 MB. Notepad tracks dirty state and confirms before New / Open. Wrap is a bound `TextWrapping` trigger.

`FileDialogService` uses WPF `OpenFileDialog`, `SaveFileDialog`, and `OpenFolderDialog` (no WinForms; WinForms types collide with WPF `Application` / `UserControl`).

## Elevation

`ElevationService.IsAdministrator()` uses `WindowsPrincipal` + `WindowsBuiltInRole.Administrator`. Restart uses `runas`. The process itself does not require admin.

## Theming

`App.xaml` defines dark brushes (`#1E1E1E` background, `#0078D4` accent) and control templates for buttons, text boxes (watermark via `Tag`), DataGrid, TreeView, and nav list items.

## Out of scope (v1)

UDP table, Windows service control, editing User / Machine environment variables, tray icon, auto-update.

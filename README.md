# Dev Helper

Windows desktop toolbox for local development. Check ports, stop stuck processes, edit the hosts file, browse folders, and inspect JSON without opening five other apps.

Repository: [aungzawmyo/window_utility](https://github.com/aungzawmyo/window_utility)

## Features

| Tool | What it does |
| --- | --- |
| **Ports** | List TCP listeners, check one port, kill by port or selected row |
| **Processes** | Search by name / PID / window title, kill selected or all matching a name |
| **Hosts** | Toggle, add, delete, and save `C:\Windows\System32\drivers\etc\hosts` |
| **Environment** | Read-only Process / User / Machine variables, copy name or value |
| **Folders** | One-click Explorer for common user, AppData, system, and `.ssh` / `.cursor` paths |
| **Explorer** | In-app file browser with persisted favorite folders |
| **JSON** | Format, minify, validate, tree view, open / save |
| **Notepad** | Simple UTF-8 editor with dirty-state confirm |

Kills always ask for confirmation (process name + PID). The first hosts save in a session writes `hosts.bak`.

## Requirements

- Windows 10 / 11 (x64)
- For development: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- For a double-click `.exe`: publish self-contained (see [developement.md](developement.md))

## Run

```powershell
dotnet run --project src/DevWindowUtility
```

Published exe (after `dotnet publish`):

```text
dist/DevHelper.exe
```

## Docs

- [developement.md](developement.md) — setup, run, build, publish
- [project_guideline.md](project_guideline.md) — conventions and how to add a tool
- [technical.md](technical.md) — architecture and implementation notes

## License

Use and modify for your own development workflow.

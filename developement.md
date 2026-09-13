# Development

How to set up, run, and ship **Dev Helper** on Windows.

## Prerequisites

- Windows 10 or 11, x64
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (this project targets `net8.0-windows`)
- Optional: Visual Studio 2022 or Cursor / VS Code with the C# extension

Check the SDK:

```powershell
dotnet --list-sdks
```

You need an `8.0.x` SDK. If `dotnet` is missing from PATH, a user-local install lives at:

```text
%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe
```

Add it for the current session:

```powershell
$env:PATH = "$env:LOCALAPPDATA\Microsoft\dotnet;$env:PATH"
```

## Solution layout

```text
window_utility/
  DevWindowUtility.sln
  src/DevWindowUtility/     WPF project (assembly name: DevHelper)
  Assets/                   app.ico, app.png (under the project)
```

Open `DevWindowUtility.sln` or the csproj directly.

## Run from source

```powershell
dotnet restore
dotnet run --project src/DevWindowUtility
```

Debug build output:

```text
src/DevWindowUtility/bin/Debug/net8.0-windows/DevHelper.exe
```

That Debug exe is **framework-dependent**. If Windows says you must install the .NET Desktop Runtime, either install the [Desktop Runtime 8](https://dotnet.microsoft.com/download/dotnet/8.0) or use `dotnet run` / a self-contained publish.

## Build

```powershell
dotnet build DevWindowUtility.sln -c Debug
dotnet build DevWindowUtility.sln -c Release
```

## Publish a standalone exe

Self-contained single file (no runtime install required):

```powershell
dotnet publish src/DevWindowUtility/DevWindowUtility.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o dist
```

Result: `dist/DevHelper.exe` (about 70 MB). `dist/` is gitignored.

Framework-dependent publish (smaller, needs Desktop Runtime 8 on the machine):

```powershell
dotnet publish src/DevWindowUtility/DevWindowUtility.csproj `
  -c Release `
  -r win-x64 `
  --self-contained false `
  -o dist-framework
```

## Icons and branding

| Item | Location |
| --- | --- |
| Window / sidebar image | `src/DevWindowUtility/Assets/app.png` |
| Exe / taskbar icon | `src/DevWindowUtility/Assets/app.ico` |
| Product name | `Dev Helper` in the csproj (`Product`, `AssemblyTitle`) |
| Assembly / exe name | `DevHelper` |

After changing `app.png`, rebuild the `.ico` with 16 / 32 / 48 / 64 / 128 / 256 sizes before publishing.

## Local data

Explorer favorites:

```text
%AppData%\DevHelper\favorites.json
```

If an older `%AppData%\DevWindowUtility\favorites.json` exists, it is copied once on first run.

## Hosts file

Writing hosts requires elevation. Use **Restart as administrator** on the Hosts page. The app starts `asInvoker` (see `app.manifest`); it does not silently request admin.

## Common issues

| Symptom | What to do |
| --- | --- |
| `dotnet` not recognized | Install the SDK or prepend `%LOCALAPPDATA%\Microsoft\dotnet` to PATH |
| Exe asks for Desktop Runtime | Use `dotnet run` or self-contained publish |
| Kill / hosts save access denied | Restart as administrator |
| JSON / Notepad refuse a file | File is over 8 MB |

## Tests

There is no test project yet. After changing a tool, run the app and exercise that page (empty state, error, and the main action).

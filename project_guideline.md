# Project guideline

Conventions for changing **Dev Helper**. Keep the app a small, local, confirm-before-damage toolbox.

## Product rules

- One window, left sidebar, one page per tool.
- Destructive actions (kill process, delete hosts row, discard unsaved note) always confirm with a name and PID / path.
- Do not silently elevate. Hosts writes that fail should tell the user to restart as administrator.
- Do not add silent kill-by-PID, hidden `runas`, or network scanners.
- Skip missing folders instead of throwing.

## Stack

- .NET 8, WPF, `CommunityToolkit.Mvvm`
- Namespace stays `DevWindowUtility` even though the product name is Dev Helper
- No extra UI kit. Use the dark styles in `App.xaml`

## Layers

| Layer | Responsibility |
| --- | --- |
| `Views/` | XAML + tiny code-behind (double-click, etc.) |
| `ViewModels/` | Commands, filters, status text |
| `Services/` | OS / file / network work |
| `Models/` | Records and observable rows |

ViewModels do not P/Invoke or parse hosts. Views do not own business logic.

## How to add a tool

1. Add a model in `Models/` if you need a new row type.
2. Add a service in `Services/` for the work.
3. Add `SomethingViewModel` with `[ObservableProperty]` and `[RelayCommand]`.
4. Add `SomethingPage.xaml` (+ code-behind) using `PageTitle`, `PageSubtitle`, `StatusText`.
5. Register a `DataTemplate` in `App.xaml` mapping the view model to the page.
6. Construct the view model in `MainViewModel` and append a `NavItem`.
7. Wire cross-page actions with events on the source view model (see Explorer → Notepad / JSON).

Keep the new page useful in one screen. If it needs a second window, stop and split the tool.

## Naming

- Product / window title: `Dev Helper`
- Exe / assembly: `DevHelper`
- Files: `PortsPage.xaml`, `PortsViewModel.cs`, `PortService.cs`
- Commands: verb first (`Refresh`, `KillByPort`, `SaveAs`)
- Status strings: short, specific, no stack traces in the status bar (use the dialog for failures)

## UI

- Reuse existing button / textbox / DataGrid styles.
- Use `EditorTextBox` for multiline editors (JSON, Notepad).
- Sidebar list uses `NavListBox` / `NavListBoxItem`. Other lists use `CompactListBox`.
- Placeholders go in `TextBox.Tag`.
- Show a status line at the bottom of the page.

## Safety checklist

Before merging a change that kills, writes system files, or deletes data:

- [ ] Confirm dialog shows what will be affected
- [ ] Access-denied path tells the user to elevate
- [ ] Hosts save stays atomic (temp file, then replace) and still writes `.bak` once per session
- [ ] No new admin-required launch flag

## Files you should not commit

- `bin/`, `obj/`, `dist/`, `.vs/`, `*.user`, `*.pdb`
- Secrets, machine-specific paths, personal notes

## Git

- Default remote: `https://github.com/aungzawmyo/window_utility.git`
- Prefer `main` for the published branch
- Commit messages: why, not a file list (for example `Add JSON tree so invalid payloads are easier to spot`)

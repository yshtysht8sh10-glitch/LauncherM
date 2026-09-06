# Current Status

Last updated: 2026-09-05

## Implemented

- WPF launcher prototype
- Four launcher display areas and an item detail area
- Item registration/editing/deletion flow in the existing UI
- Individual launch and folder-open handlers
- Icon loading from executable paths
- Settings and saved data text-file handling

## Investigation Completed

- Confirmed the four launcher areas are fixed XAML `StackPanel`s with code-side lists indexed by launcher number and display order.
- Confirmed the current item record and the Shift-JIS colon-delimited `de.txt` / symbolic `se.txt` persistence formats.
- Confirmed there is no ID, type, arguments, working directory, or schema version in the current data.
- Documented a four-Workspace legacy mapping and a separate, backup-preserving migration strategy.
- Documented the recommended generic LaunchItem model and launch responsibilities in `docs/ai/workspace-design.md`.

## Implemented (v0.1)

- Added Workspace/LaunchItem/LaunchItemType domain models.
- Added versioned `workspaces.json` persistence and read-only `de.txt` migration into four Workspaces.
- Added `LaunchService` for Application, Folder, File, URL, and persistent Command (`cmd.exe /k`) launch.
- Added minimal Workspace selection, individual launch, and “すべて起動” UI as the startup window.
- Restored the legacy-inspired dark/blue layout: header gear, blue layout blocks, left-side item tiles, and right-side selected-item details.

## Verified

- Visual Studio 2022 MSBuild Rebuild succeeds with 0 errors for Debug.
- Output: `LauncherM/bin/Debug/LauncherM.exe`
- Startup resource path was corrected to `01_UI/0010_MainWindow.xaml`.

## Planned

- Explicit Workspace / Group model
- Generic Launch Item types
- Reliable batch launch
- URL and command item support
- Better persistence model
- Add read-only Workspace/LaunchItem classes and a legacy `de.txt` mapping adapter with focused checks; do not change the UI or persistence cutover in that step.

## Open Decisions

- Preservation format for legacy `PathFileSelect` and `PathImagevisual` data.
- Versioned new persistence format and exact backup/rollback behavior.
- Command quoting/lifetime/error policy and batch-launch failure semantics.

## Verified

- Visual Studio 2022 MSBuild Debug Build succeeds with 0 errors; existing unused-field warnings remain.
- The new startup XAML and all new model/repository/service files compile into `LauncherM/bin/Debug/LauncherM.exe`.
- The UI Build succeeds with the legacy-inspired Workspace window as the startup resource.
- The Workspace window was visually tuned toward the legacy reference: gray header, dark panels, outlined gray controls, blue selection accents, tile layout, and right-side detail card.

## Not Yet Verified

- GUI startup, external Application/Folder/URL/Command launches, JSON restart loading, and legacy migration with a real user `de.txt` were not executed end-to-end in this pass.
- The gear currently opens the existing settings window with a temporary settings object; persistence of those settings from the new window is not yet integrated.
- The reference image was used for visual comparison, but pixel-level visual QA of the running WPF window remains pending.

These items are not implemented by this restart task.

## Known Issues

- Some source contains legacy absolute paths such as `F:\作業用\...`; these are environment-specific and must not be treated as portable behavior.
- Runtime behavior for every launch type and saved-data migration has not been comprehensively verified.
- The build emits unused-field warnings.

## Build Environment

- Visual Studio 2022 Community MSBuild 17.14
- .NET Framework reference assemblies 4.8
- C# compiler language version set to `latest` for the legacy project

## Next Investigation

Confirm persistence format and launch behavior on a clean machine, then decide whether the current data model can be incrementally adapted to Workspace / Launch Item concepts.

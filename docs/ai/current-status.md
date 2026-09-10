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
- Added drag-and-drop registration to the left item area: dropped Windows files, folders, executables, and `.lnk` shortcuts are added to the selected Workspace and saved to `workspaces.json`.
- Added Windows-associated icons for registered files and folders in both item tiles and the selected-item detail area.
- Added multi-selection and deletion for LaunchItem cards: active cards are highlighted, Ctrl-click toggles selection, mouse dragging across cards adds them to the selection, and Delete/right-click can remove the selected cards.
- Added Workspace management from the header menu: create, rename, delete, and toggle display of all Workspaces' LaunchItems.
- Added LaunchItem add/edit dialogs for name, type, target, and arguments; changes are persisted to `workspaces.json`.
- URL LaunchItems now support optional browser and browser-profile settings. Chrome/Edge use `--profile-directory`, Firefox uses `-P`, and Default preserves OS browser launching.

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

## Browser Workspace extension (2026-09-09)

Implemented: URL editor detects Chrome/Edge profile names from the standard user-data
`Local State` file's `profile.info_cache`; only display name and directory identifiers
are used. The selected directory is stored in BrowserProfile, not the display name.
Manual profile entry remains available. BrowserWindowGroup is an optional version-1
JSON member. Explicit Chrome/Edge groups launch once per browser/profile/group with
`--new-window` and multiple URLs. Empty groups retain individual launching. Default
and Firefox do not support window grouping. Workspace defaults are deferred.

Verified: Debug and Release configuration builds; 12 focused checks including grouping,
failure continuation, old JSON, serialization and actual profile metadata detection.
Both browsers accepted real launch commands and additional windows appeared; exact
A/B tab placement and C separation remain NOT VERIFIED because Computer Use stopped.
Cold starts, GUI save/restart, and distinct-profile window identity remain unverified.

2026-09-09 Chrome-not-found fix: reproduced in an x86 process: both ProgramFiles and
ProgramFilesX86 resolve to Program Files (x86), while installed Chrome is in Program
Files. Added App Paths lookup in both registry views plus ProgramW6432 fallback.
All 12 checks pass when compiled x86. Release fix built and the normal bin/Release executable updated after LauncherM closed.

No credentials, cookies, tokens, login automation, CDP, browser extensions or browser
UI automation are part of the product. Authentication remains in browser profiles.

## URL icons (2026-09-09)

Implemented: URL LaunchItems with no explicit IconPath fetch and cache the site's
favicon under `%LOCALAPPDATA%\LauncherM\WebsiteIcons`. LauncherM first requests the
site's conventional `/favicon.ico`; if the site refuses that request, it falls back
to Google's favicon service. Existing URL items are filled after startup without
blocking the window, and new/edited/dropped URLs are filled before saving. The edit
dialog exposes an icon path and file picker for ICO, PNG, JPEG, BMP, GIF, EXE and LNK.
Raster image files are rendered directly instead of showing their Windows file-type
icon. The existing right-click `アイコンを変更` action remains available.

Verified: Debug build succeeds. An x86 focused check downloaded the ChatGPT favicon,
validated it through WPF's image decoder, and confirmed same-origin cache reuse.

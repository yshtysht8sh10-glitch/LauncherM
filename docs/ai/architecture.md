# Architecture

## Current Architecture

- Solution: `LauncherM.sln`
- Active project: `LauncherM/LauncherM.csproj`
- UI: WPF (`Window`, XAML)
- Target: .NET Framework 4.8
- Main UI: `01_UI/0010_MainWindow.xaml` and code-behind
- Settings UI: `01_UI/0020_EnvironmentWindow.xaml`
- Application logic: partial `MainWindow` files under `02_Application`
- Infrastructure: text-file reader/writer helpers under `04_Infrastructure`
- Data: `StructureDataSave` / `StructureDataSavePartData` stored in the main window code-behind

The current prototype uses a flat list of launcher slots and serialized text files (`de.txt`, `se.txt`). It is not yet a Workspace/Launch Item model.

## Investigated Existing Structure

- The four launcher areas are fixed WPF `StackPanel`s: `stackLauncher00` through `stackLauncher03` in `01_UI/0010_MainWindow.xaml`.
- `Const.MaxQuantityStackLauncher` is fixed at `4`. Visibility and colors are also four named properties (`DispLauncher00` ... `03`).
- `MainWindow` creates `m_StackLauncher` as a four-element `List<StackPanel>` and `m_ListListButtonElementLink` as a nested list indexed by `[launcher number][display order]`. These lists reduce event-handler duplication, but the XAML controls and several settings/handlers remain explicitly duplicated.
- Each displayed item is a WPF `Button` whose content is a `StackPanel` containing an image and text. The UI is directly coupled to the saved data and uses code-behind event handlers rather than data binding or view models.

## Current Data Model and Persistence

The persisted item is the nested `MainWindow.StructureDataSavePartData` class. It contains `NoAffiliationLuncher`, `OrderDisplayInLuncher`, `PathExe`, `PathImageIcon`, `PathImagevisual`, `StringTitle`, `StringMemo`, and `PathFileSelect` (`List<string>`). There is no item ID, type, arguments, working directory, or schema/version field.

`de.txt` is read and written in the current working directory using Shift-JIS. Each item is one colon-delimited line: launcher number, display order, executable/target path, icon path, visual path, title, memo, then zero or more select.def/lua paths. Colons and other reserved characters are escaped by the existing replacement helpers. `se.txt` stores display settings as symbolic `key:value` lines, also in Shift-JIS. Neither format has an explicit version or atomic-write/backup protocol. Paths are absolute when selected by the user, so portability and missing-path handling are risks.

## Recommended Migration Direction

Do not replace the existing persistence in-place yet. Introduce a small domain model and an adapter that can read the legacy `de.txt`, map launcher numbers 0..3 to four generated Workspaces, and map each part to a LaunchItem. Preserve the legacy files until the new format has been written and validated; write the new format to a separate versioned file and keep a backup before conversion. The adapter should be the only place that knows the legacy colon format.

The recommended initial model is described in `docs/ai/workspace-design.md`. It deliberately leaves ordering, delays, and readiness policies out of the first implementation while keeping the model extensible.

## v0.1 Implementation

`03_Domain/WorkspaceModels.cs` now contains `Workspace`, `LaunchItem`, `LaunchItemType`, and versioned `WorkspaceDocument`. URL LaunchItems optionally store `Browser` (`Default`, `Chrome`, `Edge`, or `Firefox`) and `BrowserProfile`; missing fields in older JSON default to the OS browser. `04_Infrastructure/WorkspaceRepository.cs` stores `workspaces.json` using the .NET Framework `DataContractJsonSerializer` and migrates legacy `de.txt` only when the JSON file does not exist. `02_Application/LaunchService.cs` owns individual and sequential Workspace launch, including browser-specific URL process arguments. `01_UI/WorkspaceWindow.xaml` provides the minimal selection, individual launch, and launch-all UI and is the current startup window.

The v0.1 UI keeps the old LauncherM character: dark gray panels, blue header accents, a gear settings entry point, and the four blue/purple layout blocks. The blocks currently toggle visual opacity only; they are “Legacy Layout Visibility Controls,” reserved for later reconnecting to panel visibility settings. Registered LaunchItems are shown as tiles on the left, with the selected item’s details and actions on the right.

The UI was tuned against the legacy reference image: gray header rather than blue fill, near-black content panes, gray outlined controls, bright blue selection/icon accents, and a fixed left/right split. The header also includes add/edit/menu affordances; add/edit remain placeholders until their data-entry flows are implemented.

## Target / Proposed Architecture

Future work may introduce an explicit Workspace and Launch Item model, with generic launch types and optional working directories. This is proposed architecture only; it is not currently implemented.

## Reusable Areas

The existing item data, settings persistence, WPF layout, individual launch handlers, and icon loading are useful starting points. They should be preserved until their behavior is understood.

## Browser Workspace extension (2026-09-09)

See workspace-design.md for Browser Context and BrowserWindowGroup semantics and
current-status.md for verification. LaunchService groups explicit Chrome/Edge window
requests, retains per-item failure reporting, and uses ordinary browser CLI arguments.
BrowserProfiles reads friendly profile metadata; persistence keeps directory IDs.
No authentication or browser session control is introduced. App Paths is searched in
both registry views so an x86 launcher can discover a 64-bit browser.

URL favicon retrieval is isolated in `04_Infrastructure/WebsiteIconCache.cs`. Cached
files live in LocalApplicationData, while `LaunchItem.IconPath` continues to represent
both automatic cache files and user-selected local icon/image files. The UI loads
raster files directly and uses the existing Windows associated-icon path for executable,
shortcut, file, and folder icons.

# Current Status

Last updated: 2026-09-05

## Implemented

- WPF launcher prototype
- Four launcher display areas and an item detail area
- Item registration/editing/deletion flow in the existing UI
- Individual launch and folder-open handlers
- Icon loading from executable paths
- Settings and saved data text-file handling

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

These items are not implemented by this restart task.

## Known Issues

- Legacy and newer source layouts coexist in `LauncherM` and `WpfApp1`; only the former is the active solution project.
- Some source contains legacy absolute paths such as `F:\作業用\...`; these are environment-specific and must not be treated as portable behavior.
- Runtime behavior for every launch type and saved-data migration has not been comprehensively verified.
- The build emits unused-field warnings.

## Build Environment

- Visual Studio 2022 Community MSBuild 17.14
- .NET Framework reference assemblies 4.8
- C# compiler language version set to `latest` for the legacy project

## Next Investigation

Confirm persistence format and launch behavior on a clean machine, then decide whether the current data model can be incrementally adapted to Workspace / Launch Item concepts.

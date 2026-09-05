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

## Target / Proposed Architecture

Future work may introduce an explicit Workspace and Launch Item model, with generic launch types and optional working directories. This is proposed architecture only; it is not currently implemented.

## Reusable Areas

The existing item data, settings persistence, WPF layout, individual launch handlers, and icon loading are useful starting points. They should be preserved until their behavior is understood.

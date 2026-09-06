# Workspace / LaunchItem Design Investigation

## Status

This is the design record for the investigation phase. It is not an implementation specification yet.

The v0.1 implementation now follows this design with a version-1 `workspaces.json`, a legacy read-only migration path, and a single `LaunchService`. The UI intentionally does not yet provide full Workspace/LaunchItem editing; JSON can be edited manually for the first usable workflow.

The current UI integrates the model into the legacy LauncherM interaction pattern: Workspace selection and launch-all are in the blue header, registered items are left-side tiles, and the selected item is shown in a right-side detail panel. The gear and blue layout blocks are retained as legacy UI affordances; the blocks currently provide only an ON/OFF visual state and are not treated as dead UI.

## Proposed model

```csharp
enum LaunchItemType { Application, Folder, File, Url, Command }

sealed class Workspace {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<LaunchItem> LaunchItems { get; set; }
}

sealed class LaunchItem {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public LaunchItemType Type { get; set; }
    public string Target { get; set; }
    public string Arguments { get; set; }
    public string WorkingDirectory { get; set; }
    public string Browser { get; set; }       // Url only; optional, defaults to Default
    public string BrowserProfile { get; set; } // Url only; optional browser profile name
    public string IconPath { get; set; }
    public string Memo { get; set; }
}
```

`Workspace` owns grouping; `LaunchItem` is the user-facing common concept. `Target` is intentionally generic. `Arguments` and `WorkingDirectory` cover VS Code, browsers, and most command-line tools without product-specific classes. `IconPath` and `Memo` preserve useful existing UI data. `Order` should be added when ordering is actually implemented, not preemptively.

## Existing-to-new mapping

Generate four Workspaces (`Launcher 1` through `Launcher 4`) from legacy launcher numbers 0 through 3. Map `StringTitle` to `Name`, `PathExe` to `Target`, `PathImageIcon` to `IconPath`, and `StringMemo` to `Memo`. The legacy `OrderDisplayInLuncher` becomes the list order during conversion. `PathImagevisual` and `PathFileSelect` have no generic meaning yet; preserve them in a migration extension/metadata field or leave them in the legacy record until their use is explicitly designed. Do not silently discard them.

## Launch strategy on .NET Framework 4.8

- Application: `ProcessStartInfo` with the executable as `FileName`, optional `Arguments`, and explicit `WorkingDirectory` when supplied.
- Folder: `explorer.exe` with the folder path, after existence validation.
- File: `ProcessStartInfo` with `UseShellExecute = true` so Windows selects the associated application.
- URL: `ProcessStartInfo` with `UseShellExecute = true`.
- Command: initially use `cmd.exe /c` with a carefully constructed argument and `WorkingDirectory`; use `/k` only when the user explicitly wants a persistent console. Keep `CreateNoWindow` and shell behavior as policy, not item-specific string hacks. Quoting and error reporting must be tested before implementation.

The UI should call a small `LaunchService` (or equivalent application-level collaborator). `Workspace` should own item membership, not call `Process.Start` itself. A separate service is justified once batch launch exists; until then, a minimal service wrapping the existing handlers is sufficient. The View should not loop over `Process.Start` directly.

## Migration and safety

Automatic migration is feasible because launcher number and display order are explicit. It is not lossless for the MUGEN-oriented `PathFileSelect` and visual-image semantics unless those are preserved as legacy metadata. Keep `de.txt` and `se.txt` readable during a transition, write a versioned new file separately, create a timestamped backup before conversion, and never overwrite valid data after a parse error. On malformed input, retain the original file, report the affected line, and allow the application to continue with the last valid/empty in-memory state only by explicit policy.

## UI reuse

The right-hand detail panel can become LaunchItem editing with limited changes; the existing title, memo, image, target selection, Play, and folder-open concepts are reusable. The four fixed panels can initially be treated as four generated Workspaces, then replaced by a Workspace list when the model is proven. Add “Launch all” at the Workspace header/selection level. Full WPF binding/ViewModel conversion is not required for the first slice.

## Risks and decisions still open

- Current save files are relative to the process working directory, have no version, and use fragile colon parsing.
- Absolute target and image paths may not exist on another machine.
- Existing code has MUGEN-specific fields mixed into the generic item record; they must not leak into Launcher Core.
- Shell quoting, command lifetime, and failure reporting need focused tests.
- Whether `PathFileSelect` is retained as extension metadata or migrated into a future MUGEN extension remains undecided.

## Smallest next implementation step

Add read-only domain classes plus a legacy `de.txt` adapter and unit-level mapping checks, without changing the UI or writing new data. This proves the four-Workspace conversion and exposes data-loss cases before any persistence cutover.

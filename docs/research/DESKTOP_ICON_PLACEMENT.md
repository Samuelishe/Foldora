# Desktop Icon Placement Research

Дата: 2026-07-01.

## Summary

Foldora currently creates the folder in the correct Desktop directory, but does not control the visual position of the desktop icon inside Explorer's desktop view.

The current legacy registry command path is not enough to implement "create here" under the original right-click point. The command receives a target path, not desktop icon-view coordinates. A post-create positioning implementation may be possible, but it requires Shell view COM research and a separate implementation spike; it is not a small MVP-safe patch.

## Current Foldora Command Path

For `Directory.Background`, registry commands use `%V`:

```text
"<Foldora.MenuHost.exe>" create --target "%V" --entry-id "<entry-id>"
```

`MenuHostCommandParser` accepts only:

```text
create --target "<directory>" --entry-id "<entry-id>"
```

`FolderMenuEntryActionService.CreateAsync` then:

1. resolves the saved menu entry by `entry-id`;
2. chooses an available folder name in the target directory;
3. calls `Directory.CreateDirectory`;
4. applies `desktop.ini` through `DesktopIniService`.

No cursor position, monitor, DPI, desktop view item index or icon-view coordinate is present in the current command shape.

## Question 1: Can Legacy Registry Command Receive Right-Click Coordinates?

No supported coordinate channel was found in the current legacy static verb approach. Foldora uses documented command placeholders for target path selection (`%1` for directory, `%V` for directory background). This gives enough information to create the folder in the correct filesystem directory, but not enough information to know where the user opened the desktop context menu.

`GetCursorPos` can read the current cursor position, but it is not a reliable substitute for the original desktop right-click point. By the time `Foldora.MenuHost.exe` starts, the cursor may be over the chosen menu item or submenu, not over the original desktop background point.

## Question 2: Can Foldora Move The Desktop Icon After Creation?

Potentially, but not through the current Core/MenuHost path alone.

The supported-looking route is Shell view integration:

- find the Explorer desktop view;
- bind/query the folder view interface;
- identify the newly created item;
- call a folder-view positioning API such as `IFolderView::SelectAndPositionItems`;
- gracefully no-op if the desktop view or item cannot be found or if Explorer policy/layout prevents positioning.

This does not require a COM shell extension by itself, but it does require COM interop, desktop view discovery, threading/apartment care and manual Windows 11 testing.

## API Candidates

- Shell COM folder view: `IFolderView` / `IFolderView2`. Candidate for post-create selection and positioning. The relevant API family includes `IFolderView::SelectAndPositionItems`.
- Explorer/Shell window discovery: likely through Shell windows / service provider / shell browser view discovery. This needs a focused prototype before production design.
- `SHChangeNotify`: useful for notifying the Shell that filesystem or association-related state changed. It can help refresh visibility/icon state, but it does not provide placement coordinates and does not position desktop icons.
- `GetCursorPos`: diagnostic-only candidate for comparing current cursor position with expected click position. It should not become default positioning behavior without proof because it can capture menu-item position.
- Desktop `SysListView32` messages such as `LVM_SETITEMPOSITION`: possible unsupported hack path. It is fragile across Windows versions, Explorer restarts and desktop implementations, so it should not be the first production route.
- `IExplorerCommand` / modern shell integration: future research route for richer invocation context, but not implemented in current MVP and not equivalent to a quick registry change.

## Risks And Constraints

- Auto arrange icons can override or reject manual positioning.
- Align icons to grid can snap the requested point.
- Multiple monitors require screen-to-desktop-view coordinate mapping.
- DPI scaling affects coordinate conversion.
- Work area, taskbar position and virtual desktop layout affect valid target points.
- Explorer can restart or recreate the desktop view between folder creation and positioning.
- Windows 11 desktop internals can differ from older Explorer behavior.
- The new item may not be visible in the desktop view immediately after `Directory.CreateDirectory`.
- Positioning can affect selection/focus in the user's desktop session.
- COM interop must stay isolated in Shell/App-specific code, not Core or WPF code-behind.

## MVP-Safe Future Design Sketch

If this becomes the next implementation spike, keep it isolated:

- add a Shell-layer abstraction, for example `IDesktopIconPositioningService`;
- provide a Windows implementation behind that abstraction and fake implementation for tests;
- keep Core model/settings JSON unchanged;
- run only for Desktop target, no-op for normal directories;
- do not use `GetCursorPos` as production truth until a prototype proves it captures the original invocation point;
- no-op with a clear diagnostic result if auto-arrange, missing desktop view or missing item prevents positioning;
- keep `MenuHost` no-console and keep registry writes under Foldora-owned roots only;
- define a manual smoke matrix for auto-arrange, align-to-grid, multi-monitor, DPI scaling and Explorer restart.

## Prototype Command

The current prototype exposes only a manual diagnostic command:

```text
foldora diagnostics desktop-icon-position --name "<desktop item name>" --x <int> --y <int> [--coordinate-space screen|view]
```

Scope:

- moves an already existing desktop item by display name;
- accepts explicit coordinates supplied by the user;
- does not create folders;
- does not infer original right-click coordinates;
- does not use `GetCursorPos` as production truth;
- does not change legacy registry command shape;
- does not change `Foldora.MenuHost.exe create` behavior.

Implementation note: the prototype is isolated in `Foldora.Shell.Desktop` behind `IDesktopIconPositioningService`. It uses Explorer desktop folder view APIs where available and returns controlled failure if the desktop view, item or positioning operation cannot be resolved.

## Manual Prototype Result

Manual Windows 11 checks confirmed that `diagnostics desktop-icon-position` can move existing desktop icons using both screen and view coordinates. When the target grid area is occupied, Explorer can shift neighboring icons according to desktop grid/layout behavior; this was accepted as usable for MVP best-effort placement.

## Best-Effort Integration

`Foldora.MenuHost` now uses the positioning service only after successful desktop-background `create`:

1. Parse `create --target "%V" --entry-id "<entry-id>"`.
2. Capture current cursor screen position as early as possible in the create branch.
3. Call existing Core `FolderMenuEntryActionService.CreateAsync`.
4. If the target directory equals `Environment.SpecialFolder.DesktopDirectory`, attempt to move the created desktop item by folder name using `DesktopIconCoordinateSpace.Screen`.
5. Treat positioning failure as non-fatal: folder creation success remains exit code `0`.

This is approximate. Legacy registry commands still do not pass the original right-click point. The captured cursor can be the menu/submenu selection point, keyboard navigation can produce different behavior, and Explorer can snap or shift icons according to grid/auto-arrange settings.

## MenuHost Placement Diagnostics And Retry

Manual production testing showed that the standalone diagnostic command can move existing desktop items, while the Explorer/MenuHost create path still placed the new folder in Explorer's free grid slot. That points to timing or production-path visibility, not to the basic positioning capability.

MenuHost now writes one JSONL record per `create` command to:

```text
%AppData%\Foldora\Logs\menuhost-placement.log
```

Each record includes command kind, target path, entry id, created folder path/name, desktop detection details, cursor capture state, coordinate space, attempt count, final positioning result/message, exception details if present and final exit code.

MenuHost also uses bounded retry only for this specific race:

```text
Desktop item was not found: <name>
```

Retry is only active when the target is Desktop and cursor capture succeeded. It does not retry non-desktop targets, missing cursor, COM rejection, invalid arguments or other failures. The default policy is 10 attempts with a 125 ms delay between attempts. Failure remains non-fatal when folder creation succeeded.

## Recommendation

Do not promise exact "create under original right-click point" in the current MVP. Keep best-effort placement enabled for desktop create flow, verify it manually in publish smoke, and only pursue heavier shell integration if the approximation is not good enough.

## Sources

- Microsoft Learn: `IFolderView::SelectAndPositionItems` - https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-ifolderview-selectandpositionitems
- Microsoft Learn: `SHChangeNotify` - https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shchangenotify
- Microsoft Learn: `GetCursorPos` - https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos
- Microsoft Learn: `LVM_SETITEMPOSITION` - https://learn.microsoft.com/en-us/windows/win32/controls/lvm-setitemposition

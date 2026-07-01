# Roadmap

## Implemented MVP

Foldora currently has a working MVP loop:

- WPF editor with custom window chrome.
- Compact/edit entry cards for menu entries.
- Settings gear and persisted language setting `ru`/`en`.
- Minimal localization foundation for main WPF labels/buttons.
- Grouped WPF presentation sections for root-level and one-level grouped entries.
- User menu entries with:
  - `DisplayName`;
  - `DefaultFolderName`;
  - `GroupName` for one-level grouping;
  - enabled/disabled state;
  - imported `.ico`.
- Add/remove entries.
- Staged save/cancel.
- Staged icon import into `%AppData%\Foldora\icons`.
- Direct `.ico` preview in WPF.
- Safe HKCU legacy Explorer menu registration under Foldora-owned roots.
- Visible menu shape:

```text
Создать папку
  Цветные
    Синяя
    Красная
  Готические
    Череп
  Музыка
```

- No-console `Foldora.MenuHost.exe` for Explorer menu commands.
- Best-effort desktop icon placement for MenuHost desktop-background create flow.
- Dev/manual publish layout in `artifacts/publish/Foldora`.
- Small menu icons through registry `Icon`.
- `register-menu`, `register-menu --dry-run`, `register-menu --host-path`, `unregister-menu`.
- `menu reset --yes`.
- `apply`, `create`, and `clear` through `desktop.ini`.
- Deletion-friendly default desktop.ini policy:
  - folder: `ReadOnly`;
  - `desktop.ini`: `Hidden`.
- Startup diagnostics in `%AppData%\Foldora\Logs\startup-error.log`.
- Resource policy for future icons/fonts/assets.
- 0BSD repository license and third-party notices.

## Current Publish Foundation

The MVP has a dev/manual publish foundation for repeatable local Explorer testing:

- `scripts/publish-dev.ps1` creates a framework-dependent Release layout:

```text
artifacts/publish/Foldora/
Foldora.App.exe
Foldora.Cli.exe
Foldora.MenuHost.exe
```

- The script does not register Explorer integration and does not start the app.
- WPF resolves sibling `Foldora.MenuHost.exe` when launched from the publish folder.
- CLI still supports explicit `register-menu --host-path "<path-to-Foldora.MenuHost.exe>"`.

## Next Stage

Best-effort desktop placement verification before installer/release polish:

- Manually verify published MenuHost desktop-background create near the cursor/menu selection area.
- If placement fails, inspect `%AppData%\Foldora\Logs\menuhost-placement.log` and use the latest JSONL entry for the next debugging step.
- Define manual smoke coverage for auto-arrange icons, align-to-grid, multi-monitor, DPI scaling and Explorer restart.
- Keep installer/MSIX as a later step until this UX-critical limitation has an explicit decision.

## Known MVP Limitations

- Modern Windows 11 compact context menu is not implemented.
- Legacy menu may appear under `Show more options`.
- Exact original right-click desktop placement is not available from the legacy command; Foldora uses best-effort positioning near the captured cursor/menu selection point and Explorer may snap/shift icons. This is tracked as `TD-0001`.
- No installer/MSIX yet.
- No production Program Files layout or code signing yet.
- No pack import/export yet.
- No PNG-to-ICO conversion.
- No full nested tree storage/runtime beyond one-level `GroupName`.
- No drag-and-drop group ordering.
- No group icons.
- No orphan icon cleanup for imported `.ico`.
- No user-facing diagnostics for `Foldora.MenuHost.exe` failures launched from Explorer.
- First-created desktop folder default-icon timing is currently not reproduced; tracked as `TD-0002` monitor item.
- Full runtime localization of all status/error/detail strings is not complete.
- No Explorer restart or icon cache reset flow.

## Future

- Installer/MSIX after stable publish layout.
- Modern Windows 11 context menu research.
- Exact original right-click coordinate source research, if best-effort placement is not enough.
- Modern `IExplorerCommand` research for advanced shell integration.
- Optional Shell refresh notification investigation for desktop.ini apply/create timing, only if `TD-0002` is reproduced again and scoped.
- Full tree menu runtime/storage beyond current one-level groups.
- Drag-and-drop ordering.
- Group icons.
- Pack import/export.
- PNG-to-ICO conversion.
- Preview generation/cache if direct `.ico` preview becomes insufficient.
- Orphan icon cleanup.
- Optional repair/normalize command only if old `System`-attribute folders become a real user need.

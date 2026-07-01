# Roadmap

## Implemented MVP

Foldora currently has a working MVP loop:

- WPF editor with custom window chrome.
- Compact/edit entry cards for menu entries.
- Settings gear and persisted language setting `ru`/`en`.
- Localization foundation for WPF labels/status/defaults and localized default menu title mode using embedded `ru`/`en` catalogs.
- Localized WPF validation rendering from Core invariant issue codes/parameters.
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
- Per-user install/uninstall scripts for `%LocalAppData%\Programs\Foldora`.
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

## Current Install Foundation

The MVP has dev/manual publish and per-user install foundations:

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
- `scripts/install-user.ps1` reuses the dev publish output and copies the app to `%LocalAppData%\Programs\Foldora`.
- `scripts/uninstall-user.ps1` unregisters Foldora-owned HKCU menu roots and removes installed binaries.
- User data remains under `%AppData%\Foldora` and is preserved by uninstall unless `-RemoveUserData` is explicitly used.
- `Foldora.MenuHost.exe` is a short-lived no-console command host launched by Explorer on user menu clicks; it is not a service, tray app, background helper or autostart process.

## Next Stage

Per-user install smoke and release polish before full installer work:

- Manually verify installed app enables Explorer integration with registry commands pointing to `%LocalAppData%\Programs\Foldora\Foldora.MenuHost.exe`.
- Manually verify published MenuHost desktop-background create near the cursor/menu selection area.
- If placement fails, inspect `%AppData%\Foldora\Logs\menuhost-placement.log` and use the latest JSONL entry for the next debugging step.
- Define manual smoke coverage for auto-arrange icons, align-to-grid, multi-monitor, DPI scaling and Explorer restart.
- Keep MSI/MSIX/winget/code signing as later work after the per-user layout is proven.

## Known MVP Limitations

- Modern Windows 11 compact context menu is not implemented.
- Legacy menu may appear under `Show more options`.
- Exact original right-click desktop placement is not available from the legacy command; Foldora uses best-effort positioning near the captured cursor/menu selection point and Explorer may snap/shift icons. This is tracked as `TD-0001`.
- No MSI/MSIX installer yet.
- No Program Files layout, winget package or code signing yet.
- No pack import/export yet.
- No PNG-to-ICO conversion.
- No full nested tree storage/runtime beyond one-level `GroupName`.
- No drag-and-drop group ordering.
- No group icons.
- No orphan icon cleanup for imported `.ico`.
- No user-facing diagnostics for `Foldora.MenuHost.exe` failures launched from Explorer.
- First-created desktop folder default-icon timing is currently not reproduced; tracked as `TD-0002` monitor item.
- Full localization is not complete: CLI defaults/diagnostics/validation output and startup fatal dialog remain tracked debt.
- Planned complete locale batch after catalog hardening: `zh-Hans`, `de`, `es`, `fr`, `ja`, `pt-BR`, `ko`; incomplete locales must not be exposed in Settings UI.
- No Explorer restart or icon cache reset flow.

## Future

- MSI/MSIX or winget after stable per-user install layout.
- Modern Windows 11 context menu research.
- Full localization pass for planned locales after `ru`/`en` catalog architecture is stable.
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

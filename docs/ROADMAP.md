# Roadmap

## Implemented MVP

Foldora currently has a working MVP loop:

- WPF editor with custom window chrome.
- MainWindow focused on menu editing; Explorer integration, installation info and danger reset live in SettingsWindow.
- Compact/edit entry cards for menu entries.
- Settings gear and persisted language setting for enabled locales `bg`, `cs`, `de`, `en`, `es`, `fr`, `hi`, `hu`, `id`, `it`, `ja`, `ko`, `nl`, `pl`, `pt-BR`, `pt-PT`, `ro`, `ru`, `th`, `tr`, `uk`, `vi`, `zh-Hans`, `zh-Hant`.
- Localization foundation for WPF labels/status/defaults and localized default menu title mode using embedded complete catalogs.
- First-run WPF language detection from `CultureInfo.CurrentUICulture`, with unsupported languages persisted as `en` and manual Settings choice preserved.
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
Create folder
  Colors
    Blue
    Red
  Work
    Documents
  Media
    Music
    Pictures
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

UI/UX audit baseline, correctness cleanup and visual polish:

1. UI/UX audit baseline:
   - `docs/UI_AUDIT.md` tracks confirmed design debt from manual inspection.
   - Keep audit findings in docs/TECH_DEBT instead of chat-only notes.
2. Small UX correctness cleanup:
   - redundant content-area `Manage in Settings` action removed from MainWindow;
   - raw `Unsaved changes: True/False` replaced with localized saved/unsaved status;
   - shared action button padding/min-height tuned for localized labels;
   - SettingsWindow scroll content gutter added.
3. Settings clarity/help/path actions cleanup:
   - explicit Foldora Explorer menu status wording;
   - user-facing `Preview changes` label for dry-run registry preview;
   - small self-authored help/info affordances;
   - Open/Copy actions for installation/user-data/MenuHost paths.
4. Settings help/layout regression fix:
   - passive non-button `?` glyphs for hover-only help;
   - wrapped long help tooltip text;
   - compact inline action buttons for Settings Explorer/path rows.
5. Help/About/Instructions window foundation:
   - Settings includes a small Help/About entry point;
   - Help/About explains basic workflow, `.ico` selection, Explorer menu location, MenuHost role, data paths, uninstall behavior and license notes;
   - this is not a full help center or screenshot-based documentation system.
6. Visual polish pass v1:
   - addressed for the current MVP windows: calmer surfaces, spacing, hierarchy, empty state, status chips, Settings path rows, Help step rows and shared WPF resource styles.
   - further polish should be driven by manual feedback and long-locale checks rather than a broad redesign.
7. Branding/assets pass:
   - app icon;
   - exe/window icon;
   - README hero/mockup after the UI looks ready for public presentation.
8. Later layout/localization polish:
   - handle feedback from long labels and non-Latin scripts across enabled locales.

Per-user install smoke and release polish before full installer work:

- Manually smoke the SettingsWindow Explorer menu flow after the cleanup: dirty draft block, enable/disable, technical details and reset reload back into MainWindow.
- Manually verify installed app enables Explorer integration with registry commands pointing to `%LocalAppData%\Programs\Foldora\Foldora.MenuHost.exe`.
- Manually verify published MenuHost desktop-background create near the cursor/menu selection area.
- If placement fails, inspect `%AppData%\Foldora\Logs\menuhost-placement.log` and use the latest JSONL entry for the next debugging step.
- Manually spot-check the polished MainWindow/SettingsWindow/HelpWindow in RU/EN first, then German/Portuguese/Ukrainian long labels and Hindi/Thai/CJK font fallback as feedback-driven layout checks.
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
- WPF catalog expansion is complete for `bg`, `cs`, `de`, `en`, `es`, `fr`, `hi`, `hu`, `id`, `it`, `ja`, `ko`, `nl`, `pl`, `pt-BR`, `pt-PT`, `ro`, `ru`, `th`, `tr`, `uk`, `vi`, `zh-Hans`, `zh-Hant`; CLI defaults/diagnostics/validation output and startup fatal dialog remain tracked debt.
- Future regional locale candidates `be`, `kk`, `uz-Latn`, `az`, `hy`, `ka`, `lt`, `lv`, `et`, `sk`, `sl`, `hr` and `sr` must not be exposed in Settings UI until complete catalogs and tests exist.
- No Explorer restart or icon cache reset flow.

## Future

- MSI/MSIX or winget after stable per-user install layout.
- Modern Windows 11 context menu research.
- Future regional candidate locale catalogs (`be`, `kk`, `uz-Latn`, `az`, `hy`, `ka`, `lt`, `lv`, `et`, `sk`, `sl`, `hr`, `sr`) after enabled catalog quality is reviewed.
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

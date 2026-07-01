# Technical Debt

Этот документ отделяет известные technical debt и accepted limitations от roadmap features. Он не является обещанием реализации: каждый пункт требует отдельного решения перед изменениями в production-коде.

## Active Debt

### TD-UI-0001 Redundant Settings Entry Points

- ID: `TD-UI-0001`
- Title: Redundant settings entry points
- Status: Fixed
- Severity: Low
- Area: WPF / UX
- Observed behavior: MainWindow has a title-bar gear button for Settings and also shows `Manage in Settings` / `Управлять в настройках` in the content area.
- Expected/desired behavior: Settings should have one clear primary entry point, and MainWindow should stay focused on menu editing.
- Known cause or hypothesis: Settings/Explorer cleanup kept a compact management action on the editor surface after moving Explorer controls to SettingsWindow.
- Current workaround: Not needed; the duplicate content-area settings action has been removed.
- Next investigation step: Keep MainWindow editor-first during future visual polish.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/UI_DESIGN.md`
  - `src/Foldora.App/MainWindow.xaml`
- Date added: 2026-07-01

### TD-UI-0002 Developer-Facing Boolean Status

- ID: `TD-UI-0002`
- Title: Developer-facing unsaved changes status
- Status: Fixed
- Severity: Low
- Area: WPF / UX / Localization
- Observed behavior: MainWindow shows `Unsaved changes: True/False` and localized equivalents, exposing a raw boolean state to users.
- Expected/desired behavior: Use user-facing state text such as `All changes saved` / `Unsaved changes` or a localized status chip.
- Known cause or hypothesis: Early presentation state was added as direct debug-friendly text.
- Current workaround: Not needed; MainWindow now shows localized saved/unsaved status strings without raw booleans.
- Next investigation step: Future visual polish may turn the text into a status chip, but the debug-style boolean is gone.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/LOCALIZATION.md`
  - `src/Foldora.App/ViewModels/MainViewModel.cs`
  - `src/Foldora.App/Localization/*.json`
- Date added: 2026-07-01

### TD-UI-0003 Cramped Button Geometry

- ID: `TD-UI-0003`
- Title: Cramped button geometry
- Status: Partially addressed / Monitor
- Severity: Low
- Area: WPF / Design System / Localization
- Observed behavior: Some MainWindow and SettingsWindow buttons look tight; text sits too close to button edges.
- Expected/desired behavior: Buttons should have consistent padding, min-width and min-height that survive long localized labels.
- Known cause or hypothesis: Current reusable button styles are functional but not yet tuned through a localization-heavy visual polish pass.
- Current workaround: Shared action buttons now have larger padding and min-height, dense Settings rows use a compact inline action style with non-tiny horizontal padding, and MainWindow/SettingsWindow have minimum widths that prevent the known broken narrow layouts.
- Next investigation step: Continue long-label visual checks during product-grade polish, especially SettingsWindow actions in long locales.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/UI_DESIGN.md`
  - `src/Foldora.App/Resources/Controls.xaml`
- Date added: 2026-07-01

### TD-UI-0004 Settings Scrollbar And Content Gutter

- ID: `TD-UI-0004`
- Title: Settings scrollbar and content gutter
- Status: Fixed
- Severity: Low
- Area: WPF / Settings UI
- Observed behavior: SettingsWindow scrollbar is visually too close to content.
- Expected/desired behavior: SettingsWindow should have a clearer right-side gutter/content padding and robust spacing when sections scroll.
- Known cause or hypothesis: SettingsWindow grew from a compact language dialog into a multi-section settings window without a dedicated spacing hardening pass.
- Current workaround: Not needed; scroll content now has a right-side gutter.
- Next investigation step: Watch long-locale SettingsWindow layouts during future polish.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/UI_DESIGN.md`
  - `src/Foldora.App/SettingsWindow.xaml`
- Date added: 2026-07-01

### TD-UI-0005 Product-Grade Visual Polish Gap

- ID: `TD-UI-0005`
- Title: Product-grade visual polish gap
- Status: Fixed for visual polish v1 / Monitor
- Severity: Medium
- Area: WPF / UX / Visual Design
- Observed behavior: The app is functional and cleaner after Settings cleanup, but still looks like a careful prototype rather than a polished product UI.
- Expected/desired behavior: Clearer hierarchy, spacing, surfaces, empty states, status presentation, cards and settings sections before public hero/mockup work.
- Known cause or hypothesis: MVP work prioritized functional shell integration, safety and localization over visual polish.
- Current workaround: Not needed for the current MVP windows; visual polish v1 refined MainWindow, SettingsWindow, HelpWindow and shared WPF resources with calmer headers, sections, status chips, empty state, path rows, help step rows and footer presentation.
- Next investigation step: Monitor manual feedback and long-locale layout issues. App icon, branding assets and README hero remain a separate branding pass, not part of this debt item.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/UI_DESIGN.md`
  - `docs/ROADMAP.md`
- Date added: 2026-07-01

### TD-UI-0006 Missing App And Build Icon

- ID: `TD-UI-0006`
- Title: Missing application/build/window icon
- Status: Open
- Severity: Low
- Area: WPF / Branding / Packaging
- Observed behavior: Foldora does not yet have a proper app/build/window icon.
- Expected/desired behavior: App icon, exe icon and window icon should be designed and bundled under the repository resource policy.
- Known cause or hypothesis: Branding/assets work was intentionally deferred until the functional MVP stabilized.
- Current workaround: The app runs without a polished icon.
- Next investigation step: Separate branding/assets pass, including license/resource review and README hero/mockup planning.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/RESOURCE_POLICY.md`
  - `THIRD_PARTY_NOTICES.md`
- Date added: 2026-07-01

### TD-UI-0007 Help / About / Instructions Window

- ID: `TD-UI-0007`
- Title: Help, about and instructions window
- Status: Partially fixed / content polish remains
- Severity: Low
- Area: WPF / UX / Docs
- Observed behavior: Settings now has targeted tooltips and a small Help/About window explaining the basic workflow, Explorer menu behavior, `Foldora.MenuHost.exe`, data paths, uninstall and license notes.
- Expected/desired behavior: Future polish should improve help copy, add screenshots only after UI visual polish, and review long help translations across all enabled locales.
- Known cause or hypothesis: MVP prioritized functional editor and shell integration before broader onboarding/help content.
- Current workaround: Use the Settings Help/About window, wrapped tooltips, README and docs. Visual polish v1 improved the HelpWindow header, section rhythm and step list readability.
- Next investigation step: Translation/content review and possible richer Help/About content or screenshots after branding/public-presentation work.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/UX_FLOW.md`
  - `docs/SETTINGS.md`
  - `src/Foldora.App/HelpWindow.xaml`
- Date added: 2026-07-01

### TD-0004 Remaining Localization Debt

- ID: `TD-0004`
- Title: Remaining localization debt
- Status: Open
- Severity: Medium
- Area: WPF / CLI / Core / Docs
- Observed behavior: WPF labels/status/defaults, default menu title mode and WPF validation rendering now use embedded complete catalogs for `bg`, `cs`, `de`, `en`, `es`, `fr`, `hi`, `hu`, `id`, `it`, `ja`, `ko`, `nl`, `pl`, `pt-BR`, `pt-PT`, `ro`, `ru`, `th`, `tr`, `uk`, `vi`, `zh-Hans` and `zh-Hant`, but some user-facing strings still live outside the localization layer.
- Expected/desired behavior: UI, runtime/status, validation and diagnostic messages should be localized consistently, while saved user data remains unchanged when language changes.
- Known cause or hypothesis: Early MVP started with Russian hardcoded defaults in Core and diagnostic CLI output. WPF validation now maps Core invariant issue codes/parameters to localized catalog strings; CLI output and startup fatal dialog still need a policy.
- Current workaround: WPF passes localized defaults for new draft entries, tracks default/custom menu title separately, and persists first-run language detection before loading draft state. Existing Core fallbacks remain compatibility/CLI paths and are documented in `docs/LOCALIZATION.md`.
- Manual check note: Ukrainian, Japanese and German WPF UI were spot-checked after locale expansion with no blocking layout issue. RU/EN remain primary verified locales; other enabled locales are catalog-complete and test-covered, with translation/layout polish left for feedback-driven future work.
- Visual polish note: visual polish v1 did not add localization keys or manually verify every enabled locale. RU/EN remain primary manually verified; the wider locale set remains catalog-complete and smoke/spot-check based.
- Next investigation step: Decide CLI localization policy, replace startup fatal dialog literals, review remaining technical plan/details strings, spot-check long UI labels after visual polish, and consider external translator review for enabled catalogs before a stable public release.
- Links to docs/tests/code:
  - `docs/LOCALIZATION.md`
  - `src/Foldora.App/Localization/*.json`
  - `src/Foldora.Core/Menu/FolderMenuSettings.cs`
  - `src/Foldora.Core/Menu/FolderMenuNameGenerator.cs`
  - `src/Foldora.Core/Validation/FolderNameValidator.cs`
  - `src/Foldora.Shell/RegistryPlan/ExplorerMenuRegistryPlanBuilder.cs`
  - `src/Foldora.Cli/DesktopIniPolicyDiagnosticsRunner.cs`
  - `src/Foldora.App/App.xaml.cs`
- Date added: 2026-07-01

### TD-0001 Desktop Icon Placement From Legacy Menu

- ID: `TD-0001`
- Title: Desktop icon placement from legacy menu
- Status: Partially mitigated / Monitor
- Severity: High
- Area: Shell
- Observed behavior: Foldora legacy menu can be invoked from desktop background, but the created folder icon is placed by Explorer in a free desktop icon-view slot, not under the cursor position where the menu was opened.
- Expected/desired behavior: If technically feasible in a future integration model, creating from desktop background should place the new folder near the invocation point.
- Known cause or hypothesis:
  - `TD-0001A`: current HKCU legacy context menu commands receive target directory path through Explorer placeholders such as `%V`, but they do not receive original right-click cursor coordinates or desktop icon-view coordinates.
  - `TD-0001B`: post-create desktop icon positioning is feasible; manual diagnostic checks confirmed that both screen and view coordinates can move existing desktop icons, with Explorer grid displacement accepted as acceptable.
- Current workaround/mitigation: `Foldora.MenuHost` now captures current cursor screen position before `create`, creates the folder through Core, and best-effort repositions the created desktop item if the target directory is the current user's Desktop directory. Placement writes local JSONL diagnostics to `%AppData%\Foldora\Logs\menuhost-placement.log` and retries boundedly only when Explorer reports that the desktop item is not found yet. User can still move the icon manually if Explorer snaps/shifts it or positioning fails.
- Next investigation step: Manual publish smoke for best-effort placement under Explorer legacy menu. If placement still fails, inspect the latest `menuhost-placement.log` entry. Exact original right-click placement still requires a separate coordinate source or heavier shell integration.
- Links to docs/tests/code:
  - `docs/SHELL_INTEGRATION.md`
  - `docs/research/DESKTOP_ICON_PLACEMENT.md`
  - `docs/SMOKE_TEST.md`
  - `src/Foldora.Shell/Desktop/WindowsDesktopIconPositioningService.cs`
  - `src/Foldora.MenuHost/DesktopPlacementCoordinator.cs`
  - `src/Foldora.MenuHost/MenuHostPlacementLogWriter.cs`
  - `src/Foldora.MenuHost/CursorPosition.cs`
  - `src/Foldora.Cli/DesktopIconPositionDiagnosticsRunner.cs`
  - `src/Foldora.Shell/RegistryPlan/ExplorerMenuShellTargetPlaceholder.cs`
  - `src/Foldora.Shell/RegistryPlan/ExplorerMenuCommandBuilder.cs`
  - `tests/Foldora.Tests/Shell/ExplorerMenuRegistryPlanBuilderTests.cs`
- Date added: 2026-07-01

### TD-0002 First Created Desktop Folder May Initially Show Default Icon

- ID: `TD-0002`
- Title: First created desktop folder may initially show default icon
- Status: Cannot reproduce / Monitor
- Severity: Low
- Area: Desktop.ini
- Observed behavior: Earlier manual Windows 11 publish testing saw the first folder created from the published Foldora legacy menu after registration/system start initially appear with the default folder icon. A retry or subsequent created folder could show the custom icon correctly. Current manual checks no longer reproduce this: new folders appear with the selected custom icon immediately.
- Expected/desired behavior: A newly created folder should show the selected custom icon consistently after Foldora writes `desktop.ini` and applies the deletion-friendly attributes.
- Known cause or hypothesis: Explorer desktop view may observe the folder before Foldora finishes writing `desktop.ini`, setting `IconResource`, and applying folder/`desktop.ini` attributes; alternatively Explorer icon cache or desktop view notification timing may be insufficient.
- Current workaround: If it reappears, refresh Explorer/Desktop or reopen the folder view. Do not add sleeps or refresh code without a fresh reproduction.
- Next investigation step:
  - Monitor future publish/manual smoke runs.
  - If reproduced again, confirm exact reproduction conditions after publish registration.
  - Compare behavior in normal folder directory vs Desktop background.
  - Inspect operation timing around `Directory.CreateDirectory`, `desktop.ini` write and attribute application.
  - Consider a small testable Shell refresh notification abstraction after create/apply if evidence supports it.
  - Avoid production sleeps unless investigation proves no better option.
- Links to docs/tests/code:
  - `docs/DESKTOP_INI.md`
  - `docs/SHELL_INTEGRATION.md`
  - `docs/SMOKE_TEST.md`
  - `src/Foldora.Core/Menu/FolderMenuEntryActionService.cs`
  - `src/Foldora.Core/DesktopIni/DesktopIniService.cs`
  - `src/Foldora.MenuHost/MenuHostCommandRunner.cs`
  - `tests/Foldora.Tests/Core/DesktopIniServiceTests.cs`
  - `tests/Foldora.Tests/Menu/FolderMenuEntryActionServiceTests.cs`
  - `tests/Foldora.Tests/MenuHost/MenuHostCommandRunnerTests.cs`
- Date added: 2026-07-01

### TD-0003 Self-Contained Icons For Created Folders

- ID: `TD-0003`
- Title: Self-contained icons for created folders
- Status: Open / Future consideration
- Severity: Low
- Area: Desktop.ini / Packaging
- Observed behavior: Foldora-created folders usually reference imported `.ico` files under `%AppData%\Foldora\icons` from `desktop.ini`.
- Expected/desired behavior: If users need fully portable/stable styled folders, Foldora could optionally copy the selected `.ico` into each target folder and reference that local copy.
- Known cause or hypothesis: Current MVP intentionally centralizes imported icons in user data to avoid duplicating files and to keep menu entry metadata simple.
- Current workaround: Default uninstall preserves `%AppData%\Foldora`, so existing styled folders keep access to imported icons.
- Next investigation step: Only design a self-contained folder-icon mode if real uninstall/portability needs justify extra file management and cleanup behavior.
- Links to docs/tests/code:
  - `docs/DESKTOP_INI.md`
  - `docs/SETTINGS.md`
  - `src/Foldora.Core/DesktopIni/DesktopIniService.cs`
  - `src/Foldora.Core/Menu/IconImportService.cs`
- Date added: 2026-07-01

## Do Not Do

- Не обещать создание папки под курсором в текущем MVP.
- Не добавлять COM shell extension без отдельного явного этапа.
- Не делать modern Windows 11 context menu в этом debt pass.
- Не патчить Explorer или системные DLL.
- Не добавлять random sleep как production fix без investigation.
- Не ломать deletion-friendly desktop.ini policy `ReadOnlyFolderHiddenDesktopIni` (`folder: ReadOnly`, `desktop.ini: Hidden`).
- Не менять Core/JSON model ради desktop icon placement или icon refresh timing.
- Не удалять `%AppData%\Foldora` по default при uninstall, потому что existing styled folders могут ссылаться на imported icons.
- Не использовать UIAutomation как acceptance criterion для этих пунктов.

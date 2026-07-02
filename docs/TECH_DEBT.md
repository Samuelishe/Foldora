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
- Current workaround: Not needed; MainWindow now shows localized saved/unsaved status strings inside reusable status chip presentation without raw booleans.
- Next investigation step: Monitor whether saved/unsaved status wording needs more severity-specific styling after manual locale checks.
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
- Current workaround: Shared action buttons now have larger padding and min-height, the base button template applies padding/alignment/foreground forwarding, dense Settings rows use a compact inline action style with non-tiny horizontal padding, Installation path buttons use short `Open`/`Copy` labels, Settings Explorer actions use short contextual Enable/Disable labels, SettingsWindow action rows wrap or use star/auto layout, MainWindow/SettingsWindow have minimum widths that prevent the known broken narrow layouts, and v2 button colors/states are centralized in `Controls.xaml`.
- Next investigation step: Continue long-label visual checks after product-grade polish, especially SettingsWindow actions in long locales and non-Latin font fallback.
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
- Status: Fixed for Visual Design Direction v2 / Monitor
- Severity: Medium
- Area: WPF / UX / Visual Design
- Observed behavior: The app is functional and cleaner after Settings cleanup, but still looks like a careful prototype rather than a polished product UI.
- Expected/desired behavior: Clearer hierarchy, spacing, surfaces, empty states, status presentation, cards and settings sections before public hero/mockup work.
- Known cause or hypothesis: MVP work prioritized functional shell integration, safety and localization over visual polish.
- Current workaround: Not needed for the current MVP windows; Visual Design Direction v2 refined MainWindow, SettingsWindow, HelpWindow and shared WPF resources with an icon-inspired light palette, stronger shared surfaces, segmented Settings tabs, status chip variants, a self-authored XAML empty-state mark, polished path/help rows and cleaner footer/status presentation.
- Next investigation step: Monitor manual RU/EN and long-locale feedback. README hero and broader branding assets remain a separate branding/pass-public-presentation step, not part of this debt item; app/window/exe icon foundation is tracked under `TD-UI-0006`.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/UI_DESIGN.md`
  - `docs/ROADMAP.md`
- Date added: 2026-07-01

### TD-UI-0006 Missing App And Build Icon

- ID: `TD-UI-0006`
- Title: Missing application/build/window icon
- Status: Fixed for folded app icon foundation / Monitor
- Severity: Low
- Area: WPF / Branding / Packaging
- Observed behavior: Foldora previously did not have a proper app/build/window icon.
- Expected/desired behavior: App icon, exe icon and window icon should be designed and bundled under the repository resource policy.
- Known cause or hypothesis: Branding/assets work was intentionally deferred until the functional MVP stabilized.
- Current workaround: Not needed for the WPF app foundation; `Foldora.App.exe`, MainWindow, SettingsWindow and HelpWindow now use a self-authored folded blue/cyan Foldora icon with a broad light-cyan folded plane, SVG source and generated multi-size ICO. The first folder/menu-badge concept and the later too-thin folded-ribbon attempt were replaced after visual review.
- Next investigation step: Monitor small-size readability and later plan README hero/mockup or broader branding polish. MenuHost/CLI executable icons are intentionally lower priority because MenuHost is no-console/invisible and CLI is developer-facing.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/RESOURCE_POLICY.md`
  - `THIRD_PARTY_NOTICES.md`
  - `src/Foldora.App/Assets/FoldoraIcon.svg`
  - `src/Foldora.App/Assets/Foldora.ico`
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
- Current workaround: Use the Settings Help/About window, wrapped tooltips, README and docs. Visual Design Direction v2 keeps the same Help/About content semantics while improving the shared header, section, step and fixed-footer presentation.
- Next investigation step: Translation/content review and possible richer Help/About content or screenshots after branding/public-presentation work.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/UX_FLOW.md`
  - `docs/SETTINGS.md`
  - `src/Foldora.App/HelpWindow.xaml`
- Date added: 2026-07-01

### TD-UI-0008 SettingsWindow Long Document Layout

- ID: `TD-UI-0008`
- Title: SettingsWindow long document layout
- Status: Fixed
- Severity: Low
- Area: WPF / Settings UI / UX
- Observed behavior: SettingsWindow stacked Application, Explorer menu, Installation, Help/About and Danger zone as one long scroll document after visual polish v1.
- Expected/desired behavior: Settings categories should be compact, navigable and scalable without making reset/danger controls visible as default content.
- Known cause or hypothesis: Settings grew incrementally from a small language dialog into a multi-section system window.
- Current workaround: Not needed; SettingsWindow now uses category tabs, keeps footer actions fixed and isolates Danger zone in its own tab.
- Next investigation step: Manual RU/EN and long-locale checks for tab header fit, path row wrapping and Explorer technical details overflow. Button clipping found after the first tabbed pass was addressed by the Settings layout robustness pass; tab header clipping found afterwards was addressed by using content-sized wrapping tab headers. Runtime tab-body centering found after the header fix was addressed by separating tab header centering from `TabItem` selected-content alignment. Settings responsive/action polish later widened the practical window size and shortened contextual Explorer actions; dynamic `SizeToContent` remains intentionally unused to avoid jumpy modal tab switching.
- Links to docs/tests/code:
  - `docs/UI_AUDIT.md`
  - `docs/UI_DESIGN.md`
  - `docs/SETTINGS.md`
  - `src/Foldora.App/SettingsWindow.xaml`
  - `src/Foldora.App/Resources/Controls.xaml`
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

## Deferred / Research Items

### TD-IMG-0001 SVG Input Support

- ID: `TD-IMG-0001`
- Title: SVG input support for icon conversion
- Status: Deferred / Research
- Severity: Low
- Area: Imaging / UX / Resource Policy
- Observed behavior: Planned image-to-ICO conversion should initially support `.png`, `.jpg`, `.jpeg` and `.bmp`, but not SVG.
- Expected/desired behavior: If SVG import becomes important, Foldora should support it without pretending WPF is a complete SVG renderer or accepting unknown-license conversion dependencies.
- Known cause or hypothesis: True SVG rendering without third-party libraries is non-trivial. WPF is not a full SVG renderer, and implementing full SVG manually is too large for the first conversion milestone.
- Current workaround: Treat SVG as phase 2/research. Users can export SVG to PNG externally before using the planned converter.
- Next investigation step: Evaluate limited SVG subset, optional dependency after license review, or an explicitly accepted external conversion pipeline.
- Links to docs/tests/code:
  - `docs/ICON_CONVERSION_ROADMAP.md`
  - `docs/RESOURCE_POLICY.md`
- Date added: 2026-07-02

### TD-IMG-0002 Orphan Imported/Generated Icon Cleanup

- ID: `TD-IMG-0002`
- Title: Orphan imported/generated icon cleanup
- Status: Deferred
- Severity: Low
- Area: Settings / Storage / Imaging
- Observed behavior: Current workflow can leave imported `.ico` files under `%AppData%\Foldora\icons` after entry deletion/replacement. IC4a/IC4b can also leave generated `.ico` files under `%AppData%\Foldora\icons\generated` when the user replaces an icon through picker/drop, deletes an entry or discards a staged edit after image conversion.
- Expected/desired behavior: Later, Foldora should be able to identify and clean unused imported/generated icons without deleting icons referenced by existing styled folders or packs.
- Known cause or hypothesis: Orphan risk increased with picker and preview-drop auto-conversion because Foldora now creates generated icon files automatically. Pack import/export will add more cases later.
- Current workaround: Preserve `%AppData%\Foldora` and avoid automatic cleanup.
- Next investigation step: Revisit after converter/batch work or pack import/export. Cleanup must respect settings, existing styled folders and future packs, and should keep generated storage separate from user-imported icons.
- Links to docs/tests/code:
  - `docs/ICON_CONVERSION_ROADMAP.md`
  - `docs/SETTINGS.md`
- Date added: 2026-07-02

### TD-IMG-0003 Gamma-Correct Resize Research

- ID: `TD-IMG-0003`
- Title: Gamma-correct resize research
- Status: Deferred / Research
- Severity: Low
- Area: Imaging / Conversion Quality
- Observed behavior: IC2b implements alpha-aware Lanczos-style resizing in byte/sRGB value space.
- Expected/desired behavior: A future converter quality pass could evaluate gamma-correct or linear-light resizing if visual evidence shows color shifts in generated icons.
- Known cause or hypothesis: Correct color-space handling adds complexity and requires a clear policy for source profiles, alpha and deterministic output. The MVP converter foundation currently needs robust alpha behavior and good downscale quality more than full color management.
- Current workaround: Use premultiplied-alpha Lanczos3 resizing, resize every ICO target frame from the original source image and avoid chained resizing.
- Next investigation step: Revisit after the full converter pipeline exists and can be visually compared on real icon samples.
- Links to docs/tests/code:
  - `docs/ICON_CONVERSION_ROADMAP.md`
  - `src/Foldora.Imaging/RgbaImageResizer.cs`
  - `tests/Foldora.Tests/Imaging/RgbaImageResizerTests.cs`
- Date added: 2026-07-02

### TD-OPS-0001 Diagnostics And Repair Center

- ID: `TD-OPS-0001`
- Title: Diagnostics and repair center
- Status: Deferred
- Severity: Medium
- Area: Settings / Shell / Storage / Supportability
- Observed behavior: Foldora has startup diagnostics and MenuHost placement logs, but no user-facing repair surface for broken install paths, registry mismatch, missing icons or corrupt settings.
- Expected/desired behavior: Future Settings -> Diagnostics / Repair can check integration state, host path validity, settings parse state, missing icons, stale registry roots, missing AppData icons referenced by `desktop.ini`, and recent MenuHost failures.
- Known cause or hypothesis: MVP prioritized editor/shell integration and visual polish over support tooling.
- Current workaround: Manual docs, CLI commands and log inspection.
- Next investigation step: Design actions such as Check integration, Repair Explorer menu, Disable stale menu, Validate icons, Open logs folder and Open settings file location. Do this after core MVP feature work, not before image conversion.
- Links to docs/tests/code:
  - `docs/ICON_CONVERSION_ROADMAP.md`
  - `docs/SHELL_INTEGRATION.md`
  - `docs/SETTINGS.md`
- Date added: 2026-07-02

### TD-UX-0009 Drag-And-Drop Ordering

- ID: `TD-UX-0009`
- Title: Cross-group and group/block drag-and-drop ordering
- Status: Partially addressed
- Severity: Low
- Area: WPF / UX / Menu Model
- Observed behavior: IC5a supports drag-handle reorder for entries within the current group and normalizes `SortOrder` in the staged draft. Cross-group entry moves and group/block ordering are not implemented.
- Expected/desired behavior: Users should be able to move entries between groups and reorder groups while preserving staged Save/Discard behavior.
- Known cause or hypothesis: Ordering UX was intentionally deferred until the editor, grouping and icon selection basics were stable.
- Current workaround: Reorder inside a group with the drag handle; use the editable `GroupName` field or group rename/delete flow for group-level changes.
- Next investigation step: Decide whether cross-group moves should remain flat `GroupName` updates or require first-class group ordering semantics. Avoid treating current group rename/delete as missing; future group polish is reorder/collapse/duplicate/context-menu work.
- Links to docs/tests/code:
  - `docs/ICON_CONVERSION_ROADMAP.md`
  - `docs/UX_FLOW.md`
  - `docs/MENU_MODEL.md`
- Date added: 2026-07-02

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

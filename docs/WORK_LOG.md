# Work Log

## 2026-07-02 - IC3 CLI convert-icon command

- Retargeted `Foldora.Cli` to `net10.0-windows` and allowed it to reference `Foldora.Imaging.Windows` for Windows/WPF imaging conversion.
- Added `convert-icon --input "<image>" --output "<icon.ico>" [--force]` for single-file PNG/JPG/JPEG/BMP to multi-size ICO conversion.
- Reused `WindowsImageToIconConverter` so CLI output uses standard `16/24/32/48/64/128/256` frames and contain-fit transparent square policy.
- Added temp-file safe write, overwrite protection without `--force`, parser/help updates and CLI runner tests for success/error paths.
- No WPF picker integration, drag/drop, converter window, batch/recursive conversion, AppData generated icon storage integration or SVG support was implemented.

## 2026-07-02 - IC2c image-to-ICO conversion foundation

- Added `WindowsImageToIconConverter` in `src/Foldora.Imaging.Windows` as the Windows-specific stream-based PNG/JPG/BMP-to-ICO conversion service.
- The service composes `WindowsImageDecoder`, pure `RgbaImageResizer`, `WindowsPngFrameEncoder` and `IcoWriter`.
- Default conversion writes 16/24/32/48/64/128/256 px PNG-compressed ICO frames; custom target sizes are validated, unique and sorted.
- Non-square source images use contain-fit into transparent square frames, preserving aspect ratio and avoiding cropping.
- Added tests for standard/custom ICO output, PNG/JPG/BMP streams, stream validation/ownership, result reporting, tiny sources, alpha survival and non-square contain-fit behavior.
- No CLI `convert-icon`, WPF picker integration, drag/drop, converter window, AppData generated icon storage integration or SVG support was implemented.

## 2026-07-02 - IC2b imaging resize/downscale foundation

- Added pure `RgbaImageResizer` in `src/Foldora.Imaging` with Lanczos3-style separable resizing from `RgbaImage` to `RgbaImage`.
- Added `ImageResizeOptions` and `ImageResizeFilter` as the small public resize contract for future converter stages.
- Implemented premultiplied-alpha filtering and unpremultiply output so transparent RGB pixels do not create color halos.
- Added resize tests for validation, requested dimensions, opaque alpha, constant-color normalization, transparent RGB behavior, semi-transparent alpha, determinism, 1px/non-square edge cases, source immutability and PNG/ICO compatibility.
- No CLI `convert-icon`, WPF picker integration, drag/drop, converter window, full conversion pipeline, SVG support or decode/encode bridge changes were implemented.

## 2026-07-02 - IC2a imaging decode/encode foundation

- Kept `src/Foldora.Imaging` as a pure `net10.0` library and added `RgbaImage` as a tightly packed RGBA buffer model for future converter stages.
- Added `src/Foldora.Imaging.Windows` as a `net10.0-windows` project using WPF imaging APIs and referencing only `Foldora.Imaging`.
- Implemented PNG/JPG/JPEG/BMP stream decoding to `RgbaImage` and PNG frame payload encoding from `RgbaImage`.
- Added tests for RGBA model validation, PNG/JPG/BMP decoding, PNG encode round-trip, alpha/opaque behavior, stream ownership and feeding encoded PNG payloads into the IC1 `IcoWriter`.
- No resize/downscale, full converter service, CLI `convert-icon`, WPF picker integration, drag/drop, converter window or SVG support was implemented.

## 2026-07-02 - IC1 imaging foundation

- Added `src/Foldora.Imaging` as a clean `net10.0` class library for future image-to-ICO conversion foundation.
- Added standard square icon frame-size models, minimal conversion options/result/error models and `IcoWriter`.
- `IcoWriter` writes deterministic little-endian ICO containers from already encoded PNG frame payloads, sorting frames by size and validating empty/duplicate inputs.
- Added binary structure tests under `tests/Foldora.Tests/Imaging` for ICONDIR, ICONDIRENTRY, 256x256 byte encoding, offsets, payload concatenation, validation and caller-owned stream behavior.
- No PNG/JPG/BMP decoding, resizing, WPF picker integration, drag-and-drop, CLI `convert-icon`, converter window, SVG, pack import/export or diagnostics/repair UI was implemented.

## 2026-07-02 - Future feature planning docs

- Added `docs/ICON_CONVERSION_ROADMAP.md` as the central planning note for image-to-ICO conversion.
- Captured the next priority order: conversion foundation, WPF picker auto-conversion, drag image onto preview, converter/batch UI, drag-and-drop ordering, pack import/export, diagnostics/repair and later release/install polish.
- Documented the proposed future `Foldora.Imaging` layer, multi-size ICO output rules, planned `convert-icon` CLI shape, WPF picker/drop flows, generated icon storage considerations, pack import/export concept, orphan cleanup timing and diagnostics/repair scenarios.
- Kept SVG support as a separate research topic after PNG/JPG/BMP conversion.
- No production code, WPF/XAML, CLI implementation, csproj, tests, image generation or asset changes were made.

## 2026-07-02 - README hero image

- Added the maintainer-provided README hero/mockup image to the public GitHub README.
- Renamed `docs/assets/readme/foldora-readme-hero.png.png` to canonical `docs/assets/readme/foldora-readme-hero.png`.
- Replaced the visible README hero/screenshot placeholder with a centered GitHub-friendly image block after the product pitch.
- Updated resource policy and third-party notices to clarify that the README hero is a documentation/presentation mockup, not a runtime app asset or reusable third-party icon pack.
- No production code, WPF UI, app icon, project files, install scripts, registry/MenuHost behavior or external downloads changed.

## 2026-07-02 - Settings responsive/action polish

- Widened SettingsWindow to a practical fixed/resizable desktop size (`Width=940`, `MinWidth=920`) while keeping `SizeToContent=Manual`.
- Kept dynamic WPF content sizing out of scope because tab-driven auto-size would make the modal SettingsWindow jump and conflict with the fixed footer plus tab-local scrolling model.
- Shortened Settings Explorer action labels across all enabled catalogs: `Enable` / `Disable`, `Включить` / `Выключить`, and matching short labels for the other enabled locales.
- Shortened the Settings Help/About tab label to short Help/Справка-style labels while leaving the Help/About section and HelpWindow content intact.
- Fixed primary button contrast by forwarding `Foreground` through the shared button template and keeping `OnAccentBrush` in normal/hover/pressed primary states.
- Added/updated design and localization tests for Settings sizing, short labels, primary button foreground and Explorer action tooltip coverage.
- Application tab content/layout, ViewModel state, settings JSON, registry/MenuHost/install/shell behavior, app icon, README hero and external assets were not changed.

## 2026-07-02 - Visual Design Direction v2

- Applied a controlled product-grade WPF visual polish pass to MainWindow, SettingsWindow, HelpWindow and shared design resources without changing business behavior.
- Updated `DesignTokens.xaml`, `Typography.xaml` and `Controls.xaml` with a soft cool page background, icon-inspired blue/cyan/violet accent system, gradient primary button, calm danger button, rounded TextBox template, status chip variants, path/chip text styles, polished Settings tabs/path rows and stronger empty/help surfaces.
- MainWindow kept its editor-first flow and gained a self-authored XAML empty-state folder/menu mark, stronger status chip treatment and a conditional status banner that only appears for real status/errors.
- SettingsWindow kept the tabbed layout and existing bindings while Application/Help/About sections, Explorer status, Installation path rows, technical details and footer use the v2 shared styles.
- HelpWindow kept its localized content semantics and now shares the v2 header/section/step/footer rhythm.
- Updated lightweight design resource/XAML tests for the new style keys and presentation contracts.
- No localization catalogs, ViewModel state, settings JSON, Core model, registry/MenuHost/install/shell behavior, app icon, README hero or external assets were changed.

## 2026-07-01 - GitHub README product landing pass

- Reworked the top of `README.md` from an engineering-first status document into a product-oriented GitHub landing flow.
- Added a concise pitch, visible hero/screenshot placeholder, highlights, quick start, example menu and screenshots placeholder without adding image assets.
- Reordered the lower README sections so user-facing capabilities, requirements, install/uninstall and development details come before limitations, safety, license and documentation links.
- Kept MVP limitations explicit below the onboarding flow: no MSI/MSIX, no modern Windows 11 compact context menu, no PNG-to-ICO conversion, no pack import/export and best-effort desktop placement only.
- No production code, screenshots, generated hero image, installer behavior, shell behavior, localization catalogs or external assets were changed.

## 2026-07-01 - App icon folded ribbon refinement

- Replaced the visually rejected app icon implementation while keeping the same production asset paths: `src/Foldora.App/Assets/FoldoraIcon.svg` and `src/Foldora.App/Assets/Foldora.ico`.
- Redrew the left folded/origami ribbon as a broad light-cyan folded plane instead of the thin diagonal hook/needle shape from the previous replacement.
- Kept the dark rounded app tile, blue/cyan/violet folder direction, generated 16/32/48/256 px ICO frames and existing WPF/exe wiring.
- Generated manual review previews under `artifacts/icon-preview/` for 256/48/32/16 px checks.
- Used the provided reference image only as visual direction; no reference board image, raster crop, external asset, icon pack or third-party resource was added.
- README hero/mockup, full branding system, registry/MenuHost/install/shell behavior, settings JSON and localization catalogs were not changed.

## 2026-07-01 - App icon replacement pass

- Replaced the first self-authored folder/menu-badge icon concept with a folded blue/cyan folder mark on a dark rounded app tile.
- Kept the same production asset paths: `src/Foldora.App/Assets/FoldoraIcon.svg` and `src/Foldora.App/Assets/Foldora.ico`.
- Regenerated the ICO with 16/32/48/256 px frames and kept existing WPF/exe wiring through `ApplicationIcon` and shared window `Icon` attributes.
- Updated the design resource test to assert the source concept text and reject the old small menu badge concept.
- Used the provided image only as visual direction; no reference board image, external asset, icon pack or third-party resource was added.
- README hero/mockup, full branding system, registry/MenuHost/install/shell behavior, settings JSON and localization catalogs were not changed.

## 2026-07-01 - Branding/App icon foundation

- Added the first self-authored Foldora app icon concept.
- Added `src/Foldora.App/Assets/FoldoraIcon.svg` as the source vector and generated `src/Foldora.App/Assets/Foldora.ico` with 16/32/48/256 px Windows icon frames.
- Wired `Foldora.App.csproj` `ApplicationIcon` and the WPF `MainWindow`, `SettingsWindow` and `HelpWindow` `Icon` attributes to the shared app icon.
- Added design/resource tests for app icon project wiring, icon file presence, core ICO sizes and window icon references.
- Updated resource policy/docs to record the icon as a self-authored 0BSD project asset; no external assets or icon packs were used.
- README hero/mockup, full branding system, registry/MenuHost/install/shell behavior, settings JSON and localization catalogs were not changed.

## 2026-07-01 - Runtime Settings tab content centering fix

- Rechecked the ineffective Settings tab content alignment fix after the fresh Release binary still showed Application, Help/About and Danger zone as centered islands.
- Found the shared runtime parent: `SettingsTabItemStyle` used `HorizontalContentAlignment` / `VerticalContentAlignment` to center tab headers, but WPF also uses those properties in the selected tab content path.
- Changed `SettingsTabItemStyle` so selected tab content stretches, while the header `ContentPresenter` remains explicitly centered inside the tab item template.
- Kept the wrapping content-sized tab header host and unchanged localized tab labels.
- Updated design resource tests to cover the shared selected-content host/template path instead of only inner tab body attributes.
- No ViewModel state, localization catalogs, settings JSON, registry/MenuHost/install behavior, shell behavior, app icon, README hero or external assets changed.

## 2026-07-01 - Settings tab content alignment follow-up

- Kept the fixed wrapping Settings tab headers and unchanged localized tab labels.
- Fixed accidental centered tab-body layout by making Settings tab content roots stretch and aligning inner forms/cards to the left/top content margin.
- Kept the Danger zone warning card constrained for readability while aligning it to the left instead of centering it.
- Added a design/XAML test for Settings tab body stretch and left/top content alignment.
- No ViewModel state, localization catalogs, settings JSON, registry/MenuHost/install behavior, shell behavior, app icon, README hero or external assets changed.

## 2026-07-01 - Settings tab header clipping fix

- Fixed a separate Settings tab header clipping issue found by manual screenshot after the button layout robustness pass.
- Replaced the custom Settings `TabControl` header host with a wrapping content-sized host so tab headers do not get squeezed by WPF `TabPanel` behavior.
- Tuned Settings tab header padding/margins and content alignment without fixed tab widths or label changes.
- Added design resource tests for wrapping Settings tab headers and non-clipping TabItem header style.
- Did not change tab labels, SettingsWindow minimum width, ViewModel state, localization catalogs, settings JSON, registry/MenuHost/install behavior or shell behavior.

## 2026-07-01 - Settings layout robustness pass

- Fixed the shared WPF button template so button `Padding` and content alignment are actually applied by the control template.
- Increased normal and inline action button geometry slightly without adding fixed widths, so long localized labels measure to their content instead of clipping.
- Raised SettingsWindow minimum/default width to protect tab headers and Explorer/path action rows from broken narrow layouts while keeping the window resizable.
- Kept Explorer actions in a wrapping row and Installation path rows as star-sized path content plus auto-sized action buttons.
- Added design resource/XAML tests for the button template contract, Settings minimum width, wrapping Explorer actions and path row layout.
- No ViewModel behavior, localization catalogs, settings JSON, registry, MenuHost, install/uninstall scripts, shell behavior, external assets or app icon work changed.

## 2026-07-01 - SettingsWindow tabbed layout cleanup

- Replaced the long single-scroll SettingsWindow document with category tabs: Application, Explorer menu, Installation, Help/About and Danger zone.
- Kept Settings footer actions fixed and left selected tab as WPF UI-only state; nothing is persisted to settings JSON.
- Made Installation path rows more compact by using short visible `Open` / `Copy` actions; command-host open keeps the same behavior and explains opening the containing folder through tooltip text.
- Added reusable Settings tab styles/tokens and localized tab/tooltip keys across all enabled catalogs.
- Updated design/localization tests and docs for the tabbed Settings layout.
- Registry, MenuHost, install/uninstall scripts, shell behavior, Core menu model, CLI localization, app icon, README hero and external assets were not changed.

## 2026-07-01 - Visual polish pass v1

- Refined the current WPF surfaces without changing behavior: MainWindow now uses a calmer page header, compact status chip, clearer menu editor sections, polished empty state and a framed save/discard footer.
- SettingsWindow sections now share the same visual rhythm, Explorer menu status is presented as a compact chip, installation/path rows use reusable row containers, and the danger zone keeps a softer warning boundary.
- HelpWindow now uses the Foldora page header treatment, section rhythm and readable boxed step rows while keeping the existing localized help content.
- Added reusable WPF styles/tokens for page headers, status chips, empty states, path rows, help steps, footer bars and softer danger/accent surfaces.
- Updated lightweight design resource tests for the new style keys and XAML usage.
- No registry, MenuHost, install/uninstall, settings JSON, Core menu model, localization catalogs, app icon, window icon or README hero behavior changed.
- App icon, branding assets and README hero remain a future branding pass; further visual polish should be feedback-driven, with RU/EN remaining the primary manually verified locales and other enabled catalogs covered by catalog completeness plus spot/smoke checks.

## 2026-07-01 - Help/About/Instructions window foundation

- Added a small Settings entry point for Help/About instead of adding another MainWindow title-bar/action entry.
- Added `HelpWindow` with resizable WPF window, scrollable content and fixed Close footer, using existing design resources.
- Added `IHelpDialogService` / `WindowHelpDialogService` and `HelpWindowViewModel` so SettingsViewModel opens help through an App-layer service and code-behind remains window plumbing only.
- Help/About content explains what Foldora does, basic entry/.ico workflow, Windows 11 legacy menu location, HKCU legacy context menu scope, MenuHost role, installed/user-data paths, uninstall behavior and 0BSD/resource policy notes.
- Added Help/About localization keys to all enabled catalogs and tests for Settings help command/XAML/localization completeness.
- Documented that this is a foundation, not a full help center; screenshots, richer onboarding and long-form translation review remain future polish.

## 2026-07-01 - Settings help/layout regression fix

- Replaced Settings `?` help controls with passive self-authored glyphs so hover-only help no longer looks like a broken clickable button.
- Added wrapped help tooltip text styling so long Explorer/registry/path help does not render as one wide line.
- Added compact inline action button geometry for Settings Explorer actions and path Open/Copy buttons, preserving the wider normal action buttons elsewhere.
- Increased inline Settings button horizontal padding after manual inspection still found cramped labels.
- Raised MainWindow and SettingsWindow minimum widths to prevent known broken narrow layouts while keeping resizing and vertical scrolling.
- Updated UI audit/design/UX docs to track the regression fix while keeping the full Help/About window and product-grade polish as future work.

## 2026-07-01 - Settings clarity/help/path actions cleanup

- Reworded Explorer menu status in Settings/MainWindow from generic status text to explicit `Foldora Explorer menu: On/Off` style strings.
- Renamed the visible Settings dry-run action to `Preview changes` / `Предпросмотр изменений`; internal command naming and shell behavior remain unchanged.
- Added small self-authored `?` help buttons/tooltips for Explorer menu meaning, registry preview and installation paths; no external assets were added.
- Added App-layer path actions for installed app path, user data path and current command host: compact Open and Copy actions.
- Added `IPathActionService` / `WindowsPathActionService` so path open/copy is testable and stays out of WPF code-behind/Core.
- Documented future Help/About/Instructions window as separate UX debt.

## 2026-07-01 - Small UX correctness cleanup

- Removed the duplicate `Manage in Settings` content-area action from MainWindow; the title-bar gear remains the Settings entry point and the editor keeps only compact Explorer menu status.
- Replaced raw `Unsaved changes: True/False` presentation with localized saved/unsaved state text.
- Added `AllChangesSaved` and `UnsavedChanges` localization keys to all enabled WPF catalogs.
- Tuned shared action button geometry through `ActionButtonStyle` with larger padding and min-height for localized labels.
- Added a SettingsWindow scroll-content right gutter while preserving the single ScrollViewer and fixed footer.
- Updated UI audit/technical debt docs: UIA/TD items 0001, 0002 and 0004 are addressed; button geometry is an initial pass with future visual polish still open.

## 2026-07-01 - UI/UX audit baseline

- Added `docs/UI_AUDIT.md` as a documentation-first baseline for UI/UX/design issues found during manual inspection after Settings cleanup.
- Captured confirmed issues: redundant Settings entry points, raw boolean unsaved-changes status, cramped buttons, SettingsWindow scrollbar/content gutter, product-grade polish gap and missing app/build/window icon.
- Added UI/design debt items `TD-UI-0001` through `TD-UI-0006` to `docs/TECH_DEBT.md`.
- Updated roadmap to sequence small UX correctness cleanup, visual polish, branding/assets and later localization-sensitive layout feedback.
- Production code/XAML was intentionally not changed in this audit pass.

## 2026-07-01 - Settings and Explorer integration cleanup

- MainWindow cleanup: removed the large Explorer integration block, technical details and danger reset from the main editor surface.
- MainWindow now focuses on menu title, groups, entry cards, icons, save/discard and editor status/errors; later small UX cleanup removed the duplicate content-area `Manage in Settings` action and left only compact Explorer menu status.
- SettingsWindow now contains Application/language, Explorer menu, Installation/path information and Danger zone sections.
- Explorer menu actions in Settings execute immediately through the existing `ExplorerIntegrationController`; language Save remains separate.
- Dirty draft policy is preserved: dry-run/register require save/discard menu changes, while unregister remains allowed with unsaved draft changes.
- Reset from Settings keeps the existing reset semantics and causes MainWindow to reload saved draft state after the settings dialog closes.
- Manual locale spot-check recorded: Ukrainian, Japanese and German UI showed no blocking layout issue; RU/EN remain primary verified and other locales remain catalog-complete/test-covered.

## 2026-07-01 - Popular locale expansion and public docs audit

- Added embedded WPF localization catalogs for `it`, `nl`, `id`, `vi`, `hi`, `th`, `zh-Hant` and `pt-PT`.
- Enabled the popular locale batch in `FoldoraLanguage`, Settings UI and first-run language detection.
- Changed Settings language dropdown from insertion order to stable English/common-name sorting while still displaying native names.
- Documented Portuguese detection policy: exact `pt-BR` maps to `pt-BR`, exact `pt-PT` maps to `pt-PT`, and bare/other `pt-*` remains `pt-BR`.
- Audited the public README-linked docs and replaced old Russian legacy-menu examples with neutral English examples where they were not specifically about Russian localization.

## 2026-07-01 - Regional locale expansion

- Added embedded WPF localization catalogs for `uk`, `pl`, `tr`, `ro`, `cs`, `hu` and `bg`.
- Enabled Ukrainian, Polish, Turkish, Romanian, Czech, Hungarian and Bulgarian in `FoldoraLanguage`, Settings language options and first-run language detection.
- Kept future regional candidates `be`, `kk`, `uz-Latn`, `az`, `hy`, `ka`, `lt`, `lv`, `et`, `sk`, `sl`, `hr` and `sr` out of Settings until their catalogs are complete.
- Extended localized default menu titles, new-entry prefixes and default folder names for the regional batch without translating existing saved menu data.
- Updated localization tests for catalog completeness, native display names, supported culture mapping, future-candidate fallback and per-locale defaults.

## 2026-07-01 - Locale catalog expansion

- Added complete embedded WPF localization catalogs for `zh-Hans`, `de`, `es`, `fr`, `ja`, `pt-BR` and `ko` alongside existing `ru`/`en`.
- Enabled the new locales in `FoldoraLanguage`, Settings UI options and first-run language detection after catalog key completeness checks.
- Updated first-run mapping: supported culture families select their enabled locale, generic `pt`/`pt-*` maps to `pt-BR`, and unsupported cultures still persist `en`.
- Extended localized default menu titles, entry display-name prefixes and default folder names for all enabled locales without translating existing saved user data.
- Expanded localization tests to cover catalog key equality, non-empty enabled catalogs, Settings language exposure, detection mapping, persistence and per-locale new entry defaults.

## 2026-07-01 - First-run locale detection and persisted language selection

- Added storage metadata for whether `language` exists in `settings.json` and whether the persisted value is supported.
- Added App-level `SettingsLanguageInitializer` and `ISystemLanguageProvider`; WPF first-run uses `CultureInfo.CurrentUICulture`, maps `ru-*` to `ru`, `en-*` to `en`, and unsupported cultures to `en`.
- Persisted the selected language before normal draft loading, so system language detection runs only for missing/invalid language and does not override manual Settings choices later.
- Unsupported persisted language values now normalize to `en` and are saved as `en`; planned incomplete locales are not auto-selected.
- Added tests for first-run Russian/unsupported system cultures, old settings without language, invalid saved language, manual override preservation, and Settings UI locale exposure.

## 2026-07-01 - Validation localization cleanup

- Extended Core validation issues with stable issue-code constants and parameter dictionaries for dynamic values such as invalid characters, length limits, group names, file paths and menu limits.
- Added App-level `ValidationMessageLocalizer`; WPF summary errors and inline entry-card validation errors now render through embedded `ru`/`en` localization catalogs instead of showing Core fallback messages directly.
- Added validation catalog keys for display name, folder name, group name, icon, entry and menu-limit errors.
- Kept Core `Message` as compatibility/debug fallback for CLI and exception paths; CLI validation localization remains tracked debt.
- Added tests for Core validation parameters, validation message rendering in Russian/English, catalog completeness and WPF localized inline errors.

## 2026-07-01 - Localized menu title default and custom-title tracking

- Added `FolderMenuSettings.TitleIsCustom` to distinguish user-edited menu titles from localized product defaults.
- Added compatibility inference for old settings: empty/missing title and known defaults `Создать папку`/`Create folder` are default-title mode; other titles are custom.
- English UI now shows default menu title `Create folder`; Russian UI keeps `Создать папку`.
- Manual title edits mark the title custom, so future language changes do not rewrite it.
- WPF reset restores localized default-title mode; CLI/Shell reset keeps the compatibility default path.
- Added tests for old settings migration, language-switch behavior, custom title preservation, reset behavior and registry plan effective title.

## 2026-07-01 - Localization foundation cleanup

- Added `docs/LOCALIZATION.md` as the source of truth for locale policy, string categories, saved user data behavior, fallback rules, audit commands and planned locales.
- Replaced the early in-memory WPF string dictionaries with embedded JSON catalogs `src/Foldora.App/Localization/ru.json` and `en.json`.
- Localized WPF new entry defaults: English UI now creates `View N` / `New folder`, Russian UI creates `Вид N` / `Новая папка`.
- Preserved saved user data semantics: changing language does not rewrite existing menu title, entries, folder names or group names.
- Moved visible WPF labels/status/icon state and settings labels further into the localization layer.
- Added catalog completeness/fallback tests and presentation tests for localized new defaults and non-translation of existing user data.
- Documented remaining localization debt: Core compatibility defaults, CLI defaults/diagnostics, validation messages and startup fatal dialog.

## 2026-07-01 - Per-user install layout foundation

- Added `scripts/install-user.ps1`: it reuses fresh dev publish output and copies `Foldora.App.exe`, `Foldora.Cli.exe`, `Foldora.MenuHost.exe` and supporting files to `%LocalAppData%\Programs\Foldora`.
- Added `scripts/uninstall-user.ps1`: it unregisters Foldora-owned Explorer menu roots through installed CLI when available, falls back to deleting only Foldora-owned HKCU roots, removes installed binaries and preserves `%AppData%\Foldora` by default.
- Documented the split between installed binaries in `%LocalAppData%\Programs\Foldora` and user data/settings/imported icons/logs in `%AppData%\Foldora`.
- Documented that `Foldora.MenuHost.exe` is a short-lived no-console Explorer command host, not a service/tray/background/autostart process.
- Added resolver coverage for installed sibling `Foldora.MenuHost.exe` in a path with spaces.

## 2026-07-01 - MenuHost desktop placement diagnostics and bounded retry

- Added append-only JSONL diagnostic logging for MenuHost desktop placement at `%AppData%\Foldora\Logs\menuhost-placement.log`.
- Each create command logs target, entry id, created folder path/name, desktop detection details, cursor capture, positioning attempts, final result/message, exception details and final exit code.
- Added bounded retry in `DesktopPlacementCoordinator` only when positioning returns `Desktop item was not found: <name>`, covering the race where Explorer desktop view has not seen the newly-created item yet.
- Retry remains desktop-only, cursor-required and non-fatal; no retry for non-desktop targets, missing cursor, COM rejection, invalid args or other failures.
- Added tests for logging, skipped states, item-not-found retry success/exhaustion, non-retried failures, create failure logging and log writer failure safety.

## 2026-07-01 - Best-effort desktop placement integration

- Diagnostic desktop icon positioning manually confirmed on Windows 11: existing desktop icons move with both screen and view coordinates; Explorer grid displacement is accepted for MVP.
- `Foldora.MenuHost` now captures current cursor screen position at the start of `create`, calls the existing Core create flow, and best-effort repositions the created folder icon only when the target directory is the current user's Desktop directory.
- Placement failure is non-fatal: successful folder creation remains a successful MenuHost command.
- Core remains independent from Shell desktop positioning; registry command shape and `%V` placeholder policy did not change.
- Added unit tests for capture-before-create ordering, desktop-only positioning, non-fatal positioning failure, create-failure no-positioning, created folder name propagation and screen coordinate usage.

## 2026-07-01 - Desktop icon positioning prototype spike

- Добавлен isolated Shell diagnostic/prototype layer `Foldora.Shell.Desktop`: `IDesktopIconPositioningService`, result model, coordinate-space enum и Windows implementation для попытки reposition existing desktop item.
- Добавлена CLI diagnostic command `foldora diagnostics desktop-icon-position --name "<desktop item name>" --x <int> --y <int> [--coordinate-space screen|view]`.
- Команда не создаёт папки, не меняет registry command shape, не используется `Foldora.MenuHost.exe` и не решает получение original right-click coordinates.
- Добавлены unit-тесты parser-а и diagnostic runner-а через fake service без реального Explorer/Desktop.
- Docs обновлены, чтобы prototype был явно отделён от production create-under-cursor behavior.

## 2026-07-01 - Desktop icon placement research spike

- Проведён research spike текущего legacy desktop background flow: registry command получает target path через `%V`, но не получает original right-click coordinates или desktop icon-view coordinates.
- Зафиксировано, что `GetCursorPos` не является надёжным production fix без отдельного доказательства: после выбора submenu текущая позиция курсора может быть позицией menu item, а не исходной точкой на desktop.
- Добавлен `docs/research/DESKTOP_ICON_PLACEMENT.md` с current command path, API candidates, рисками и MVP-safe future design sketch.
- `TD-0001` повышен до high-priority research и разделён по смыслу на legacy coordinate gap и возможное post-create positioning через Shell view APIs.
- `TD-0002` переведён в `Cannot reproduce / Monitor`: текущая ручная проверка больше не воспроизводит first-created default icon.
- Production-код не менялся: Shell COM positioning, `GetCursorPos`, ListView messages, sleeps и `SHChangeNotify` не добавлялись без отдельного implementation spike.

## 2026-07-01 - Technical debt foundation and desktop behavior investigation

- Добавлен `docs/TECH_DEBT.md` с active debt format и двумя пунктами: `TD-0001` desktop icon placement is controlled by Explorer и `TD-0002` first created desktop folder may initially show default icon.
- `TD-0001` зафиксирован как accepted limitation текущего legacy `%V`/desktop background flow: Foldora получает target directory, но не получает cursor/icon-view coordinates.
- `TD-0002` зафиксирован как open investigation: вероятный Explorer desktop view/icon cache timing после `Directory.CreateDirectory` -> `desktop.ini` write -> attributes.
- `AGENTS.md`, `docs/README.md` и `docs/FILE_INDEX.md` обновлены, чтобы technical debt документ был частью будущих сессий.
- `docs/SHELL_INTEGRATION.md`, `docs/DESKTOP_INI.md`, `docs/SMOKE_TEST.md`, `docs/ROADMAP.md` и `docs/PROJECT_STATE.md` разделяют placement limitation и first-create icon refresh debt.
- Production-код не менялся: текущий код не содержит Shell refresh notification, но добавлять `SHChangeNotify`/WinAPI abstraction без отдельной reproduction matrix и тестируемого дизайна преждевременно.

## 2026-07-01 - Dev publish layout foundation

- Добавлен `scripts/publish-dev.ps1`: PowerShell 7 compatible script очищает только `artifacts/publish/Foldora`, публикует Release framework-dependent `Foldora.App`, `Foldora.Cli` и `Foldora.MenuHost` в одну папку и печатает next steps.
- `artifacts/` добавлен в `.gitignore`.
- WPF `ExplorerCommandHostPathResolver` теперь предпочитает sibling `Foldora.MenuHost.exe`, сохраняет Debug fallback и выдаёт controlled failure, если host не найден, вместо fallback на console CLI или несуществующий путь.
- Добавлены unit-тесты resolver-а: sibling publish host, missing-host failure, Debug fallback и регистрация через resolved sibling MenuHost без реального registry.
- README и docs обновлены под manual publish smoke flow; installer/MSIX/Program Files/code signing не добавлялись.

## 2026-07-01 - MVP stabilization pass

- Проверены root `README.md`, `LICENSE`, `THIRD_PARTY_NOTICES.md`, `AGENTS.md` и resource-policy docs после 0BSD/licensing изменения: README не обещает installer/MSIX/publish flow, лицензия указана как 0BSD, сторонние visual assets не заявлены.
- Проверены WPF compact/edit ViewModel/XAML: `IsEditing`, inline validation errors и entry count остаются presentation-only; `EntryId` не показывается в normal compact flow, кроме tooltip.
- `docs/SMOKE_TEST.md` дополнен явным шагом invalid entry -> inline error -> исправление -> `Готово`/`Сохранить`.
- `docs/UI_DESIGN.md` и `docs/MENU_MODEL.md` уточнены, чтобы current grouping containers не смешивались с прежним полем `Группа` в карточке и чтобы save-triggered registry rebuild не конфликтовал с историческим phase 2 описанием.
- Production-код и XAML не менялись.

## 2026-06-18 - Licensing and compact entry cards

- Добавлен root `LICENSE` со стандартной Zero-Clause BSD License (0BSD).
- Добавлен `THIRD_PARTY_NOTICES.md`: bundled third-party visual assets отсутствуют; test-only NuGet dependencies перечислены с license metadata из `.nuspec`.
- Root `README.md` обновлён: license scope, third-party material rules, requirements, `dotnet restore/build/test/run`, disclaimer and AI-assisted development note.
- `docs/RESOURCE_POLICY.md`, `AGENTS.md` и `docs/CODING_RULES.md` уточняют обязательный license audit перед добавлением сторонних ресурсов.
- Entry cards получили compact view state и inline edit state. Saved entries стартуют compact; новые draft entries стартуют в edit mode.
- `Готово` сворачивает только presentation state и не сохраняет settings; глобальная `Сохранить` остаётся единственным persistence action.
- Validation errors из Core validation layer раскрывают affected entry card и показываются inline рядом с полями.
- Group containers показывают entry count; Core menu model/settings format не менялись.

## 2026-06-18 - WPF layout correctness after design system

- Убрано визуальное дублирование `Foldora`: custom title bar остаётся application title, а content area использует semantic page header `Меню папок`/`Folder menu` с subtitle.
- `PrimaryButtonStyle`, `SecondaryButtonStyle` и `DangerButtonStyle` переведены на общий `ActionButtonStyle`, чтобы action buttons одного ряда имели одинаковую геометрию.
- `SettingsWindow` сделан resizable и перестроен на layout `header / scrollable content / fixed footer`.
- Language section больше не зависит от фиксированной высоты окна; central settings content прокручивается, а footer actions остаются доступными.
- Settings command/dialog flow не менялся; открытие окна настроек считается ручной проверкой пользователя, UIAutomation не используется как acceptance criterion для modal/custom-chrome WPF.

## 2026-06-18 - WPF design system foundation

- Добавлены `DesignTokens.xaml`, `Typography.xaml` и `Controls.xaml` как базовая WPF design system foundation.
- Вынесены semantic palette/brushes, spacing/radius/control-size tokens, reusable typography styles and reusable control/container styles.
- `App.xaml` подключает resource dictionaries в порядке tokens -> typography -> controls.
- `MainWindow.xaml` переведён с локальных hex/style definitions на semantic resources для page/title bar, buttons, cards/group containers, dangerous zone and status area.
- `SettingsWindow.xaml` использует те же surface, typography and button styles.
- Добавлены lightweight tests на подключение resource dictionaries и наличие ключевых semantic resources/styles.
- Поведение ViewModel/Core/CLI/Shell/MenuHost/settings не менялось; dark theme не реализована, только подготовлена через semantic brushes.

## 2026-06-18 - WPF grouping container UX redesign

- WPF groups redesigned from simple section headers into visual containers with header, nested entry cards and contextual add-entry button.
- `Без группы` remains a special root section and has no delete-group action.
- Non-empty group containers support inline title rename; rename updates `GroupName` for all entries in that group in draft.
- Delete group is staged and removes all draft entries with that `GroupName` only after inline confirmation; settings/registry still change only on `Сохранить`.
- Entry cards no longer show the always-visible technical `Группа:` textbox; grouping is controlled at container level.
- The supplied visual reference was used only as structural guidance, not as visual styling.
- Core menu model, registry shape, CLI behavior and one-level grouping limits were not changed.

## 2026-06-18 - WPF UX cleanup and resource policy

- Settings gear в custom title bar заменён с emoji/font-dependent glyph на self-authored XAML vector icon; внешние ассеты не добавлялись.
- WPF editor теперь показывает entries сгруппированными секциями `Без группы` и `<GroupName>` поверх существующей flat-модели `FolderMenuEntry.GroupName`.
- Добавлена кнопка `+ Добавить группу`; она создаёт обычный draft entry с новым `GroupName`, а не persistent empty group entity.
- Поле `Группа` остаётся в карточке как простой способ переместить entry между секциями; full tree и drag-and-drop не реализованы.
- Root `README.md` усилен минимальными требованиями, AppData layout, safety disclaimer, AI/Codex note и third-party resources note.
- Добавлен `docs/RESOURCE_POLICY.md` с правилами добавления и атрибуции внешних ресурсов.

## 2026-06-18 - MVP stabilization documentation

- Добавлен `docs/SMOKE_TEST.md` как ручной Windows 11 checklist для build/test, WPF startup, entry editing, Explorer integration, folder creation, save-triggered rebuild, unregister/reset и startup logs.
- `docs/SMOKE_TEST.md` добавлен в `AGENTS.md`, `docs/README.md` и `docs/FILE_INDEX.md`.
- Root `README.md` уточняет implemented one-level grouping, deletion-friendly `desktop.ini` attributes, known limitations и необходимость stable installed paths.
- `docs/ROADMAP.md` разделён на implemented MVP, next stage publish/dev layout и future work.
- `docs/PROJECT_STATE.md` обновлён с текущим stabilization status и следующим этапом stable `Foldora.App.exe`/`Foldora.Cli.exe`/`Foldora.MenuHost.exe` paths.
- Production-код не менялся.

## 2026-06-18 - One-level grouping MVP

- `FolderMenuEntry` расширен полем `GroupName`; пустое/whitespace значение означает root-level entry.
- Добавлена validation для `GroupName`: trim, максимум 80 символов, control chars запрещены, `/` и `\` запрещены как not-yet-supported nested groups.
- `FolderMenuSettingsValidator` теперь проверяет max 30 groups и max 30 enabled children per group.
- CLI `menu add` получил `--group`, а `menu list` показывает `Group: <root>` или имя группы.
- WPF карточка entry получила поле `Группа`/`Group` с подсказкой, что пустое значение оставляет пункт в корне меню.
- Registry plan builder строит one-level submenus под техническими keys `group-NNN`; `GroupName` пишется только как `MUIVerb`, не как registry path.
- Entry icon values продолжают работать для root-level и grouped entries.
- Full tree storage, nested depth > 1, drag-and-drop ordering и group icons не реализованы.

## 2026-06-18 - WPF startup bugfix

- Исправлен startup hang после custom title bar/settings/language foundation: `MainViewModel.CreateDefault()` больше не вызывает `LoadAsync().GetAwaiter().GetResult()` на WPF startup path.
- `MainViewModel` создаёт localization service с default language, а сохранённый `Language` применяет после async `LoadAsync`.
- `App.xaml` больше не использует `StartupUri`; `App.OnStartup` устанавливает обработчики ошибок и создаёт `MainWindow` вручную.
- Добавлен минимальный `StartupDiagnosticsService`, который пишет startup exceptions в `%AppData%\Foldora\Logs\startup-error.log`.
- Startup exceptions больше не исчезают молча: приложение пишет log и показывает простой error dialog.
- Custom title bar, settings gear, settings window и language foundation сохранены.
- Добавлены tests для default ViewModel construction без синхронной загрузки settings и для controlled startup diagnostic log.

## 2026-06-18 - WPF shell/settings foundation

- Главное окно WPF переведено на custom title bar через `WindowChrome`: `Foldora`, settings gear, minimize, maximize/restore и close находятся в единой шапке.
- Standard visible Windows title bar скрыт, resize border сохранён; maximize должен respect Windows work area/taskbar за счёт `WindowChrome`, без WinAPI `WM_GETMINMAXINFO`.
- Code-behind расширен только window plumbing: загрузка ViewModel, minimize, maximize/restore, close и обновление glyph maximize/restore.
- Добавлено settings window для выбора языка приложения.
- `FoldoraSettings.Language` нормализуется в `ru`/`en`; default и fallback для старых/невалидных settings - `ru`.
- Добавлен минимальный App-level localization service и bindable `LocalizationResources` для основных labels/buttons WPF editor.
- Runtime смена языка обновляет часть основных labels, но полный перевод всех status/error messages оставлен future cleanup.
- Группировка пунктов меню не реализована; roadmap фиксирует near-future one-level `FolderMenuEntry.GroupName` перед full tree model.

## 2026-06-18 - Documentation cleanup and public README

- Root `README.md` переписан как публичная GitHub-страница проекта: продуктовая идея, early MVP status, Windows 11/.NET 10/WPF stack, текущие возможности, ограничения, build/run и базовые CLI-примеры.
- `docs/DESKTOP_INI.md` уточняет результат ручной проверки default policy `ReadOnlyFolderHiddenDesktopIni`: folder attrib `R`, `desktop.ini` attrib `H`, custom icon сохраняется после refresh/reopen Explorer, deletion warnings из-за `System` attributes исчезли для новых папок.
- Зафиксировано, что старые папки с прежней `CompatibilitySystem` policy не мигрируются автоматически, и это нормально для текущего MVP.
- Repair/normalize command убрана из roadmap как ближайший investigation track; оставлена только как low-priority optional future idea при реальной потребности.
- `docs/SHELL_INTEGRATION.md` дополнительно фиксирует limitation legacy menu: Foldora создаёт папку в target directory, но Explorer выбирает позицию desktop icon; размещение строго под курсором не поддерживается текущим MVP.
- Production-код не менялся.

## 2026-06-18 - Desktop.ini production default policy

- После ручной проверки Windows 11 production default изменён на `ReadOnlyFolderHiddenDesktopIni`.
- Новые Foldora-created/apply folders получают folder `ReadOnly`, а `desktop.ini` получает только `Hidden`.
- `System` больше не ставится по default ни на папку, ни на `desktop.ini`, чтобы избежать Windows deletion warning.
- `CompatibilitySystem` и остальные policies сохранены для diagnostic/manual verification.
- Старые папки, созданные прежней policy, не мигрируются автоматически и могут сохранять `System` attributes.
- В roadmap/future ideas добавлена будущая repair/normalize command для старых папок, но она не реализована в этом шаге.
- Обновлены tests default policy, default apply attributes, entry-id apply/create и MenuHost create behavior.

## 2026-06-18 - Desktop.ini attribute policy investigation

- Зафиксировано MVP-ограничение legacy registry menu: Foldora получает target directory path (`%1`/`%V`), но не получает cursor coordinates или desktop icon-view coordinates; позицию нового значка выбирает Explorer.
- Добавлена Core-модель `DesktopIniAttributePolicy` с policies `CompatibilitySystem`, `ReadOnlyFolderSystemDesktopIni`, `ReadOnlyFolderHiddenDesktopIni` и `SystemFolderHiddenDesktopIni`.
- `DesktopIniService` теперь применяет выбранную attribute policy через `DesktopIniOptions`, но default production behavior оставлен прежним: folder `System`, `desktop.ini` `Hidden + System`.
- Добавлена CLI diagnostic command `foldora diagnostics desktop-ini-policy --target "<directory>" --icon "<ico>"`.
- Diagnostic command создаёт по одной тестовой папке на policy, применяет custom icon и печатает manual checklist; registry/AppData не трогаются.
- `docs/DESKTOP_INI.md` получил manual verification matrix с результатами `TBD`, чтобы выбрать deletion-friendly default после ручной проверки Explorer.
- Добавлены tests для policy attributes, desktop.ini content shape, parser diagnostics command и diagnostic runner.

## 2026-06-18 - WPF UX cleanup phase 1

- Главное окно WPF переведено с технического `DataGrid` на список карточек пунктов меню.
- User-facing labels заменили technical names: `Название в меню`, `Имя создаваемой папки`, `Показывать в меню`, `Иконка`.
- `EntryId` скрыт из основного UI; он остаётся внутренним идентификатором и доступен только как tooltip карточки.
- Добавлено нормальное empty state для пустого списка entries без demo entries.
- Normal integration controls отделены от `Опасная зона`; reset больше не находится рядом с dry-run/register/unregister.
- Technical registry plan details скрыты в `Expander` и показываются только при наличии деталей операции.
- Status area разделяет user-facing status и список ошибок; technical details больше не выводятся прямо в основном статусе.
- Добавлены presentation properties в `MainViewModel` и минимальные tests для empty/non-empty state, details toggle и reset confirmation state.
- Core/CLI/Shell/MenuHost/settings/registry behavior не изменялись.

## 2026-06-18 - Explorer integration UX hardening

- Registry plan теперь пишет `Icon = <entry.IconPath>` на entry key, если imported `.ico` существует; `DisplayName` по-прежнему используется только как `MUIVerb`.
- Добавлен `Foldora.MenuHost` как no-console `WinExe` для запуска Explorer menu commands без мигания console window.
- `Foldora.MenuHost` поддерживает `create --target --entry-id` и `apply --folder --entry-id`, используя существующий `FolderMenuEntryActionService`.
- `register-menu` получил `--host-path "<absolute-path-to-Foldora.MenuHost.exe>"`; legacy `--cli-path` сохранён как dev/backward-compatible alias.
- Default register path resolution предпочитает `Foldora.MenuHost.exe`; CLI остаётся console tool для ручных команд.
- WPF command-host resolver предпочитает `Foldora.MenuHost.exe` и имеет fallback на CLI только для dev/debug ситуации.
- WPF `Сохранить` теперь rebuild-ит Foldora-owned registry menu, если integration уже была enabled; при disabled integration Save пишет только settings.
- Если rebuild после settings save падает, settings не откатываются, UI показывает `Настройки сохранены, но меню Проводника не обновлено.`
- Если после Save enabled entries нет, register-service удаляет owned roots, сохраняет `ExplorerIntegrationEnabled = false`, UI сообщает, что меню отключено.
- Добавлены tests для registry `Icon`, MenuHost, `--host-path`, WPF save rebuild и failure semantics.

## 2026-06-18 - WPF editor phase 4

- Добавлен App-level `ExplorerIntegrationController` для WPF-команд dry-run/register/unregister/reset поверх существующего `ExplorerMenuRegistrationService`.
- Главное окно получило отдельный блок `Интеграция с Проводником`: статус, `Проверить план`, `Включить меню Проводника`, `Отключить меню Проводника` и `Сбросить меню` с checkbox-подтверждением.
- `Проверить план` строит validated registry plan, показывает summary операций/root paths/command example и не пишет registry/settings.
- `Включить меню Проводника` требует clean draft, применяет validated HKCU plan и включает `ExplorerIntegrationEnabled`; при отсутствии enabled entries удаляет owned roots и оставляет integration disabled.
- `Отключить меню Проводника` разрешено даже при unsaved draft changes, удаляет только Foldora-owned roots, сохраняет entries и ставит `ExplorerIntegrationEnabled = false`.
- `Сбросить меню` очищает entries, возвращает title к `Создать папку`, отключает integration и не удаляет AppData root/settings/packs/imported icons.
- Обычный WPF `Сохранить` не перестраивает registry menu.
- Добавлены controller tests с fake registry для dry-run/register/unregister/reset и dirty-state policy.

## 2026-06-18 - WPF editor phase 3

- Добавлен App/WPF preview service для прямой загрузки `.ico` без генерации файлов в `%AppData%\Foldora\previews`.
- Entry rows теперь показывают preview около 50x50 для saved `IconPath` и pending выбранной `.ico`.
- Pending icon preview обновляется до save, но импорт в AppData по-прежнему происходит только при `Сохранить`.
- Missing/corrupt icon preview возвращает structured result и не валит окно; UI показывает empty preview/status.
- Core project остаётся без WPF-зависимостей; preview loading находится только в `Foldora.App`.
- Тесты переведены на `net10.0-windows` для проверки WPF preview service; добавлены tests для valid/missing/corrupt preview и project boundary.

## 2026-06-18 - WPF editor phase 2

- WPF editor получил `+ Добавить пункт`, staged удаление entries, row action `Выбрать .ico` и row action `Удалить`.
- Выбор `.ico` через WPF file picker сохраняет только pending source path в draft; AppData и `settings.json` не меняются до `Сохранить`.
- `SaveAsync` draft editor валидирует pending `.ico`, импортирует их через `IconImportService` в `%AppData%\Foldora\icons\<entry-id>.ico`, обновляет `IconPath` и затем сохраняет settings.
- Добавлен App-layer picker abstraction `IIconFilePicker`; file picker не находится в Core и не размещён в code-behind.
- Удаление entry из WPF остаётся staged и не удаляет импортированные `.ico`; orphan cleanup отложен.
- Preview, registry rebuild/buttons, reset UI, nested UI и drag-and-drop не реализованы.
- Расширены unit-тесты draft editor на add/remove/pending icon import/invalid icon/cancel/orphan behavior.

## 2026-06-18 - WPF editor phase 1

- Добавлен Core draft editor для staged-save редактирования `CreateFolderMenu.Title` и существующих `FolderMenuEntry` без WPF/registry-зависимостей.
- WPF bootstrap-окно заменено на минимальный редактор: title, список entries, `DisplayName`, `DefaultFolderName`, `IsEnabled`, `EntryId`, статус и ошибки.
- Добавлены ViewModel-классы и команды; code-behind оставлен только для `InitializeComponent`, `DataContext` и initial load.
- `Сохранить` валидирует draft через существующий Core validation layer и пишет `settings.json` только при отсутствии ошибок.
- `Отменить изменения` перезагружает draft из сохранённого baseline.
- WPF phase 1 не выбирает `.ico`, не показывает preview, не добавляет/удаляет entries и не перестраивает registry menu.
- Добавлены unit-тесты draft editor logic на временном storage root без реального AppData/registry.

## 2026-06-18 - Documentation consolidation

- Добавлен `docs/PRODUCT_VISION.md` с продуктовой концепцией, главным MVP-объектом `FolderMenuEntry`, ролью packs и принципом freedom with safety.
- Добавлен `docs/UX_FLOW.md` с целевым WPF MVP, staged-save flow, input behavior для `DefaultFolderName`, preview policy и cleanup controls.
- `AGENTS.md`, `docs/README.md` и `docs/FILE_INDEX.md` обновлены, чтобы новые документы были обязательной частью будущих сессий.
- Roadmap/future notes дополнены WPF MVP, nested menu model, preview generation, orphan icon cleanup и installer/publish path вопросом.
- Requirements дополнены локальной Windows/PowerShell/tooling средой и требованием .NET SDK 10.x.
- Shell/menu/settings/UI/packs документы синхронизированы с текущей продуктовой моделью и safety-правилами.

## 2026-06-18 - Simplified Explorer menu shape and menu reset

- Registry plan builder больше не создаёт промежуточный ключ `create-folder`.
- Видимое legacy menu теперь имеет форму `<CreateFolderMenu.Title> -> entries`; fallback title: `Создать папку`.
- Technical safety boundary сохранён: owned roots остаются `Software\Classes\Directory\shell\Foldora` и `Software\Classes\Directory\Background\shell\Foldora`.
- Entries создаются напрямую под `...\Foldora\shell\entry-...`; `DisplayName` используется только как `MUIVerb`, а не как registry key path.
- `unregister-menu` сохранён как безопасное отключение Explorer integration без удаления пользовательских entries/settings.
- Добавлена CLI-команда `menu reset --yes`: удаляет только Foldora-owned registry roots, очищает entries, возвращает title к `Создать папку`, ставит `ExplorerIntegrationEnabled = false`, не удаляет AppData root/settings/packs/icons.
- `menu reset` без `--yes` отказывается выполнять сброс.
- Обновлены unit-тесты registry shape, cleanup semantics, reset semantics и CLI parser.

## 2026-06-17 - HKCU registry writer and register-menu CLI

- Добавлены `IRegistryAccess` и `WindowsRegistryAccess`; `Microsoft.Win32.Registry` используется только в `WindowsRegistryAccess`.
- Добавлен `ExplorerMenuRegistryWriter`, который применяет только validated registry plan.
- Добавлен `ExplorerMenuRegistrationService` для `register-menu`, `register-menu --dry-run` и `unregister-menu`.
- CLI `register-menu` теперь поддерживает `--dry-run` и `--cli-path`.
- CLI `unregister-menu` удаляет только Foldora-owned roots и является idempotent.
- `register-menu --dry-run` печатает delete/create/set операции и не меняет registry/settings.
- После успешной регистрации settings получает `ExplorerIntegrationEnabled = true`; при пустых enabled entries меню удаляется и флаг остаётся/становится `false`.
- Добавлены fake registry tests без записи в реальный реестр.

## 2026-06-17 - Testable registry plan builder

- Добавлен слой `Foldora.Shell.RegistryPlan` для построения будущих HKCU legacy context menu operations.
- Добавлены модели registry plan: target kind, hive, create key, set value, delete key и validation result.
- Добавлен `ExplorerMenuRegistryPlanBuilder` для flat menu entries.
- Добавлен `ExplorerMenuCommandBuilder`, который формирует команды `create --target ... --entry-id ...` через `CommandLineQuoter`.
- Добавлен `ExplorerMenuRegistryPlanValidator`, запрещающий HKLM и операции вне Foldora-owned roots.
- Empty enabled entries строят только delete owned root operations, без пустого submenu.
- Добавлены unit-тесты registry plan builder/validator.

## 2026-06-17 - Execute menu entries from CLI

- Добавлен `FolderMenuEntryResolver` для поиска enabled entry в settings.
- Добавлен `UniqueFolderNameService` для выбора имени `Name`, `Name (2)`, `Name (3)` без перезаписи файлов и папок.
- Добавлен `FolderMenuEntryActionService` для `apply` и `create` по `entry-id`.
- CLI `apply` расширен режимом `--entry-id`; `--icon` и `--entry-id` взаимоисключающие.
- Реализован CLI `create --target "<directory>" --entry-id "<entry-id>"`.
- Добавлены тесты action-сервиса, parser-а и unique folder name helper.

## 2026-06-17 - Menu model validation

- `FolderMenuEntry` расширен полем `DefaultFolderName` с fallback `Новая папка`.
- Добавлен validation слой для display name, folder name, menu limits и `.ico` structure.
- Добавлен `FolderNameSanitizer` для будущего WPF input/paste flow.
- `IconImportService` теперь проверяет `.ico` header/directory и лимит 10 MB.
- `FolderMenuService` валидирует entry до импорта и сохранения.
- CLI `menu add` получил опцию `--folder-name`, а `menu list` показывает `DefaultFolderName`.
- Добавлен документ `docs/MENU_MODEL.md` со staged-save, nested menu и registry safety design.
- Расширены unit-тесты validation/model/settings/CLI.

## 2026-06-17 - User menu entries and settings storage

- Добавлен AppData layout с папками `icons`, `previews`, `packs` и файлом `settings.json`.
- Добавлены модели `FolderMenuEntry` и `FolderMenuSettings`.
- Расширена `FoldoraSettings` настройками меню `CreateFolderMenu`.
- Добавлен JSON storage `FoldoraSettingsStorage` с `EnsureCreatedAsync`, `LoadAsync`, `SaveAsync`.
- Добавлены `IconImportService`, `FolderMenuNameGenerator` и `FolderMenuService`.
- Реализованы CLI-команды `foldora menu list`, `foldora menu add --icon ... [--name ...]`, `foldora menu remove --entry-id ...`.
- Добавлены тесты storage, fallback-имён, импорта `.ico`, menu service и CLI parser.

## 2026-06-17 - CLI apply/clear vertical slice

- Реализована команда `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"`.
- Реализована команда `foldora clear --folder "<folder>"`.
- `DesktopIniService` теперь проверяет существование `.ico`, обновляет `IconResource` в секции `[.ShellClassInfo]` и сохраняет чужие секции `desktop.ini`.
- `clear` удаляет только `IconResource` из `[.ShellClassInfo]`; если полезных строк не осталось, удаляет `desktop.ini`.
- Добавлен тестируемый CLI parser.
- Расширены unit-тесты Core и добавлены тесты CLI parser.

## 2026-06-17 - Bootstrap initialization

- Создана solution `Foldora.sln`.
- Добавлены проекты `Foldora.Core`, `Foldora.Shell`, `Foldora.Cli`, `Foldora.App`, `Foldora.Tests`.
- Настроены project references по архитектуре.
- Добавлены минимальные модели, AppData paths, desktop.ini service, shell registrar skeleton и CLI help.
- Добавлена стартовая документация, `AGENTS.md`, `README.md`, `.gitignore`.
- Добавлены unit-тесты для AppData paths, desktop.ini content/write и command line quoting.

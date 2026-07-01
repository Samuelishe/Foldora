# Technical Debt

Этот документ отделяет известные technical debt и accepted limitations от roadmap features. Он не является обещанием реализации: каждый пункт требует отдельного решения перед изменениями в production-коде.

## Active Debt

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

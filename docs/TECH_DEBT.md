# Technical Debt

Этот документ отделяет известные technical debt и accepted limitations от roadmap features. Он не является обещанием реализации: каждый пункт требует отдельного решения перед изменениями в production-коде.

## Active Debt

### TD-0001 Desktop Icon Placement Is Controlled By Explorer

- ID: `TD-0001`
- Title: Desktop icon placement is controlled by Explorer
- Status: Accepted limitation / Research later
- Severity: Medium
- Area: Shell
- Observed behavior: Foldora legacy menu can be invoked from desktop background, but the created folder icon is placed by Explorer in a free desktop icon-view slot, not under the cursor position where the menu was opened.
- Expected/desired behavior: If technically feasible in a future integration model, creating from desktop background could place the new folder near the invocation point.
- Known cause or hypothesis: Current HKCU legacy context menu commands receive target directory path through Explorer placeholders such as `%V`, but they do not receive cursor coordinates or desktop icon-view coordinates.
- Current workaround: User can move the created desktop icon manually after creation.
- Next investigation step: Research modern shell integration, COM, `IExplorerCommand`, desktop view APIs or another explicit advanced shell path in a separate stage.
- Links to docs/tests/code:
  - `docs/SHELL_INTEGRATION.md`
  - `docs/SMOKE_TEST.md`
  - `src/Foldora.Shell/RegistryPlan/ExplorerMenuShellTargetPlaceholder.cs`
  - `src/Foldora.Shell/RegistryPlan/ExplorerMenuCommandBuilder.cs`
  - `tests/Foldora.Tests/Shell/ExplorerMenuRegistryPlanBuilderTests.cs`
- Date added: 2026-07-01

### TD-0002 First Created Desktop Folder May Initially Show Default Icon

- ID: `TD-0002`
- Title: First created desktop folder may initially show default icon
- Status: Open / Investigating
- Severity: Medium
- Area: Desktop.ini
- Observed behavior: On Windows 11 desktop, the first folder created from the published Foldora legacy menu after registration/system start may initially appear with the default folder icon. A retry or subsequent created folder can show the custom icon correctly.
- Expected/desired behavior: A newly created folder should show the selected custom icon consistently after Foldora writes `desktop.ini` and applies the deletion-friendly attributes.
- Known cause or hypothesis: Explorer desktop view may observe the folder before Foldora finishes writing `desktop.ini`, setting `IconResource`, and applying folder/`desktop.ini` attributes; alternatively Explorer icon cache or desktop view notification timing may be insufficient.
- Current workaround: Refresh Explorer/Desktop, reopen the folder view, or create again; later creations can pick up the custom icon correctly.
- Next investigation step:
  - Confirm exact reproduction conditions after publish registration.
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

## Do Not Do

- Не обещать создание папки под курсором в текущем MVP.
- Не добавлять COM shell extension без отдельного явного этапа.
- Не делать modern Windows 11 context menu в этом debt pass.
- Не патчить Explorer или системные DLL.
- Не добавлять random sleep как production fix без investigation.
- Не ломать deletion-friendly desktop.ini policy `ReadOnlyFolderHiddenDesktopIni` (`folder: ReadOnly`, `desktop.ini: Hidden`).
- Не менять Core/JSON model ради desktop icon placement или icon refresh timing.
- Не использовать UIAutomation как acceptance criterion для этих пунктов.

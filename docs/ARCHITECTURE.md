# Architecture

Foldora разделена на шесть проектов:

- `Foldora.Core` - доменная логика, модели, settings, AppData paths, desktop.ini, validation и тестируемые filesystem операции.
- `Foldora.Shell` - Windows Shell integration: HKCU legacy context menu, unregister flow и command line quoting.
- `Foldora.Cli` - тонкий console интерфейс для команд context menu и ручного запуска.
- `Foldora.MenuHost` - no-console Windows executable для запуска Explorer context menu commands.
- `Foldora.App` - WPF-приложение настроек.
- `Foldora.Tests` - unit-тесты Core и Shell.

Зависимости:

- `Foldora.App` -> `Foldora.Core`, `Foldora.Shell`.
- `Foldora.Cli` -> `Foldora.Core`, `Foldora.Shell`.
- `Foldora.MenuHost` -> `Foldora.Core`.
- `Foldora.Shell` -> `Foldora.Core`.
- `Foldora.Tests` -> `Foldora.Core`, `Foldora.Shell`, `Foldora.Cli`, `Foldora.App`, `Foldora.MenuHost`.
- `Foldora.Core` не зависит от других проектов Foldora.

Границы: Core не знает о WPF, CLI, registry API и конкретном UI. Registry logic не находится в Core. WPF code-behind используется только для UI plumbing.

Пользовательские menu entries являются главным MVP-объектом для будущего Explorer submenu. Пользователь сам выбирает любые `.ico` и любые подписи; Foldora копирует `.ico` в AppData и хранит metadata в `settings.json`. Registry context menu на следующем этапе должен генерироваться из этих сохранённых entries, а не из жёстко заданных категорий вроде Documents/Code/Photos.

Validation/model слой находится в `Foldora.Core`, а не в CLI/WPF. `DisplayName` отвечает только за подпись меню, `DefaultFolderName` - за имя создаваемой папки. Эти поля не смешиваются.

Исполнение сохранённых entries также находится в `Foldora.Core`: `FolderMenuEntryActionService` загружает settings, резолвит enabled entry, создаёт папку при необходимости и вызывает `DesktopIniService`. CLI и MenuHost не дублируют filesystem или `desktop.ini` логику.

`DesktopIniService` применяет атрибуты через `DesktopIniAttributePolicy`. Default policy после ручной проверки Windows 11: `ReadOnlyFolderHiddenDesktopIni`, то есть folder `ReadOnly`, `desktop.ini` `Hidden`. Выбор default централизован в `DesktopIniAttributePolicy.Default`; CLI/MenuHost/WPF production flows не выбирают policy сами. `CompatibilitySystem` и другие policies сохранены для diagnostics/manual verification.

`Foldora.Shell` содержит только shell-specific planning logic. `ExplorerMenuRegistryPlanBuilder` строит testable plan HKCU операций под Foldora-owned roots, но не пишет в реестр. Видимый legacy menu root берётся из `CreateFolderMenu.Title`, при этом technical registry root остаётся `Foldora` для safety boundary. Entry registry keys получают `Icon` value только из существующего imported `IconPath`; это shell menu icon, не WPF preview.
`ExplorerMenuRegistryWriter` применяет только validated plan через `IRegistryAccess`. `WindowsRegistryAccess` - единственное место, где используется `Microsoft.Win32.Registry`; тесты используют fake/in-memory registry access.

`Foldora.MenuHost` является Windows-subsystem executable (`WinExe`) без UI и console output. Explorer legacy menu должен запускать `Foldora.MenuHost.exe`, чтобы не мигало console window. MenuHost возвращает non-zero exit code при ошибках; user-facing diagnostics для context menu failures остаются future work.

Cleanup flow разделён на две операции. `unregister-menu` удаляет только Foldora-owned registry roots и ставит `ExplorerIntegrationEnabled = false`, не удаляя entries/settings. `menu reset --yes` удаляет те же owned roots, очищает `CreateFolderMenu.Entries`, возвращает title к `Создать папку` и сохраняет settings; AppData root, `settings.json`, packs и импортированные `.ico` не удаляются.

WPF editor использует staged-save слой в `Foldora.Core`: `FolderMenuDraftEditor` и `FolderMenuDraftEntry`. Этот слой загружает settings, держит draft-копии, валидирует через существующий validation layer и сохраняет settings только по явному `SaveAsync`. Phase 2 добавляет draft add/remove и pending icon source: выбранные `.ico` импортируются через `IconImportService` только во время save. Phase 3 добавляет `.ico` preview только в `Foldora.App`: WPF service загружает preview напрямую из saved/pending path и не создаёт preview-файлы.

Phase 4 добавляет в `Foldora.App` App-level `ExplorerIntegrationController`: WPF ViewModel вызывает controller, controller использует существующий `ExplorerMenuRegistrationService` из `Foldora.Shell`, а real registry access по-прежнему проходит только через `WindowsRegistryAccess`. Dry-run/register/unregister/reset являются отдельными явными commands. Dry-run/register требуют clean draft, чтобы registry отражал saved settings. Unregister можно выполнять при unsaved draft changes, потому что он не зависит от draft entries и сохраняет пользовательские entries/settings. После UX hardening обычный WPF `SaveAsync` rebuild-ит registry только если `ExplorerIntegrationEnabled` уже был `true`; при disabled integration Save пишет только settings. UX cleanup phase 1 добавляет только presentation properties (`HasEntries`, status/error/detail visibility state) и card/list XAML; business logic остаётся в Core/Shell/App services, а code-behind не содержит бизнес-логики и не пишет registry.

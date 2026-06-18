# Work Log

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

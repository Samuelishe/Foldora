# Work Log

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

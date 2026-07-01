# Work Log

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

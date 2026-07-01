# UX Flow

Этот документ фиксирует целевой пользовательский flow для WPF MVP. WPF editor реализует staged editing, add/remove entries, staged выбор `.ico`, прямой preview из `.ico`, явные Explorer integration controls и user-facing card/list layout.

Window shell foundation: главное окно использует custom title bar с названием приложения, settings gear и window controls. Settings gear открывает настройки приложения: language, Help/About, Explorer integration, installation/path information and danger zone. Изменение языка сохраняется отдельно от draft entries и не пишет registry; Explorer integration actions в Settings выполняются сразу.

Startup errors не должны приводить к silent exit/no-window состоянию. При startup exception Foldora пишет `%AppData%\Foldora\Logs\startup-error.log` и показывает простой startup error dialog. Обычная загрузка settings остаётся async после создания `MainWindow`, чтобы не блокировать WPF dispatcher до появления окна.

## WPF MVP Editor

Минимальный редактор пользовательского меню должен содержать:

- поле названия top-level menu, по умолчанию локализованное для текущего enabled locale;
- список пользовательских entries;
- кнопку `+` для добавления пункта;
- кнопку удаления пункта;
- поле `DisplayName`;
- поле `DefaultFolderName`;
- поле `GroupName`/`Группа` для optional одноуровневой группы;
- кнопку `Выбрать .ico`;
- preview иконки примерно 50x50;
- checkbox `IsEnabled`;
- кнопку `Сохранить`;
- кнопку `Отменить изменения`;
- компактный статус Explorer menu. Управление системными действиями открывается через gear/settings в title bar.

После UX cleanup phase 1 список entries отображается как карточки, а не как technical table. Основные labels:

- `Название в меню`;
- `Имя создаваемой папки`;
- `Группа`;
- `Показывать в меню`;
- `Иконка`.

`EntryId` скрыт из основного пользовательского flow.

UI не должен полагаться на `MessageBox` как основной механизм ошибок. Предпочтительные механизмы:

- inline validation рядом с полями;
- status area;
- список ошибок/предупреждений перед сохранением;
- аккуратные dialog windows позже, если нужны.

## Staged Save

Редактор не должен сразу применять изменения в registry.

Целевая модель:

```text
Открыть Foldora
  -> загрузить Saved settings
  -> создать Draft state
  -> пользователь добавляет/удаляет/редактирует пункты
  -> Explorer menu не меняется
  -> пользователь нажимает "Сохранить"
      -> validate draft
      -> import pending icons
      -> write settings.json
      -> if ExplorerIntegrationEnabled was true: rebuild Foldora-owned registry menu
      -> show success/error
```

Добавление, удаление и редактирование строк в UI не должны сразу писать в registry. Registry меняется только по явным действиям: `Сохранить`, если integration уже была включена, или отдельные Explorer integration buttons.

После UX hardening `Сохранить` валидирует draft, импортирует pending `.ico` в AppData и пишет `settings.json`. Если `ExplorerIntegrationEnabled = true`, после successful save WPF rebuild-ит Foldora-owned registry menu и показывает `Настройки сохранены. Меню Проводника обновлено.` Если integration disabled, Save пишет только settings и показывает `Настройки сохранены.`

Выбор `.ico` является staged:

```text
User clicks "Выбрать .ico"
  -> file picker returns external source path
  -> draft entry stores PendingIconSourcePath
  -> UI shows selected filename/status and direct .ico preview
  -> settings.json is not changed
  -> AppData icons are not permanently changed yet

User clicks "Сохранить"
  -> validate draft
  -> validate pending icon source
  -> import icon through IconImportService
  -> update entry.IconPath to imported AppData path
  -> save settings.json
```

`Отменить изменения` очищает pending icon selections, возвращает удалённые draft entries и убирает новые unsaved entries.

Если registry rebuild упадёт после успешного сохранения settings, UI показывает:

```text
Настройки сохранены, но меню Проводника не обновлено.
```

Сложный rollback settings из-за registry failure не нужен.

Если после Save enabled entries стало 0, register-service удаляет Foldora-owned roots, сохраняет `ExplorerIntegrationEnabled = false` и UI показывает: `Настройки сохранены. Включённых пунктов нет, меню Проводника отключено.`

## DefaultFolderName Input

`DefaultFolderName` - это имя папки, создаваемой при выборе пункта меню.

При создании нового entry поле по умолчанию:

```text
Новая папка
```

Если пользователь очищает поле, runtime fallback:

```text
Новая папка
```

Решение для будущего WPF input:

- при ручном вводе invalid Windows filename chars лучше не давать печатать;
- при paste invalid chars можно заменять пробелом/удалять и показывать предупреждение;
- при сохранении validator всё равно обязан проверить данные;
- CLI не должен молча исправлять явно invalid `--folder-name`, CLI должен вернуть понятную ошибку.

## Group Input

`GroupName` - optional имя одноуровневого submenu.

```text
Create folder
  Colors
    Blue
    Red
  Media
    Music
```

Если поле пустое, entry остаётся прямо в root menu. `/` и `\` не принимаются, чтобы пользователь не думал, что nested groups уже поддерживаются. Full tree, drag-and-drop ordering и group icons остаются future work.

WPF показывает эту flat-модель как секции:

```text
No group
  Music

Colors
  Blue
  Red
```

Секции являются только presentation layer. Пустые группы не сохраняются как отдельные сущности. `+ Добавить группу` создаёт draft entry с новым `GroupName`; если пользователь отменит изменения или удалит этот entry, такая группа исчезнет.

После grouping container UX cleanup non-empty groups показываются как отдельные visual containers:

- editable group title in header;
- delete-group button with inline confirmation;
- nested entry cards;
- contextual `+ Добавить пункт в эту группу`.

`Без группы` является root-секцией для entries с пустым `GroupName`; она не имеет кнопки удаления группы. Удаление группы в UI удаляет из draft все entries с этим `GroupName`, но не пишет settings и registry до `Сохранить`. Переименование группы обновляет `GroupName` у всех entries этой группы в draft.

Схематичный visual reference для этого UX используется только как структура `группа -> элементы`; цвета, размеры, шрифты и грубая геометрия не копируются.

## Compact/Edit Entry Cards

WPF entry cards имеют compact view state и inline edit state.

Default state для загруженных saved entries - compact:

```text
[preview] Название в меню
          Имя папки: <DefaultFolderName>
          Иконка: <state>
          [enabled] [Редактировать] [Удалить]
```

Новый draft entry сразу раскрывается в edit state, чтобы пользователь мог задать поля и выбрать `.ico`.

Edit state показывает:

- `Название в меню`;
- `Имя создаваемой папки`;
- `Показывать в меню`;
- icon preview/status;
- `Выбрать .ico`;
- `Готово`;
- `Удалить`;
- inline validation errors for this entry.

`Готово` не сохраняет settings. Оно только сворачивает карточку, если у неё нет inline validation errors. Глобальная кнопка `Сохранить` остаётся единственным действием persistence для draft menu.

Если Core validation возвращает error с `EntryId`, соответствующая карточка раскрывается и показывает ошибку inline. Общий status/errors area остаётся как summary.

## Icon Preview

WPF phase 3 показывает `.ico` напрямую в строке entry, примерно 50x50.

Preview source:

- pending selected icon, если он есть;
- иначе saved `IconPath`;
- иначе empty placeholder.

Если preview decode падает или файл отсутствует, editor показывает empty preview/status и не валит окно.

Папка зарезервирована на будущее:

```text
%AppData%\Foldora\previews\
```

Preview generation всё ещё future:

- не генерировать preview-файлы без необходимости;
- если WPF preview из `.ico` окажется медленным/нестабильным, добавить отдельный preview generator;
- preview generator должен хранить ресурсы в `%AppData%\Foldora\previews\`.

## Explorer Integration Controls

Главный экран больше не показывает большой блок Explorer integration или danger zone. Он остаётся focused menu editor: title, groups, entries, icons, save/discard, status/errors. Допустимый главный экран status - компактная строка `Explorer menu: Enabled/Disabled` без дублирующей кнопки управления; Settings открываются через gear в title bar.

Explorer integration управляется в SettingsWindow. В UI явно разделены операции:

- `Предпросмотр изменений` / `Preview changes` - WPF-аналог `register-menu --dry-run`: построить и валидировать registry plan, показать summary операций/root paths/command example, ничего не писать в registry и не менять `ExplorerIntegrationEnabled`.
- `Включить меню Проводника` - применить validated HKCU legacy menu из saved settings и поставить `ExplorerIntegrationEnabled = true`, если есть enabled entries.
- `Отключить меню Проводника` - выполнить семантику `unregister-menu`: убрать Foldora из Explorer, но сохранить entries/settings.
- `Сбросить меню` - выполнить семантику `menu reset --yes` после явного подтверждения: очистить entries, вернуть title к localized default-title mode, отключить integration.

`Предпросмотр изменений` и `Включить меню Проводника` требуют clean draft. Если есть unsaved changes, UI показывает `Сначала сохраните изменения.` и не выполняет operation. Это сохраняет правило: registry отражает saved settings, а не временный draft.

`Отключить меню Проводника` можно выполнять при unsaved changes, потому что операция не зависит от draft entries и не удаляет пользовательские entries/settings.

Если enabled entries нет, `Включить меню Проводника` не создаёт пустое меню: Foldora-owned roots удаляются, `ExplorerIntegrationEnabled` остаётся `false`, UI сообщает `Нет включённых пунктов меню. Меню Проводника не создано.`

`Сбросить меню` требует явного подтверждения в UI. Reset очищает saved entries, возвращает title в localized default-title mode для текущего enabled locale, удаляет только Foldora-owned registry roots, сохраняет `settings.json`, не удаляет AppData root, packs и импортированные `.ico`.

Dangerous reset отображается в SettingsWindow в отдельной `Опасная зона`, а не рядом с редактором меню.

Technical registry plan details скрыты по умолчанию. Обычный status показывает краткий user-facing результат; operation roots, counts и command example доступны в раскрываемом technical details block.

Отсутствие меню в Explorer является нормальным состоянием, а не ошибкой.

## Empty State

Если entries нет, WPF показывает empty state:

```text
Пока нет пунктов меню.
Добавьте первый пункт: выберите .ico, задайте название в меню и имя создаваемой папки.
```

Demo entries автоматически не создаются.

## Manual Verification Flow

Для разработки и ручной проверки:

```text
foldora menu add --icon "<path-to-test.ico>" --name "Documents" --folder-name "Documents"
foldora register-menu --dry-run
foldora register-menu --host-path "<absolute-path-to-Foldora.MenuHost.exe>"
```

После проверки:

```text
foldora unregister-menu
```

Для полного сброса пользовательского меню:

```text
foldora menu reset --yes
```

## Settings Flow

Settings UI является полноценным окном настроек приложения:

```text
Настройки

[Application] [Explorer menu] [Installation] [Help/About] [Danger zone]

Application tab
  Язык приложения:
  [Български / 简体中文 / 繁體中文 / Čeština / Nederlands / English / Français / Deutsch / हिन्दी / Magyar / Bahasa Indonesia / Italiano / 日本語 / 한국어 / Polski / Português (Brasil) / Português (Portugal) / Română / Русский / Español / ไทย / Türkçe / Українська / Tiếng Việt]

Explorer menu tab
  Foldora Explorer menu: On/Off
  Preview changes
  Enable Explorer menu
  Disable Explorer menu
  Passive ? glyphs with wrapped tooltip/help text explaining HKCU preview and legacy Explorer menu
  Technical details

Installation tab
  Installed app path
    Open / Copy
  User data path
    Open / Copy
  Current command host
    Open / Copy
  MenuHost is not a service note

Help/About tab
  Short instructions
  Open Help/About window

Danger zone tab
  Reset menu confirmation
  Reset menu

[Сохранить] [Закрыть]
```

`Language` сохраняется в `%AppData%\Foldora\settings.json`. Complete/enabled values: `bg`, `cs`, `de`, `en`, `es`, `fr`, `hi`, `hu`, `id`, `it`, `ja`, `ko`, `nl`, `pl`, `pt-BR`, `pt-PT`, `ro`, `ru`, `th`, `tr`, `uk`, `vi`, `zh-Hans`, `zh-Hant`. При первом WPF запуске язык выбирается из system UI culture только для complete/enabled locale; unsupported language -> `en`; затем выбор сохраняется и не определяется повторно на каждом старте. exact `pt-BR` maps to `pt-BR`, exact `pt-PT` maps to `pt-PT`, and bare/other `pt-*` remains `pt-BR` for backward-compatible MVP behavior. Labels/status/defaults WPF идут через App localization catalogs. Смена языка обновляет untouched/default menu title, но не переводит custom menu title, entries, folder names или group names. Новые entries после смены языка используют текущий UI language. UI честно предупреждает, что некоторые изменения языка могут применяться после перезапуска.

Settings window должен оставаться пригодным для будущих секций настроек. Окно resizable; содержимое настроек разделено на category tabs, а footer actions `Сохранить`/`Закрыть` закреплены снизу и не прокручиваются. Vertical scroll допустим внутри конкретной вкладки, если её содержимое не помещается, но SettingsWindow не должен снова превращаться в одну длинную scroll-простыню. `Сохранить` относится к language/settings save. Explorer menu actions и reset выполняются immediately через отдельные кнопки и не являются staged changes. Проверка открытия modal settings window выполняется вручную; автоматические UIAutomation-клики не используются как критерий acceptance для custom-chrome/modal WPF.

SettingsWindow имеет практический minimum width для вкладок и RU/EN action labels. Tab headers are content-sized and may wrap as a header row instead of clipping long localized category labels. Tab bodies start from a consistent left/top inset rather than centering forms or warning cards in the available space. Если action labels становятся длиннее в других локалях, кнопки должны измеряться по содержимому и переноситься через wrapping action row, а не clip-иться. Installation path rows держат path text в `*` content column, а Open/Copy actions в `Auto` column; длинный path может wrap/trim через tooltip, но кнопки не должны вытесняться.

Installation path rows use short visible `Open` / `Copy` labels. For command-host path, `Open` still opens the containing folder; tooltip text explains this without making the visible button label long.

Help/About window является короткой встроенной справкой, а не полноценным help center. Оно открывается из SettingsWindow, использует resizable WPF Window, scrollable content and fixed close footer, и объясняет: что делает Foldora, базовые шаги создания entry, выбор `.ico`, где появляется legacy Explorer menu, что `Foldora.MenuHost.exe` не является сервисом/background helper, где лежат installed binaries/user data, как работает uninstall and why user data/icons are kept by default.

Visual polish v1 сохраняет этот UX flow без новых функций: MainWindow получает более спокойный editor header/status/empty/footer rhythm, SettingsWindow получает ровные секции/path rows/danger presentation, HelpWindow получает shared header/section rhythm и читаемые step rows. Это не меняет staged-save, immediate Settings actions, shell integration behavior или settings JSON.

Manual locale spot-check после catalog expansion: Ukrainian, Japanese and German UI were manually checked without blocking layout issues. RU/EN остаются primary verified locales; остальные enabled locales catalog-complete and test-covered, а translation/layout polish принимается как future feedback work.

## UI Audit Follow-Up

Documentation-first audit is tracked in `docs/UI_AUDIT.md`. Known follow-up items:

- redundant settings entry point on MainWindow - addressed by removing the content-area action;
- raw unsaved-changes boolean text - addressed with localized saved/unsaved status;
- initial button ergonomics pass for localized labels - addressed in shared action button style;
- SettingsWindow scrollbar/content gutter - addressed;
- SettingsWindow clarity pass - addressed: explicit Foldora Explorer menu status, Preview changes naming, tooltip help and path Open/Copy actions;
- Settings help/layout regression - addressed: passive non-button `?` glyphs, wrapped long tooltips and compact inline action buttons for Settings rows;
- button/window sizing regression - addressed: Settings inline buttons have more horizontal padding and MainWindow/SettingsWindow minimum widths prevent known broken narrow layouts;
- Help/About foundation - addressed: SettingsWindow now opens a small localized Help/About window for longer instructions;
- visual polish pass v1 - addressed for current MVP windows and shared WPF resources;
- SettingsWindow tabbed layout cleanup - addressed: settings categories are tabs, danger reset is isolated, and path row actions use short Open/Copy labels;
- Settings layout robustness pass - addressed: shared button template applies padding/alignment, Settings minimum width protects normal tab/action layout, and Explorer/path actions no longer rely on fixed small widths;
- Settings tab header clipping - addressed: tab headers use content-sized wrapping layout, separate from the action-button clipping fix;
- Settings tab body alignment - addressed: Application/Help/Danger content is left/top aligned, not centered in the tab body;
- app/window/exe icon foundation - addressed with a self-authored folded blue/cyan project asset; README hero and broader branding remain future work;
- treat further visual polish as feedback-driven, especially for long labels and non-Latin font fallback.

This audit does not change the staged-save model, Explorer integration behavior, registry safety or SettingsWindow immediate-action semantics.

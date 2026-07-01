# Menu Model

Пользовательские menu entries - главный MVP-объект Foldora. Пользователь сам выбирает любые `.ico` и любые подписи; Foldora запрещает только технически опасные значения, которые ломают файловую систему, настройки, реестр или Проводник. Продуктовая концепция подробнее описана в `PRODUCT_VISION.md`.

Foldora не запрещает пользователю делать странное меню, но запрещает то, что ломает файловую систему, настройки приложения, реестр или Проводник.

## Entry Fields

- `Id` - стабильный технический идентификатор. Не зависит от `DisplayName`.
- `DisplayName` - подпись пункта в будущем контекстном меню.
- `DefaultFolderName` - имя папки, которую Foldora создаст при выборе пункта.
- `GroupName` - optional visible name одноуровневой группы/submenu.
- `IconPath` - копия `.ico` внутри `%AppData%\Foldora\icons`.
- `PreviewPath` - optional/future preview.
- `SortOrder` - порядок пункта.
- `IsEnabled` - скрыть пункт без удаления.

`DisplayName`, `DefaultFolderName` и `GroupName` не смешиваются. `DisplayName` и `GroupName` нельзя использовать как id, имя файла или registry key. Дубликаты `DisplayName` и одинаковые `GroupName` разрешены.

Localization rule: custom saved menu title, entry display names, folder names and group names are user data and are not auto-translated when application language changes. Menu title also has a default-title mode tracked by `TitleIsCustom = false`; in that mode the title follows the current complete locale (`Создать папку` for `ru`, `Create folder` for `en`). WPF creates new draft entries with localized App-level defaults (`Вид N`/`Новая папка` for `ru`, `View N`/`New folder` for `en`). Core fallback defaults remain for compatibility/CLI safety and are not the preferred UI localization path.

Пример разрешённого пользовательского выбора:

```text
Skull
Skull
Skull
```

Сохранённые entries исполняются через CLI:

- `apply --folder "<folder>" --entry-id "<entry-id>"` применяет `IconPath` entry к существующей папке.
- `create --target "<directory>" --entry-id "<entry-id>"` создаёт новую папку с `DefaultFolderName` и применяет `IconPath`.

Entry должен быть enabled, иметь непустой `IconPath`, а импортированная иконка должна существовать.

## Explorer Menu Shape

Текущий MVP генерирует legacy Explorer menu в форме:

```text
<CreateFolderMenu.Title>
  <GroupName>
    <DisplayName grouped entry>
  <DisplayName entry 1>
  <DisplayName entry 2>
```

Если `CreateFolderMenu.Title` пустой или состоит только из пробелов, используется compatibility fallback `Создать папку`. В normal settings flow default-title mode сохраняет effective localized title, поэтому English default registry menu должен получать `Create folder`. Технический registry root при этом остаётся `Foldora`, чтобы safety validator мог разрешать только Foldora-owned paths:

```text
HKCU\Software\Classes\Directory\shell\Foldora
HKCU\Software\Classes\Directory\Background\shell\Foldora
```

Старый видимый слой `Foldora -> Создать папку -> entries` больше не используется в MVP. Root-level entries находятся напрямую под `...\Foldora\shell\entry-...`. Grouped entries находятся под техническим group key `...\Foldora\shell\group-NNN\shell\entry-...`. `DisplayName` и `GroupName` остаются только видимым текстом (`MUIVerb`) и никогда не используются как registry key path.

## Validation

`DisplayName`: trim по краям, после fallback непустой, максимум 80 символов, control chars запрещены, кириллица/emoji/пробелы разрешены, дубликаты разрешены.

`DefaultFolderName`: пустое значение получает fallback `Новая папка`, максимум 80 символов, control chars запрещены, запрещены Windows filename characters `< > : " / \ | ? *`, reserved names `CON`, `PRN`, `AUX`, `NUL`, `COM1`...`COM9`, `LPT1`...`LPT9`, а также trailing dot/space.

`GroupName`: trim по краям, empty/whitespace разрешены и означают root-level entry, максимум 80 символов, control chars запрещены, кириллица/emoji/пробелы разрешены. `/` и `\` запрещены, потому что nested groups вида `A/B` пока не поддерживаются.

Menu limits на текущем этапе: max total entries 100, max enabled entries 50, max groups 30, max enabled children per group 30.

При создании папки конфликт имён решается без перезаписи: `Name`, `Name (2)`, `Name (3)` и далее. Учитываются конфликты и с папками, и с файлами.

`FolderNameSanitizer` существует только для будущего WPF input/paste convenience. Он не заменяет validator; перед сохранением validator всё равно проверяет данные.

Core validators возвращают stable issue codes и параметры, а не выбирают UI language. Примеры параметров: invalid `character`, `maxLength`, `actualLength`, `reservedName`, `groupName`, `filePath`, `extension`, `limit`, `count`. WPF рендерит эти issues через App localization catalogs. Core `Message` остаётся compatibility/debug fallback для CLI и исключений.

## Icon Validation

Foldora принимает только настоящие `.ico`: файл должен существовать, читаться, быть не пустым, быть не больше 10 MB и иметь корректную ICO-структуру header/directory. PNG не конвертируется и не принимается даже при переименовании.

## Staged Save

WPF editor не должен применять изменения сразу:

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
      -> if ExplorerIntegrationEnabled was true: rebuild HKCU Foldora menu
      -> show success/error
```

При добавлении/удалении элементов в UI registry не трогать. Registry меняется только по явным actions: `Сохранить`, если integration уже была включена, или отдельные integration buttons.

Phase 2 WPF editor добавил draft-состояние для title, entries, add/remove и pending icon source. Выбранный `.ico` хранится как pending source path и не становится постоянным `IconPath` до `Save`. Save импортирует pending icons в `%AppData%\Foldora\icons\<entry-id>.ico` и сохраняет `settings.json`.

Phase 3 WPF editor показывает preview напрямую из pending source path или saved `IconPath`. Preview не меняет модель entries, не пишет `PreviewPath` и не создаёт файлы в `%AppData%\Foldora\previews`.

Phase 4 WPF editor добавляет explicit Explorer integration controls. В текущем Settings UI действие `Предпросмотр изменений` соответствует dry-run preview и вместе с `Включить меню Проводника` требует clean draft, чтобы registry строился только из saved settings. `Отключить меню Проводника` можно выполнять при unsaved draft changes; оно не удаляет entries. `Сбросить меню` требует подтверждения, очищает saved entries, возвращает title к `Создать папку`, отключает integration и не удаляет imported `.ico`.

После UX hardening WPF `Сохранить` rebuild-ит Foldora-owned registry menu, если `ExplorerIntegrationEnabled` уже был `true`. Это не нарушает staged-save: registry меняется только по явному нажатию Save, а не при редактировании полей. Если enabled entries после save нет, menu roots удаляются и `ExplorerIntegrationEnabled` сохраняется как `false`.

Удаление entry в WPF phase 2 не удаляет импортированный `.ico`; orphan icon cleanup является отдельной будущей задачей.

Если отдельный registry operation упадёт после registry write, но до сохранения settings, пользователь должен получить понятную ошибку. Сложный rollback settings из-за registry failure не нужен; safety boundary обеспечивается plan validation и Foldora-owned roots.

## One-level Grouping

Текущий grouping MVP реализован через flat поле `FolderMenuEntry.GroupName`. Это покрывает базовый UX групп без немедленного перехода на полноценную tree-модель:

```text
Create folder
  Colors
    Blue
    Red
  Work
    Documents
  Media
    Music
    Pictures
```

Пустое значение оставляет пункт прямо в root menu. Непустое значение объединяет entries с одинаковым `GroupName` в один submenu.

WPF может показывать entries как visual group containers `Без группы` и `<GroupName>`, но это не меняет persisted model. Пустые группы не сохраняются отдельно; `+ Добавить группу` в UI создаёт обычный draft entry с новым `GroupName`.

`Без группы` - root-section для entries с empty/whitespace `GroupName`, не удаляемая group entity. Удаление non-empty group в UI означает staged удаление всех entries с этим `GroupName`. Переименование group container означает staged обновление `GroupName` у всех entries этой группы.

Entry count в header group container является presentation state. Он вычисляется из entries и не сохраняется в Core model или `settings.json`.

Full tree storage остаётся future:

```text
Create folder
  Colors
    Red
    Blue
  Work
    Documents
    Reports
  Media
  Photos
```

Предварительные ограничения tree-модели: max depth after root menu title = 2, max children per group = 30, max total nodes = 100, max enabled create entries = 50.

Будущий HKCU context menu должен вызывать уже существующие CLI-команды `create --entry-id` и `apply --entry-id`.

Текущий registry plan builder поддерживает flat entries plus one-level groups. Будущая nested model должна заменить flat source на tree source и строить submenu tree с теми же safety limits.

## Registry Safety

Будущий registry writer:

- пишет только в HKCU;
- пишет только в свои ключи;
- удаляет только свои ключи;
- не трогает чужие shell entries;
- не пишет в HKLM;
- не требует admin;
- строится через testable registry plan builder;
- plan builder должен гарантировать, что все keys находятся только под Foldora-owned paths.

Registry writer реализован как отдельный слой и применяет только validated plan. `register-menu --dry-run` строит и валидирует plan, но не пишет и не удаляет реальные registry keys.

Registry entry может иметь `Icon = <entry.IconPath>`, если imported `.ico` существует. Это small shell icon для legacy menu. `DisplayName` никогда не используется как icon path.

`unregister-menu` удаляет только Foldora-owned registry roots и отключает Explorer integration без удаления entries/settings. `menu reset --yes` очищает entries, возвращает title к `Создать папку`, ставит `ExplorerIntegrationEnabled = false` и удаляет только Foldora-owned registry roots. Импортированные `.ico` остаются в `%AppData%\Foldora\icons`; очистка orphan icons отложена.

Будущие owned paths:

```text
HKCU\Software\Classes\Directory\Background\shell\Foldora
HKCU\Software\Classes\Directory\shell\Foldora
```

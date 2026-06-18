# Menu Model

Пользовательские menu entries - главный MVP-объект Foldora. Пользователь сам выбирает любые `.ico` и любые подписи; Foldora запрещает только технически опасные значения, которые ломают файловую систему, настройки, реестр или Проводник. Продуктовая концепция подробнее описана в `PRODUCT_VISION.md`.

Foldora не запрещает пользователю делать странное меню, но запрещает то, что ломает файловую систему, настройки приложения, реестр или Проводник.

## Entry Fields

- `Id` - стабильный технический идентификатор. Не зависит от `DisplayName`.
- `DisplayName` - подпись пункта в будущем контекстном меню.
- `DefaultFolderName` - имя папки, которую Foldora создаст при выборе пункта.
- `IconPath` - копия `.ico` внутри `%AppData%\Foldora\icons`.
- `PreviewPath` - optional/future preview.
- `SortOrder` - порядок пункта.
- `IsEnabled` - скрыть пункт без удаления.

`DisplayName` и `DefaultFolderName` не смешиваются. `DisplayName` нельзя использовать как id, имя файла или registry key. Дубликаты `DisplayName` разрешены.

Пример разрешённого пользовательского выбора:

```text
Череп
Череп
Череп
```

Сохранённые entries исполняются через CLI:

- `apply --folder "<folder>" --entry-id "<entry-id>"` применяет `IconPath` entry к существующей папке.
- `create --target "<directory>" --entry-id "<entry-id>"` создаёт новую папку с `DefaultFolderName` и применяет `IconPath`.

Entry должен быть enabled, иметь непустой `IconPath`, а импортированная иконка должна существовать.

## Explorer Menu Shape

Текущий MVP генерирует legacy Explorer menu в форме:

```text
<CreateFolderMenu.Title>
  <DisplayName entry 1>
  <DisplayName entry 2>
```

Если `CreateFolderMenu.Title` пустой или состоит только из пробелов, используется fallback `Создать папку`. Технический registry root при этом остаётся `Foldora`, чтобы safety validator мог разрешать только Foldora-owned paths:

```text
HKCU\Software\Classes\Directory\shell\Foldora
HKCU\Software\Classes\Directory\Background\shell\Foldora
```

Старый видимый слой `Foldora -> Создать папку -> entries` больше не используется в MVP. Entries находятся напрямую под `...\Foldora\shell\entry-...`. `DisplayName` остаётся только видимым текстом (`MUIVerb`) и никогда не используется как registry key path.

## Validation

`DisplayName`: trim по краям, после fallback непустой, максимум 80 символов, control chars запрещены, кириллица/emoji/пробелы разрешены, дубликаты разрешены.

`DefaultFolderName`: пустое значение получает fallback `Новая папка`, максимум 80 символов, control chars запрещены, запрещены Windows filename characters `< > : " / \ | ? *`, reserved names `CON`, `PRN`, `AUX`, `NUL`, `COM1`...`COM9`, `LPT1`...`LPT9`, а также trailing dot/space.

Flat menu limits на текущем этапе: max total entries 100, max enabled entries 50.

При создании папки конфликт имён решается без перезаписи: `Name`, `Name (2)`, `Name (3)` и далее. Учитываются конфликты и с папками, и с файлами.

`FolderNameSanitizer` существует только для будущего WPF input/paste convenience. Он не заменяет validator; перед сохранением validator всё равно проверяет данные.

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
      -> if explorerIntegrationEnabled: rebuild HKCU Foldora menu
      -> show success/error
```

При добавлении/удалении элементов в UI registry не трогать. Registry перестраивать только по явному `Сохранить` или отдельному integration action.

Phase 2 WPF editor использует draft-состояние для title, entries, add/remove и pending icon source. Выбранный `.ico` хранится как pending source path и не становится постоянным `IconPath` до `Save`. Save импортирует pending icons в `%AppData%\Foldora\icons\<entry-id>.ico`, сохраняет `settings.json` и не перестраивает registry menu.

Удаление entry в WPF phase 2 не удаляет импортированный `.ico`; orphan icon cleanup является отдельной будущей задачей.

Если registry rebuild в будущем упадёт: `Настройки сохранены, но меню Проводника не обновлено.` Не делать сложный rollback settings из-за registry failure.

## Future Nested Menu

Будущая модель должна поддержать flat entries и группы:

```text
Создать папку
  Цветные
    Красная
    Синяя
  Готические
    Череп
    Скелет
  Череп
  Фото
```

Предварительные ограничения tree-модели: max depth after root `Создать папку` = 2, max children per group = 30, max total nodes = 100, max enabled create entries = 50.

На текущем этапе storage остаётся flat; nested runtime/storage migration не реализована.

Будущий HKCU context menu должен вызывать уже существующие CLI-команды `create --entry-id` и `apply --entry-id`.

Текущий registry plan builder поддерживает только flat entries. Будущая nested model должна заменить flat source на tree source и строить submenu tree с теми же safety limits.

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

`unregister-menu` удаляет только Foldora-owned registry roots и отключает Explorer integration без удаления entries/settings. `menu reset --yes` очищает entries, возвращает title к `Создать папку`, ставит `ExplorerIntegrationEnabled = false` и удаляет только Foldora-owned registry roots. Импортированные `.ico` остаются в `%AppData%\Foldora\icons`; очистка orphan icons отложена.

Будущие owned paths:

```text
HKCU\Software\Classes\Directory\Background\shell\Foldora
HKCU\Software\Classes\Directory\shell\Foldora
```

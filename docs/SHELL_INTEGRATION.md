# Shell Integration

MVP использует legacy context menu через HKCU:

```text
HKCU\Software\Classes\Directory\shell
HKCU\Software\Classes\Directory\Background\shell
```

HKLM не используется. Регистрация и удаление меню должны быть явными командами пользователя.

Текущий MVP-вид меню:

```text
<CreateFolderMenu.Title>
  <GroupName>
    <DisplayName grouped entry>
  <DisplayName entry 1>
  <DisplayName entry 2>
```

Fallback title для пустого/whitespace `CreateFolderMenu.Title`: `Создать папку`. Старый видимый верхний слой `Foldora` больше не используется в MVP, но technical registry key остаётся `Foldora`.

`Foldora.Shell` содержит безопасный skeleton registrar, testable registry plan builder и HKCU writer, который применяет только validated plan.

Будущий HKCU context menu должен вызывать существующие CLI-команды:

```text
foldora create --target "<directory>" --entry-id "<entry-id>"
foldora apply --folder "<folder>" --entry-id "<entry-id>"
foldora clear --folder "<folder>"
```

## Registry Plan Builder

Builder поддерживает два target kind:

- `Directory` -> owned root `HKCU\Software\Classes\Directory\shell\Foldora`.
- `DirectoryBackground` -> owned root `HKCU\Software\Classes\Directory\Background\shell\Foldora`.

Текущая one-level menu shape:

```text
Создать папку
  Цветные
    Синяя
    Красная
  <DisplayName entry 1>
  <DisplayName entry 2>
```

Registry shape сохраняет owned root `Foldora`:

```text
Software\Classes\Directory\shell\Foldora
  MUIVerb = <CreateFolderMenu.Title>
  SubCommands = ""

Software\Classes\Directory\shell\Foldora\shell\entry-001-<entry-id>
  MUIVerb = <DisplayName>
  Icon = <entry.IconPath>

Software\Classes\Directory\shell\Foldora\shell\group-001
  MUIVerb = <GroupName>
  SubCommands = ""

Software\Classes\Directory\shell\Foldora\shell\group-001\shell\entry-001-<entry-id>
  MUIVerb = <DisplayName>
  Icon = <entry.IconPath>
```

Для `Directory\Background` используется такой же layout под `Software\Classes\Directory\Background\shell\Foldora`. Промежуточный ключ `create-folder` больше не создаётся.

`Icon` пишется только если `entry.IconPath` непустой и файл существует. Это маленькая shell-иконка legacy menu, не WPF preview 50x50 и не generated preview file.

Disabled entries не попадают в plan. Duplicate `DisplayName` и duplicate `GroupName` разрешены. `DisplayName` и `GroupName` используются только как menu text value (`MUIVerb`) и не используются как registry key name или `Icon` value. Entry key names строятся по sort index и stable `entry-id`; group key names строятся как technical `group-NNN`.

Group ordering deterministic: groups сортируются по минимальному `SortOrder` среди enabled entries в группе, затем по normalized group name. Entries внутри группы сортируются по `SortOrder`, затем по `Id`. Root-level entries сортируются по `SortOrder`, затем по `Id`.

Если enabled entries нет, builder строит только delete operation для Foldora-owned root. Это безопаснее, чем оставлять пустое активное submenu в Explorer.

Command values вызывают no-console MenuHost:

```text
"<Foldora.MenuHost.exe>" create --target "<placeholder>" --entry-id "<entry-id>"
```

Путь к executable host, shell target placeholder и `entry-id` quote-ятся через `CommandLineQuoter`. `Foldora.Cli.exe` остаётся console tool для ручных команд; Explorer context menu должен использовать `Foldora.MenuHost.exe`, чтобы не мигало console window.

Placeholder policy на текущем этапе:

- `Directory` -> `%1`.
- `DirectoryBackground` -> `%V`.

Это documented placeholder policy для тестов plan builder. Manual verification на Windows 11 требуется всякий раз, когда эта policy меняется.

## Desktop Placement Limitation

Legacy registry context menu передаёт Foldora target directory path (`%1` или `%V`), но не передаёт cursor coordinates или координаты desktop icon-view. Поэтому при создании папки из desktop background menu Foldora создаёт папку в правильной target directory, но позицию нового значка на рабочем столе выбирает Explorer.

Создание папки именно под курсором не поддерживается текущей MVP-интеграцией. Для такого поведения нужен отдельный advanced shell integration path, например `IExplorerCommand`, COM shell extension, работа с Explorer view positioning или другой глубокий shell layer. Это future/non-MVP и не должно решаться registry/placeholder hacks.

## Folder Icon Attribute Policy

Explorer menu commands в итоге вызывают `DesktopIniService` через Core action services. Для новых папок используется default `DesktopIniAttributePolicy.ReadOnlyFolderHiddenDesktopIni`: folder получает `ReadOnly`, а `desktop.ini` получает только `Hidden`.

Этот default выбран после ручной проверки Windows 11, потому что `System` на папке или `desktop.ini` вызывает плохой deletion UX. Старые папки, созданные прежней policy `CompatibilitySystem`, автоматически не исправляются и могут сохранять warning при удалении.

Ручная проверка default policy подтвердила:

```text
folder attrib: R
desktop.ini attrib: H
custom icon survives Explorer refresh/reopen
deletion warning caused by System attributes is gone for new folders
```

## Registry Writer

`ExplorerMenuRegistryWriter` всегда валидирует plan перед применением. Writer пишет только через `IRegistryAccess`; реальный доступ к Windows Registry находится только в `WindowsRegistryAccess`.

`register-menu`:

1. Загружает settings и создаёт их при необходимости.
2. Строит plan для `Directory` и `DirectoryBackground`.
3. Валидирует plan.
4. Применяет plan в HKCU.
5. Ставит `ExplorerIntegrationEnabled = true`, если есть enabled entries.

Если enabled entries нет, пустое меню не создаётся: writer удаляет Foldora-owned roots и сохраняет `ExplorerIntegrationEnabled = false`.

`register-menu --dry-run` строит и валидирует plan, печатает delete/create/set операции, но не пишет в registry и не меняет settings.

`register-menu --host-path "<absolute-path-to-Foldora.MenuHost.exe>"` задаёт executable host для registry commands. Старый `--cli-path` сохранён как legacy/dev override, но для Explorer UX предпочтителен `--host-path`.

`unregister-menu` удаляет только:

```text
HKCU\Software\Classes\Directory\Background\shell\Foldora
HKCU\Software\Classes\Directory\shell\Foldora
```

Команда idempotent: отсутствие ключей не является ошибкой.

`unregister-menu` является безопасным отключением Explorer integration: settings entries сохраняются, `ExplorerIntegrationEnabled` становится `false`, чужие registry keys не трогаются.

`menu reset --yes` выполняет полный сброс пользовательского меню к пустому дефолту: удаляет только Foldora-owned registry roots, очищает `CreateFolderMenu.Entries`, возвращает title к `Создать папку`, ставит `ExplorerIntegrationEnabled = false` и сохраняет `settings.json`. AppData root, `packs` и импортированные `.ico` на этом шаге не удаляются.

Отсутствие Foldora menu в Explorer является нормальным дефолтным состоянием.

## WPF Explorer Integration Controls

WPF phase 4 использует тот же `ExplorerMenuRegistrationService`, что и CLI, через App-level controller. Это не вторая registry implementation.

`Проверить план` соответствует `register-menu --dry-run`: строит и валидирует plan, показывает summary delete/create/set operations, Foldora-owned roots и пример command value. Registry и settings не меняются.

`Включить меню Проводника` соответствует `register-menu`: требует отсутствия unsaved draft changes, строит validated plan из saved settings, применяет HKCU writer и сохраняет `ExplorerIntegrationEnabled = true`, если есть enabled entries. Если enabled entries нет, пустое menu не создаётся: Foldora-owned roots удаляются, а `ExplorerIntegrationEnabled = false`.

`Отключить меню Проводника` соответствует `unregister-menu`: удаляет только Foldora-owned roots, сохраняет entries/title и ставит `ExplorerIntegrationEnabled = false`. Operation idempotent и разрешена даже при unsaved draft changes.

`Сбросить меню` соответствует `menu reset --yes` после UI confirmation: очищает entries, возвращает title к `Создать папку`, ставит `ExplorerIntegrationEnabled = false`, удаляет только Foldora-owned roots и не удаляет AppData root, `settings.json`, packs или imported `.ico`.

WPF `Сохранить` rebuild-ит registry menu только если `ExplorerIntegrationEnabled` уже был `true`. Если integration disabled, Save пишет только settings. Если rebuild после settings save упал, settings не откатываются, UI показывает `Настройки сохранены, но меню Проводника не обновлено.`

## Manual Verification

1. Добавить тестовый entry:

```text
foldora menu add --icon "<path-to-test.ico>" --name "Череп" --folder-name "Череп"
```

2. Посмотреть entries:

```text
foldora menu list
```

3. Сначала проверить plan:

```text
foldora register-menu --dry-run
```

4. Зарегистрировать menu:

```text
foldora register-menu
```

Если текущий exe path не совпадает с будущим install/publish path, использовать:

```text
foldora register-menu --host-path "<absolute-path-to-Foldora.MenuHost.exe>"
```

`--cli-path` можно использовать как legacy/dev override, но тогда Explorer будет запускать console app и может кратко показать console window.

5. Проверить в Windows 11 Explorer:

- ПКМ по папке.
- ПКМ по пустому месту внутри директории.
- `Show more options`, если пункт попал в legacy menu.

6. После ручной проверки или если target placeholder работает неправильно:

```text
foldora unregister-menu
```

7. Для полного сброса пользовательского меню:

```text
foldora menu reset --yes
```

8. Зафиксировать фактическое поведение `%1` и `%V`.

Будущие registry safety rules:

- HKCU only.
- Foldora-owned roots only.
- No HKLM.
- No admin required.
- No cleaning whole `Directory\shell`.
- No touching other programs.
- `DisplayName` never used as registry key.
- Registry keys for entries are technical and based on sort/index/id.
- Writer applies only validated plan.
- Tests must use fake/in-memory registry.
- Real registry access only through `WindowsRegistryAccess`.
- `unregister-menu` must remain idempotent.

Будущие owned paths:

```text
HKCU\Software\Classes\Directory\Background\shell\Foldora
HKCU\Software\Classes\Directory\shell\Foldora
```

Не делать на текущем этапе: COM shell extension, `IExplorerCommand`, sparse package, MSIX и modern Windows 11 compact context menu integration.

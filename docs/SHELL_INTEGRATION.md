# Shell Integration

MVP использует legacy context menu через HKCU:

```text
HKCU\Software\Classes\Directory\shell
HKCU\Software\Classes\Directory\Background\shell
```

HKLM не используется. Регистрация и удаление меню должны быть явными командами пользователя.

Целевой вид меню:

```text
Foldora
  Create folder
    Documents
    Code
    Photos
    Archive
    Custom...
  Apply icon
    Documents
    Code
    Photos
    Archive
    Custom...
  Clear icon
  Settings
```

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

Текущая flat menu shape:

```text
Foldora
  Создать папку
    <DisplayName entry 1>
    <DisplayName entry 2>
```

Disabled entries не попадают в plan. Duplicate `DisplayName` разрешены. `DisplayName` используется только как menu text value (`MUIVerb`) и не используется как registry key name. Entry key names строятся по sort index и stable `entry-id`.

Если enabled entries нет, builder строит только delete operation для Foldora-owned root. Это безопаснее, чем оставлять пустое активное submenu в Explorer.

Command values вызывают существующий CLI:

```text
"<Foldora.Cli.exe>" create --target "<placeholder>" --entry-id "<entry-id>"
```

Путь к exe, shell target placeholder и `entry-id` quote-ятся через `CommandLineQuoter`.

Placeholder policy на текущем этапе:

- `Directory` -> `%1`.
- `DirectoryBackground` -> `%V`.

Это documented placeholder policy для тестов plan builder. Реальная корректность `%1`, `%V`, `%L` и других Explorer placeholders требует ручной проверки на Windows 11 перед включением registry writer.

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

`unregister-menu` удаляет только:

```text
HKCU\Software\Classes\Directory\Background\shell\Foldora
HKCU\Software\Classes\Directory\shell\Foldora
```

Команда idempotent: отсутствие ключей не является ошибкой.

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
foldora register-menu --cli-path "<absolute-path-to-Foldora.Cli.exe>"
```

5. Проверить в Windows 11 Explorer:

- ПКМ по папке.
- ПКМ по пустому месту внутри директории.
- `Show more options`, если пункт попал в legacy menu.

6. Если target placeholder работает неправильно:

```text
foldora unregister-menu
```

7. Зафиксировать фактическое поведение `%1` и `%V`.

Будущие registry safety rules:

- Foldora пишет только в HKCU.
- Foldora пишет только в свои ключи.
- Foldora удаляет только свои ключи.
- Foldora не трогает чужие shell entries.
- Foldora не пишет в HKLM.
- Foldora не требует admin.
- Перед registry writer нужен testable registry plan builder.
- Plan builder должен гарантировать, что все keys находятся только под Foldora-owned paths.

Будущие owned paths:

```text
HKCU\Software\Classes\Directory\Background\shell\Foldora
HKCU\Software\Classes\Directory\shell\Foldora
```

Не делать на текущем этапе: COM shell extension, `IExplorerCommand`, sparse package, MSIX и modern Windows 11 compact context menu integration.

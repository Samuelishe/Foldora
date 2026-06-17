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

`Foldora.Shell` содержит безопасный skeleton registrar и testable registry plan builder. Реальные registry writes будут добавлены отдельным этапом.

Будущий HKCU context menu должен вызывать существующие CLI-команды:

```text
foldora create --target "<directory>" --entry-id "<entry-id>"
foldora apply --folder "<folder>" --entry-id "<entry-id>"
foldora clear --folder "<folder>"
```

## Registry Plan Builder

Текущий шаг строит только plan, но не пишет в реестр. Builder поддерживает два target kind:

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

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

Bootstrap содержит `Foldora.Shell` и безопасный skeleton registrar. Реальные registry writes будут добавлены отдельным этапом.

Будущий HKCU context menu должен вызывать существующие CLI-команды:

```text
foldora create --target "<directory>" --entry-id "<entry-id>"
foldora apply --folder "<folder>" --entry-id "<entry-id>"
foldora clear --folder "<folder>"
```

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

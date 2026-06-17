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

Не делать на текущем этапе: COM shell extension, `IExplorerCommand`, sparse package, MSIX и modern Windows 11 compact context menu integration.

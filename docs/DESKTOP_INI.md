# Desktop.ini

MVP применяет иконку папки через `desktop.ini` внутри целевой папки.

Формат:

```ini
[.ShellClassInfo]
IconResource=<absolute-path-to-icon>,0
```

Алгоритм `apply`:

1. Создать или обновить `desktop.ini`.
2. Записать секцию `[.ShellClassInfo]`.
3. Записать `IconResource=<absolute-path-to-icon>,0`.
4. Поставить атрибуты `desktop.ini`: Hidden + System.
5. Поставить атрибут System целевой папке.

Это текущая default policy `CompatibilitySystem`. Она оставлена без изменения, потому что уже подтверждена как рабочая для custom folder icon. При этом ручная проверка показала плохой deletion UX: Windows может предупреждать, что файл `desktop.ini` является системным, если пользователь удаляет созданную Foldora папку.

## Attribute Policies

Foldora теперь имеет тестируемую модель `DesktopIniAttributePolicy`, чтобы проверить deletion-friendly альтернативы без слепой смены production default.

Поддерживаемые policies:

| Policy | Folder attributes | desktop.ini attributes | Icon visible | Delete warning | Notes |
| --- | --- | --- | --- | --- | --- |
| `CompatibilitySystem` | System | Hidden + System | TBD | TBD | current default |
| `ReadOnlyFolderSystemDesktopIni` | ReadOnly | Hidden + System | TBD | TBD | candidate |
| `ReadOnlyFolderHiddenDesktopIni` | ReadOnly | Hidden | TBD | TBD | candidate |
| `SystemFolderHiddenDesktopIni` | System | Hidden | TBD | TBD | candidate |

Результаты `Icon visible` и `Delete warning` намеренно оставлены `TBD`: Codex не может надёжно проверить Explorer UI warning автоматическими тестами. Лучший deletion-friendly default нужно выбрать после ручной проверки на Windows 11.

Manual diagnostic command:

```text
foldora diagnostics desktop-ini-policy --target "<directory>" --icon "<ico>"
```

Команда создаёт внутри `--target` по одной папке на policy:

```text
Foldora Policy Test - CompatibilitySystem
Foldora Policy Test - ReadOnly SystemIni
Foldora Policy Test - ReadOnly HiddenIni
Foldora Policy Test - System HiddenIni
```

Manual checklist:

1. Посмотреть, появилась ли кастомная иконка.
2. Обновить Explorer / F5.
3. Закрыть и открыть папку.
4. Попробовать удалить каждую тестовую папку.
5. Зафиксировать, где появляется warning про системный `desktop.ini`.

Diagnostic command ничего не пишет в registry, не требует admin rights и не трогает папки вне явно указанного `--target`.

Алгоритм `clear`:

1. Проверить, что целевая папка существует.
2. Удалить строку `IconResource` из секции `[.ShellClassInfo]`.
3. Если после очистки `desktop.ini` не содержит полезных строк, удалить файл.
4. Если в `desktop.ini` есть другие секции или данные, сохранить их.
5. Не снимать агрессивно атрибут `System` с папки, потому что он может быть нужен другим shell-настройкам.

В MVP не делать агрессивный reset icon cache и не перезапускать Explorer по умолчанию. Explorer может не обновить иконку мгновенно из-за кэша.

Windows лучше работает с `.ico`, а не `.png`. `.ico` должен содержать размеры 16, 32, 48, 64, 128, 256 px. Нельзя переименовывать PNG в ICO. Explorer может кэшировать иконки, поэтому визуальное обновление не всегда мгновенное.

Сейчас реализована работа с прямым абсолютным путём к `.ico` и с сохранённым menu entry через `--entry-id`. `--style` и resolver стилей пока не реализованы.

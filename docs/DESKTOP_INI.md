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
4. Поставить атрибуты `desktop.ini`: Hidden.
5. Поставить атрибут ReadOnly целевой папке.

Это текущая default policy `ReadOnlyFolderHiddenDesktopIni`. Она выбрана после ручной проверки на Windows 11: custom icon остаётся видимой после refresh/reopen Explorer, а deletion warning, связанный с `System` attributes, исчезает для новых папок.

Старые папки, созданные прежней policy, могут иметь folder `System` и `desktop.ini` `Hidden + System`. Foldora не мигрирует и не чинит такие папки автоматически в этом шаге; они могут продолжать показывать warning при удалении. Отдельная repair/normalize command рассматривается как future work.

## Attribute Policies

Foldora теперь имеет тестируемую модель `DesktopIniAttributePolicy`, чтобы проверить deletion-friendly альтернативы без слепой смены production default.

Поддерживаемые policies:

| Policy | Folder attributes | desktop.ini attributes | Icon visible | Delete warning | Notes |
| --- | --- | --- | --- | --- | --- |
| `CompatibilitySystem` | System | Hidden + System | Yes | Yes | previous default; warning may mention `desktop.ini` or folder |
| `ReadOnlyFolderSystemDesktopIni` | ReadOnly | Hidden + System | Yes | Yes | `desktop.ini` still has System |
| `ReadOnlyFolderHiddenDesktopIni` | ReadOnly | Hidden | Yes | No | current default |
| `SystemFolderHiddenDesktopIni` | System | Hidden | Yes | Yes | folder still has System |

Результаты основаны на ручной проверке Windows 11. Вывод: `System` на папке даёт плохой deletion UX, и `System` на `desktop.ini` тоже нежелателен. Для новых Foldora-created folders используется `ReadOnlyFolderHiddenDesktopIni`.

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
5. Не снимать агрессивно атрибуты папки, потому что они могут быть нужны другим shell-настройкам или старой Foldora policy.

В MVP не делать агрессивный reset icon cache и не перезапускать Explorer по умолчанию. Explorer может не обновить иконку мгновенно из-за кэша.

Windows лучше работает с `.ico`, а не `.png`. `.ico` должен содержать размеры 16, 32, 48, 64, 128, 256 px. Нельзя переименовывать PNG в ICO. Explorer может кэшировать иконки, поэтому визуальное обновление не всегда мгновенное.

Сейчас реализована работа с прямым абсолютным путём к `.ico` и с сохранённым menu entry через `--entry-id`. `--style` и resolver стилей пока не реализованы.

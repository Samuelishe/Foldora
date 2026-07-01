# CLI

Целевые команды:

```text
foldora create --target "<directory>" --style "<style-id>"
foldora apply --folder "<folder>" --icon "<absolute-icon-path>"
foldora apply --folder "<folder>" --entry-id "<entry-id>"
foldora create --target "<directory>" --entry-id "<entry-id>"
foldora clear --folder "<folder>"
foldora menu list
foldora menu add --icon "<absolute-icon-path>" [--name "<display-name>"] [--folder-name "<default-folder-name>"] [--group "<group-name>"]
foldora menu remove --entry-id "<entry-id>"
foldora menu reset --yes
foldora import-pack --path "<pack-path>"
foldora list-packs
foldora list-styles
foldora register-menu
foldora register-menu --host-path "<absolute-path-to-Foldora.MenuHost.exe>"
foldora unregister-menu
foldora diagnostics desktop-ini-policy --target "<directory>" --icon "<absolute-icon-path>"
foldora settings
```

Реализовано сейчас:

- не падает без аргументов;
- показывает help;
- выполняет `apply --folder --icon` через `DesktopIniService`;
- выполняет `apply --folder --entry-id`, применяя иконку сохранённого enabled entry;
- выполняет `create --target --entry-id`, создавая папку с `DefaultFolderName` entry и применяя его иконку;
- выполняет `clear --folder` через `DesktopIniService`;
- выполняет `menu list`, создавая AppData/settings при первом запуске;
- выполняет `menu add --icon ... [--name ...] [--folder-name ...] [--group ...]`, импортируя `.ico` в `%AppData%\Foldora\icons`;
- выполняет `menu remove --entry-id ...`, удаляя entry и принадлежащую ему копию `.ico`;
- выполняет `menu reset --yes`, очищая пользовательские entries, возвращая title к `Создать папку`, удаляя только Foldora-owned registry roots и отключая Explorer integration;
- выполняет `register-menu`, применяя validated HKCU registry plan;
- выполняет `register-menu --dry-run`, печатая plan без записи в registry и без изменения settings;
- выполняет `register-menu --host-path "<path-to-Foldora.MenuHost.exe>"` для ручной проверки с publish/install path;
- выполняет `register-menu --cli-path "<path>"` как legacy/dev alias для старого console host behavior;
- выполняет `unregister-menu`, удаляя только Foldora-owned HKCU roots;
- выполняет `diagnostics desktop-ini-policy --target ... --icon ...`, создавая тестовые папки для ручной проверки desktop.ini attribute policies;
- для неготовых команд пишет понятное skeleton-сообщение;

Ограничения текущего CLI:

- `--style` пока не реализован.
- `import-pack` пока не реализован.
- Legacy registry context menu использует HKCU и Foldora-owned roots. Modern Windows 11 compact menu не реализован.
- При запуске через `dotnet run` путь текущего процесса может отличаться от publish path; для ручной проверки Explorer UX используйте `register-menu --host-path`.
- Explorer restart и icon cache reset не выполняются.
- Explorer может не обновить иконку мгновенно из-за кэша.

Если `--name` не указан или пустой, `menu add` использует первое свободное fallback-имя `Вид N`.
Если `--folder-name` не указан или пустой, используется `Новая папка`.
Если `--group` не указан или пустой, entry остаётся в root menu. Если `--group "Цветные"` указан, entry попадает в одноуровневую группу `Цветные`. `/` и `\` в group name запрещены, потому что nested groups пока не поддерживаются.
CLI не исправляет явно невалидный `--folder-name` молча: запрещённые Windows filename characters, reserved device names, trailing dot/space, control chars и слишком длинные значения дают понятную ошибку, entry не создаётся.

`menu list` показывает группу как `Group: <root>` или `Group: <group-name>`.

Для `apply` доступны два взаимоисключающих режима: `--icon` для прямого пути к `.ico` или `--entry-id` для сохранённого entry. Если указаны оба или не указан ни один, CLI возвращает ошибку.

`create --entry-id` создаёт папку внутри `--target`. Если имя занято файлом или папкой, используется схема `Name (2)`, `Name (3)` и так далее, максимум до разумного лимита попыток.

Обычные `apply` и `create` используют текущий default `desktop.ini` attribute policy из Core: `ReadOnlyFolderHiddenDesktopIni` (`folder: ReadOnly`, `desktop.ini: Hidden`). `System` не ставится по default ни на папку, ни на `desktop.ini`.

`unregister-menu` idempotent: отсутствие Foldora-owned keys не считается ошибкой.
`unregister-menu` не удаляет entries/settings и нужен для безопасного временного отключения Explorer integration.

`register-menu` по умолчанию пытается использовать `Foldora.MenuHost.exe` рядом с текущим executable или в соседнем Debug output. MenuHost - Windows-subsystem executable без console window; именно он должен быть target для Explorer legacy menu. `Foldora.Cli.exe` остаётся console app для ручных команд, diagnostics и разработки. `--cli-path` сохранён как backward-compatible override, но может вернуть console flash при запуске из Explorer.

Manual publish layout создаётся командой:

```text
pwsh scripts/publish-dev.ps1
```

После этого рядом лежат:

```text
artifacts/publish/Foldora/Foldora.App.exe
artifacts/publish/Foldora/Foldora.Cli.exe
artifacts/publish/Foldora/Foldora.MenuHost.exe
```

Для publish CLI registration:

```text
artifacts/publish/Foldora/Foldora.Cli.exe register-menu --host-path "<repo>\artifacts\publish\Foldora\Foldora.MenuHost.exe"
```

Перед удалением publish-папки:

```text
artifacts/publish/Foldora/Foldora.Cli.exe unregister-menu
```

`menu reset --yes` - полный сброс пользовательского меню к пустому дефолту. Команда не удаляет весь `%AppData%\Foldora`, не удаляет `settings.json`, не трогает `packs` и не удаляет импортированные `.ico` на этом шаге. Без `--yes` reset отказывается выполняться.

Будущее улучшение: `unregister-menu --dry-run`, который покажет удаляемые Foldora-owned roots без записи в registry и без изменения settings.

## Diagnostics

`diagnostics desktop-ini-policy` предназначена только для ручной проверки deletion UX в Explorer. Команда не пишет в registry, не использует AppData settings и не требует admin rights.

Пример:

```text
foldora diagnostics desktop-ini-policy --target "C:\Users\User\Desktop" --icon "C:\Icons\test.ico"
```

Она создаёт по одной папке на каждую policy:

- `CompatibilitySystem`
- `ReadOnlyFolderSystemDesktopIni`
- `ReadOnlyFolderHiddenDesktopIni`
- `SystemFolderHiddenDesktopIni`

После запуска нужно вручную проверить, видна ли кастомная иконка после refresh/reopen Explorer и появляется ли warning при удалении папки.

Текущий production default уже выбран: `ReadOnlyFolderHiddenDesktopIni`. Остальные policies оставлены для diagnostics и regression/manual verification. Команда diagnostics не выполняет repair/migration старых папок.

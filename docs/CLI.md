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
foldora diagnostics desktop-icon-position --name "<desktop item name>" --x <int> --y <int> [--coordinate-space screen|view]
foldora convert-icon --input "<image-path>" --output "<ico-path>" [--force]
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
- выполняет `diagnostics desktop-icon-position --name ... --x ... --y ... [--coordinate-space screen|view]`, prototype-попытку переместить уже существующий desktop item;
- выполняет `convert-icon --input "<image-path>" --output "<ico-path>" [--force]`, конвертируя один PNG/JPG/JPEG/BMP файл в multi-size `.ico`;
- для неготовых команд пишет понятное skeleton-сообщение;

Ограничения текущего CLI:

- `--style` пока не реализован.
- `import-pack` пока не реализован.
- Legacy registry context menu использует HKCU и Foldora-owned roots. Modern Windows 11 compact menu не реализован.
- При запуске через `dotnet run` путь текущего процесса может отличаться от publish path; для ручной проверки Explorer UX используйте `register-menu --host-path`.
- Explorer restart и icon cache reset не выполняются.
- Explorer может не обновить иконку мгновенно из-за кэша.
- `diagnostics desktop-icon-position` не используется Explorer menu и не получает исходные координаты right-click; это только ручная feasibility-проверка post-create positioning.
- `convert-icon` поддерживает только single-file conversion. Directory input/output, batch conversion, `--recursive`, custom frame sizes/filter/fit flags, SVG and AppData/generated-icon integration пока не реализованы.

Если `--name` не указан или пустой, `menu add` использует первое свободное fallback-имя `Вид N`.
Если `--folder-name` не указан или пустой, используется `Новая папка`.
Если `--group` не указан или пустой, entry остаётся в root menu. Если `--group "Colors"` указан, entry попадает в одноуровневую группу `Colors`. `/` и `\` в group name запрещены, потому что nested groups пока не поддерживаются.
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

Per-user install layout создаётся отдельно:

```text
pwsh scripts/install-user.ps1
```

После install CLI находится здесь:

```text
%LocalAppData%\Programs\Foldora\Foldora.Cli.exe
```

Installed app/CLI должны регистрировать Explorer menu с sibling host:

```text
%LocalAppData%\Programs\Foldora\Foldora.MenuHost.exe
```

Удаление installed binaries и Explorer menu:

```text
pwsh scripts/uninstall-user.ps1
```

Uninstall по default сохраняет `%AppData%\Foldora`; `-RemoveUserData` удаляет settings/imported icons/logs и может сломать custom icons уже созданных folders, если их `desktop.ini` ссылается на `%AppData%\Foldora\icons`.

`menu reset --yes` - полный сброс пользовательского меню к пустому дефолту. Команда не удаляет весь `%AppData%\Foldora`, не удаляет `settings.json`, не трогает `packs` и не удаляет импортированные `.ico` на этом шаге. Без `--yes` reset отказывается выполняться.

Будущее улучшение: `unregister-menu --dry-run`, который покажет удаляемые Foldora-owned roots без записи в registry и без изменения settings.

## Icon Conversion CLI

Implemented single-file command:

```powershell
Foldora.Cli.exe convert-icon --input ".\image.png" --output ".\folder.ico"
Foldora.Cli.exe convert-icon --input ".\image.png" --output ".\folder.ico" --force
```

Supported input formats:

- `.png`
- `.jpg`
- `.jpeg`
- `.bmp`

Output:

- multi-size `.ico`;
- default frames: `16`, `24`, `32`, `48`, `64`, `128`, `256`;
- non-square images use contain-fit with transparent padding;
- output is not overwritten unless `--force` is passed;
- the command writes to a temporary file in the output directory first, then moves it to the final `.ico` path.

Not implemented in this CLI step:

- batch/directory conversion;
- `--recursive`;
- custom sizes/filter/fit mode flags;
- SVG;
- WPF picker auto-conversion;
- drag image onto icon preview;
- AppData generated icon storage integration.

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

`diagnostics desktop-icon-position` предназначена только для ручной проверки, может ли Foldora попросить Explorer reposition уже существующий desktop item.

Пример:

```text
foldora diagnostics desktop-icon-position --name "Foldora Test" --x 100 --y 100 --coordinate-space screen
```

Параметры:

- `--name` - display name существующего desktop item/folder.
- `--x` и `--y` - целевая точка.
- `--coordinate-space screen` - входные координаты экрана; CLI попытается преобразовать их в desktop view coordinates.
- `--coordinate-space view` - координаты уже считаются desktop view coordinates.

Команда не создаёт папки, не пишет registry, не меняет settings, не регистрирует Explorer menu и не запускается из `Foldora.MenuHost.exe`. Она может вернуть controlled failure, если desktop view не найден, item не найден, Explorer отклонил positioning или layout policy вроде auto-arrange не позволяет перемещение.

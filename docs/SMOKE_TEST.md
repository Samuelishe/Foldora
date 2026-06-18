# Smoke Test

Практический checklist для ручной проверки Foldora MVP на Windows 11.

Не используйте локальные пользовательские пути в документации и issue reports. В примерах ниже используйте placeholders:

```text
<path-to-icon.ico>
<path-to-Foldora.MenuHost.exe>
```

## 1. Build/Test

```text
dotnet build Foldora.sln
dotnet test Foldora.sln
```

Ожидаемо:

- build проходит без ошибок;
- test suite проходит полностью.

## 2. WPF Startup

```text
dotnet run --project src/Foldora.App/Foldora.App.csproj
```

Ожидаемо:

- открывается окно;
- title окна: `Foldora`;
- приложение отвечает;
- settings gear открывает окно настроек;
- выбор языка `ru`/`en` сохраняется без изменения menu draft и registry.

Если startup падает до показа окна, проверить:

```text
%AppData%\Foldora\Logs\startup-error.log
```

## 3. Entry Editing

1. Добавить root entry.
2. Указать `Название в меню`.
3. Указать `Имя создаваемой папки`.
4. Оставить `Группа` пустой.
5. Выбрать `<path-to-icon.ico>`.
6. Убедиться, что direct `.ico` preview появился.
7. Добавить grouped entry.
8. Указать `Группа`, например `Цветные`.
9. Нажать `Сохранить`.
10. Изменить draft и нажать `Отменить изменения`.

Ожидаемо:

- root entry остаётся в корне меню;
- grouped entry попадает в одноуровневую группу;
- pending icon импортируется только после `Сохранить`;
- `Отменить изменения` откатывает draft fields, added/removed entries и pending icon selections.

## 4. Explorer Integration

1. Нажать `Проверить план`.
2. Проверить short status и раскрываемые technical details.
3. Нажать `Включить меню Проводника`.
4. Открыть Explorer.
5. Проверить legacy menu через `Show more options`, если пункт не виден в compact menu.

Ожидаемо:

```text
Создать папку
  Цветные
    Синяя
  Музыка
```

- root entry работает;
- grouped entry работает;
- small icons видны рядом с entries;
- при выборе entry не мигает console window, потому что Explorer запускает `Foldora.MenuHost.exe`.

Для CLI/manual flow:

```text
foldora register-menu --dry-run
foldora register-menu --host-path "<path-to-Foldora.MenuHost.exe>"
```

## 5. Folder Creation

1. Выбрать entry в legacy Explorer menu.
2. Проверить созданную папку.
3. Повторить выбор entry при уже существующей папке с тем же именем.

Ожидаемо:

- папка создаётся с `DefaultFolderName`;
- при конфликте имён создаётся `Name (2)`, затем `Name (3)`;
- custom folder icon появляется через `desktop.ini`;
- Explorer может обновить иконку не мгновенно из-за cache.

Проверить атрибуты:

```text
attrib "<created-folder>"
attrib "<created-folder>\desktop.ini"
```

Ожидаемо:

```text
folder attrib: R
desktop.ini attrib: H
```

Также проверить:

- icon survives Explorer refresh/reopen;
- удаление новой папки не показывает warning про системный `desktop.ini` или системную папку.

## 6. Save-triggered Rebuild

1. Включить Explorer integration.
2. Добавить новый entry или изменить `GroupName`.
3. Нажать `Сохранить`.
4. Проверить Explorer legacy menu.

Ожидаемо:

- меню Проводника обновилось после `Сохранить`;
- повторно нажимать `Включить меню Проводника` не нужно;
- если enabled entries стало 0, Foldora-owned roots удаляются и integration становится disabled.

## 7. Unregister/Reset

Проверить `Отключить меню Проводника` или CLI:

```text
foldora unregister-menu
```

Ожидаемо:

- legacy menu исчезает;
- entries/settings сохраняются;
- `ExplorerIntegrationEnabled = false`.

Проверить reset:

```text
foldora menu reset --yes
```

Ожидаемо:

- entries очищаются;
- title возвращается к `Создать папку`;
- legacy menu удаляется;
- `%AppData%\Foldora\settings.json` сохраняется;
- imported `.ico` остаются в `%AppData%\Foldora\icons`;
- packs не трогаются.

## 8. Known Manual Checks

- Modern Windows 11 compact context menu не реализован; legacy menu может быть под `Show more options`.
- Позицию нового значка на Desktop выбирает Explorer; Foldora не создаёт desktop folder строго под курсором.
- Registry menu в dev/manual проверке может указывать на Debug `Foldora.MenuHost.exe`; production publish должен дать стабильный installed path.
- User-facing diagnostics для failures внутри `Foldora.MenuHost` из Explorer menu пока future work.

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

## 1a. Manual Publish Layout

```text
pwsh scripts/publish-dev.ps1
```

Ожидаемо:

- script создаёт `artifacts/publish/Foldora`;
- script не регистрирует Explorer menu;
- script не запускает приложение;
- рядом лежат:

```text
artifacts/publish/Foldora/Foldora.App.exe
artifacts/publish/Foldora/Foldora.Cli.exe
artifacts/publish/Foldora/Foldora.MenuHost.exe
```

Для publish smoke:

1. Запустить `artifacts/publish/Foldora/Foldora.App.exe`.
2. Добавить или отредактировать entry.
3. Нажать `Сохранить`.
4. Нажать `Включить меню Проводника`.
5. Проверить, что registry command указывает на `artifacts/publish/Foldora/Foldora.MenuHost.exe`, а не на Debug output.
6. Проверить Explorer legacy menu через `Show more options`, если пункт не виден в compact menu.
7. Создать папку через Desktop background context menu.
8. Проверить `%AppData%\Foldora\Logs\menuhost-placement.log`; последняя запись должна соответствовать create-команде.
9. Перед удалением publish-папки выполнить `artifacts/publish/Foldora/Foldora.Cli.exe unregister-menu`.

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
2. Убедиться, что новый entry сразу открыт в edit mode.
3. Указать `Название в меню`.
4. Указать `Имя создаваемой папки`.
5. Оставить root entry в `Без группы`.
6. Выбрать `<path-to-icon.ico>`.
7. Убедиться, что direct `.ico` preview появился.
8. Нажать `Готово` и убедиться, что карточка свернулась в compact view.
9. Нажать `Редактировать` и убедиться, что draft fields не потерялись.
10. Добавить grouped entry.
11. Указать группу через group container, например `Цветные`.
12. Временно сделать entry невалидным, например указать запрещённый символ в имени создаваемой папки, и нажать `Сохранить`.
13. Убедиться, что affected card раскрылась и показала inline validation error.
14. Исправить значение и нажать `Готово`; убедиться, что карточка снова свернулась без сохранения settings.
15. Нажать `Сохранить`.
16. Изменить draft и нажать `Отменить изменения`.

Ожидаемо:

- root entry остаётся в корне меню;
- grouped entry попадает в одноуровневую группу;
- saved entries показываются compact по умолчанию;
- validation errors раскрывают affected card и показываются inline;
- `Готово` только сворачивает карточку и не сохраняет settings;
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
artifacts/publish/Foldora/Foldora.Cli.exe register-menu --dry-run --host-path "<repo>\artifacts\publish\Foldora\Foldora.MenuHost.exe"
artifacts/publish/Foldora/Foldora.Cli.exe register-menu --host-path "<repo>\artifacts\publish\Foldora\Foldora.MenuHost.exe"
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
- при desktop background create папка появляется near cursor/menu selection area, а не в левом свободном desktop slot;
- exact original right-click point не гарантируется: legacy menu не передаёт эти координаты, Explorer может snap/shift icons по grid/auto-arrange rules;
- при занятой соседней grid cell проверить, что Explorer может сдвинуть существующий icon; это допустимое поведение MVP.
- если desktop placement не сработал, открыть `%AppData%\Foldora\Logs\menuhost-placement.log` и приложить latest JSONL entry к следующему investigation step.

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
- Desktop placement является best-effort: Foldora пытается передвинуть созданный desktop folder near captured cursor/menu selection point, но exact original right-click point недоступна через legacy `%V`.
- Первая папка на Desktop может сначала появиться с default icon из-за возможного Explorer refresh/icon cache timing issue; это отдельный `TD-0002`, не placement limitation.
- Registry menu в publish/manual проверке должен указывать на `artifacts/publish/Foldora/Foldora.MenuHost.exe`.
- MenuHost desktop placement diagnostics находятся в `%AppData%\Foldora\Logs\menuhost-placement.log`.
- Если publish-папку нужно удалить, сначала выполнить `unregister-menu`, чтобы Explorer menu не ссылался на удалённый host.
- User-facing diagnostics для failures внутри `Foldora.MenuHost` из Explorer menu пока future work.

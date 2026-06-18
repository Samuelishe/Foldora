# UX Flow

Этот документ фиксирует целевой пользовательский flow для WPF MVP. Phase 4 уже реализует staged editing, add/remove entries, staged выбор `.ico`, прямой preview из `.ico` и явные Explorer integration controls.

## WPF MVP Editor

Минимальный редактор пользовательского меню должен содержать:

- поле названия top-level menu, по умолчанию `Создать папку`;
- список пользовательских entries;
- кнопку `+` для добавления пункта;
- кнопку удаления пункта;
- поле `DisplayName`;
- поле `DefaultFolderName`;
- кнопку `Выбрать .ico`;
- preview иконки примерно 50x50;
- checkbox `IsEnabled`;
- кнопку `Сохранить`;
- кнопку `Отменить изменения`;
- кнопку `Включить меню Проводника`;
- кнопку `Отключить меню Проводника`;
- кнопку `Сбросить меню`.

UI не должен полагаться на `MessageBox` как основной механизм ошибок. Предпочтительные механизмы:

- inline validation рядом с полями;
- status area;
- список ошибок/предупреждений перед сохранением;
- аккуратные dialog windows позже, если нужны.

## Staged Save

Редактор не должен сразу применять изменения в registry.

Целевая модель:

```text
Открыть Foldora
  -> загрузить Saved settings
  -> создать Draft state
  -> пользователь добавляет/удаляет/редактирует пункты
  -> Explorer menu не меняется
  -> пользователь нажимает "Сохранить"
      -> validate draft
      -> import pending icons
      -> write settings.json
      -> show success/error
```

Добавление, удаление и редактирование строк в UI не должны сразу писать в registry. В текущем MVP registry перестраивается только по отдельным явным Explorer integration actions, а не по обычной кнопке `Сохранить`.

В текущем phase 4 `Сохранить` валидирует draft, импортирует pending `.ico` в AppData и пишет `settings.json`. Даже если `ExplorerIntegrationEnabled = true`, WPF не перестраивает registry menu и показывает нейтральный статус о том, что меню Проводника не обновлялось.

Выбор `.ico` является staged:

```text
User clicks "Выбрать .ico"
  -> file picker returns external source path
  -> draft entry stores PendingIconSourcePath
  -> UI shows selected filename/status and direct .ico preview
  -> settings.json is not changed
  -> AppData icons are not permanently changed yet

User clicks "Сохранить"
  -> validate draft
  -> validate pending icon source
  -> import icon through IconImportService
  -> update entry.IconPath to imported AppData path
  -> save settings.json
```

`Отменить изменения` очищает pending icon selections, возвращает удалённые draft entries и убирает новые unsaved entries.

Если registry rebuild в будущем упадёт после успешного сохранения settings, UI должен показать:

```text
Настройки сохранены, но меню Проводника не обновлено.
```

Сложный rollback settings из-за registry failure не нужен.

## DefaultFolderName Input

`DefaultFolderName` - это имя папки, создаваемой при выборе пункта меню.

При создании нового entry поле по умолчанию:

```text
Новая папка
```

Если пользователь очищает поле, runtime fallback:

```text
Новая папка
```

Решение для будущего WPF input:

- при ручном вводе invalid Windows filename chars лучше не давать печатать;
- при paste invalid chars можно заменять пробелом/удалять и показывать предупреждение;
- при сохранении validator всё равно обязан проверить данные;
- CLI не должен молча исправлять явно invalid `--folder-name`, CLI должен вернуть понятную ошибку.

## Icon Preview

WPF phase 3 показывает `.ico` напрямую в строке entry, примерно 50x50.

Preview source:

- pending selected icon, если он есть;
- иначе saved `IconPath`;
- иначе empty placeholder.

Если preview decode падает или файл отсутствует, editor показывает empty preview/status и не валит окно.

Папка зарезервирована на будущее:

```text
%AppData%\Foldora\previews\
```

Preview generation всё ещё future:

- не генерировать preview-файлы без необходимости;
- если WPF preview из `.ico` окажется медленным/нестабильным, добавить отдельный preview generator;
- preview generator должен хранить ресурсы в `%AppData%\Foldora\previews\`.

## Explorer Integration Controls

В UI явно разделены операции:

- `Проверить план` - WPF-аналог `register-menu --dry-run`: построить и валидировать registry plan, показать summary операций/root paths/command example, ничего не писать в registry и не менять `ExplorerIntegrationEnabled`.
- `Включить меню Проводника` - применить validated HKCU legacy menu из saved settings и поставить `ExplorerIntegrationEnabled = true`, если есть enabled entries.
- `Отключить меню Проводника` - выполнить семантику `unregister-menu`: убрать Foldora из Explorer, но сохранить entries/settings.
- `Сбросить меню` - выполнить семантику `menu reset --yes` после явного подтверждения: очистить entries, вернуть title к `Создать папку`, отключить integration.

`Проверить план` и `Включить меню Проводника` требуют clean draft. Если есть unsaved changes, UI показывает `Сначала сохраните изменения.` и не выполняет operation. Это сохраняет правило: registry отражает saved settings, а не временный draft.

`Отключить меню Проводника` можно выполнять при unsaved changes, потому что операция не зависит от draft entries и не удаляет пользовательские entries/settings.

Если enabled entries нет, `Включить меню Проводника` не создаёт пустое меню: Foldora-owned roots удаляются, `ExplorerIntegrationEnabled` остаётся `false`, UI сообщает `Нет включённых пунктов меню. Меню Проводника не создано.`

`Сбросить меню` требует явного подтверждения в UI. Reset очищает saved entries, возвращает title к `Создать папку`, удаляет только Foldora-owned registry roots, сохраняет `settings.json`, не удаляет AppData root, packs и импортированные `.ico`.

Отсутствие меню в Explorer является нормальным состоянием, а не ошибкой.

## Manual Verification Flow

Для разработки и ручной проверки:

```text
foldora menu add --icon "<path-to-test.ico>" --name "Череп" --folder-name "Череп"
foldora register-menu --dry-run
foldora register-menu --cli-path "<absolute-path-to-Foldora.Cli.exe>"
```

После проверки:

```text
foldora unregister-menu
```

Для полного сброса пользовательского меню:

```text
foldora menu reset --yes
```

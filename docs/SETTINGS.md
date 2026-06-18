# Settings

Пользовательские данные хранятся в AppData, а не рядом с `.exe`.

Фиксированный layout:

```text
%AppData%\Foldora\settings.json
%AppData%\Foldora\icons\
%AppData%\Foldora\previews\
%AppData%\Foldora\packs\
```

Пути строятся через:

```csharp
Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
```

Минимальная модель `FoldoraSettings`:

- `ActivePackId`
- `Language`
- `ExplorerIntegrationEnabled`
- `DefaultCreateStyleId`
- `DefaultApplyStyleId`
- `ShowLegacyContextMenu`
- `OpenPickerForCustomStyle`
- `CreateFolderMenu`

`Language`:

- supported values: `ru`, `en`;
- default: `ru`;
- старые settings без `language` загружаются как `ru`;
- unsupported values нормализуются в `ru`;
- settings UI сохраняет язык в тот же `settings.json`.

WPF startup не должен синхронно читать `settings.json` в конструкторском path главного окна. `MainViewModel.CreateDefault()` создаёт localization service с fallback `ru`, а сохранённый `Language` применяется после async `LoadAsync`. Если чтение settings всё же падает на startup path, ошибка не глотается молча: диагностический файл пишется в `%AppData%\Foldora\Logs\startup-error.log`.

При первом обращении `FoldoraSettingsStorage` создаёт root, `icons`, `previews`, `packs` и `settings.json`, если они отсутствуют. Пустой список entries после первого запуска является нормальным состоянием: Foldora не добавляет demo entries автоматически.

Минимальный JSON по смыслу:

```json
{
  "explorerIntegrationEnabled": false,
  "language": "ru",
  "createFolderMenu": {
    "title": "Создать папку",
    "entries": []
  }
}
```

`FolderMenuEntry`:

- `Id` - стабильный технический идентификатор, не зависящий от подписи.
- `DisplayName` - пользовательская подпись для будущего Explorer submenu.
- `DefaultFolderName` - имя папки, которую Foldora создаст при выборе пункта.
- `GroupName` - optional visible name одноуровневой группы; empty означает root-level entry.
- `IconPath` - импортированная копия `.ico` внутри `%AppData%\Foldora\icons`.
- `PreviewPath` - optional/future путь preview.
- `SortOrder` - порядок пунктов.
- `IsEnabled` - возможность скрыть пункт без удаления.

`DisplayName` и `GroupName` нельзя использовать как имя файла, registry key или stable id.
Дубликаты `DisplayName` разрешены и сохраняются как пользовательский выбор.
Одинаковые `GroupName` разрешены и объединяют entries в одно submenu. Старые settings без `groupName` загружаются как root-level entries.
WPF grouped sections являются presentation layer; пустые группы не сохраняются в JSON как отдельные сущности.

Если старый entry не содержит `DefaultFolderName`, при загрузке используется fallback `Новая папка`.

`ExplorerIntegrationEnabled` отражает состояние последнего успешного register/unregister flow. `register-menu --dry-run` не меняет этот флаг.

WPF editor сохраняет draft в `settings.json` только по кнопке `Сохранить`. Если `ExplorerIntegrationEnabled = false`, Save пишет только settings и сохраняет flag false. Если `ExplorerIntegrationEnabled = true`, Save после записи settings rebuild-ит Foldora-owned registry menu из только что сохранённых settings.

Выбранные в WPF `.ico` до save хранятся только как draft pending source path и не попадают в `settings.json`. Во время save они импортируются в `%AppData%\Foldora\icons\<entry-id>.ico`, после чего постоянный `IconPath` указывает только на AppData-копию.

WPF phase 3 preview не меняет JSON format: `PreviewPath` не заполняется, preview-файлы не создаются, `%AppData%\Foldora\previews\` остаётся зарезервированным future layout.

WPF phase 4 меняет `ExplorerIntegrationEnabled` только через явные Explorer integration actions:

- `Проверить план` не меняет settings.
- `Включить меню Проводника` сохраняет `ExplorerIntegrationEnabled = true`, если есть enabled entries; если enabled entries нет, сохраняет `false`.
- `Отключить меню Проводника` сохраняет `ExplorerIntegrationEnabled = false` и не удаляет entries/title.
- `Сбросить меню` сохраняет `ExplorerIntegrationEnabled = false`, очищает entries и возвращает title к `Создать папку`.

`Проверить план` и `Включить меню Проводника` требуют отсутствия unsaved draft changes. `Отключить меню Проводника` можно выполнить при unsaved draft changes; в этом случае saved settings получают только новое значение integration flag, а текущий draft в UI не перезатирается.

После save-triggered rebuild current register-service policy сохраняется: если enabled entries нет, Foldora-owned registry roots удаляются и `ExplorerIntegrationEnabled` становится `false`. Если registry rebuild падает после успешного settings save, settings не откатываются; UI показывает warning/error.

`CreateFolderMenu.Title` является видимым top-level именем legacy Explorer menu. Если title пустой/whitespace при построении registry plan, используется fallback `Создать папку`. Technical registry key остаётся `Foldora` и не зависит от title.

`unregister-menu` меняет только `ExplorerIntegrationEnabled = false` после удаления Foldora-owned registry roots; `CreateFolderMenu.Entries` и title сохраняются.

`menu reset --yes` сохраняет `settings.json`, но сбрасывает пользовательское меню:

```json
{
  "explorerIntegrationEnabled": false,
  "createFolderMenu": {
    "title": "Создать папку",
    "entries": []
  }
}
```

Команда не удаляет `%AppData%\Foldora`, `packs` и импортированные `.ico`.

`%AppData%\Foldora\previews\` зарезервирован на будущее. WPF MVP может показывать `.ico` напрямую и не обязан генерировать preview-файлы.

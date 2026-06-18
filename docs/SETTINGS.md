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

При первом обращении `FoldoraSettingsStorage` создаёт root, `icons`, `previews`, `packs` и `settings.json`, если они отсутствуют. Пустой список entries после первого запуска является нормальным состоянием: Foldora не добавляет demo entries автоматически.

Минимальный JSON по смыслу:

```json
{
  "explorerIntegrationEnabled": false,
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
- `IconPath` - импортированная копия `.ico` внутри `%AppData%\Foldora\icons`.
- `PreviewPath` - optional/future путь preview.
- `SortOrder` - порядок пунктов.
- `IsEnabled` - возможность скрыть пункт без удаления.

`DisplayName` нельзя использовать как имя файла, registry key или stable id.
Дубликаты `DisplayName` разрешены и сохраняются как пользовательский выбор.

Если старый entry не содержит `DefaultFolderName`, при загрузке используется fallback `Новая папка`.

`ExplorerIntegrationEnabled` отражает состояние последнего успешного register/unregister flow. `register-menu --dry-run` не меняет этот флаг.

WPF editor phase 1 сохраняет draft в `settings.json` только по кнопке `Сохранить`. Он сохраняет существующее значение `ExplorerIntegrationEnabled`, но не перестраивает registry menu и не меняет этот флаг.

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

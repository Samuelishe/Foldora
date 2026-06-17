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

Если старый entry не содержит `DefaultFolderName`, при загрузке используется fallback `Новая папка`.

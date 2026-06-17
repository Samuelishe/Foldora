# Settings

Пользовательские данные хранятся в AppData, а не рядом с `.exe`.

Фиксированный layout:

```text
%AppData%\Foldora\settings.json
%AppData%\Foldora\packs\
%AppData%\Foldora\icons\
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

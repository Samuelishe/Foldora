# Packs

Минимальная модель pack manifest:

```json
{
  "id": "fluent-folders",
  "displayName": "Fluent Folders",
  "version": "1.0.0",
  "author": "User",
  "styles": [
    {
      "id": "code",
      "displayName": "Code",
      "icon": "icons/code.ico",
      "category": "Work"
    },
    {
      "id": "docs",
      "displayName": "Documents",
      "icon": "icons/docs.ico",
      "category": "Work"
    }
  ]
}
```

Pack import/export пока не реализован. Icon packs являются будущей возможностью обмена и распространения наборов entries, а не основой текущего MVP. Текущий priority ниже image-to-ICO conversion, WPF picker/drop integration, same-group entry reorder and deeper ordering work; подробный порядок зафиксирован в `ICON_CONVERSION_ROADMAP.md`.

Иконки должны быть настоящими `.ico`, а не переименованными PNG. Для MVP используются абсолютные пути в AppData.

Главный MVP-объект сейчас - пользовательские menu entries в `settings.json`: пользователь выбирает произвольный `.ico`, задаёт любую подпись, Foldora копирует иконку в `%AppData%\Foldora\icons` и позже сгенерирует registry context menu из этих entries.

Menu entries валидируются отдельно от pack manifest: `DisplayName` может дублироваться, а `DefaultFolderName` должен быть безопасным Windows folder name.

Будущий pack import должен создавать или обновлять пользовательские entries через те же validation rules, а не обходить модель `FolderMenuEntry`. Pack не должен получать дополнительные права на filesystem или registry.

Possible future `.foldorapack` direction: zip container with `manifest.json`, `icons/` and optional `preview/`. Import should unpack to temp, validate manifest, icon paths, file count and size limits, then copy selected icons into AppData and generate new entry ids. Export should collect selected/current entries, copy used icons, write manifest and zip the result. Merge/replace strategy must require user confirmation and must never blindly overwrite settings.

Packs are user-provided content unless Foldora explicitly ships official packs. Bundled official packs require license review and notices; imported third-party icons remain under their own rights and Foldora must not imply ownership.

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

Pack import/export пока не реализован. Icon packs являются будущей возможностью обмена и распространения наборов, а не основой текущего MVP.

Иконки должны быть настоящими `.ico`, а не переименованными PNG. Для MVP используются абсолютные пути в AppData.

Главный MVP-объект сейчас - пользовательские menu entries в `settings.json`: пользователь выбирает произвольный `.ico`, задаёт любую подпись, Foldora копирует иконку в `%AppData%\Foldora\icons` и позже сгенерирует registry context menu из этих entries.

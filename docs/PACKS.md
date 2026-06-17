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

На bootstrap-этапе полноценный import pack flow не реализуется.

Иконки должны быть настоящими `.ico`, а не переименованными PNG. Для MVP используются абсолютные пути в AppData.

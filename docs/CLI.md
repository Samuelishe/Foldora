# CLI

Целевые команды:

```text
foldora create --target "<directory>" --style "<style-id>"
foldora apply --folder "<folder>" --style "<style-id>"
foldora clear --folder "<folder>"
foldora import-pack --path "<pack-path>"
foldora list-packs
foldora list-styles
foldora register-menu
foldora unregister-menu
foldora settings
```

Bootstrap CLI:

- не падает без аргументов;
- показывает help;
- для неготовых команд пишет понятное skeleton-сообщение;
- не выполняет registry operations без отдельной реализации.

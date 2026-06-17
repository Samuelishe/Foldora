# CLI

Целевые команды:

```text
foldora create --target "<directory>" --style "<style-id>"
foldora apply --folder "<folder>" --icon "<absolute-icon-path>"
foldora clear --folder "<folder>"
foldora import-pack --path "<pack-path>"
foldora list-packs
foldora list-styles
foldora register-menu
foldora unregister-menu
foldora settings
```

Реализовано сейчас:

- не падает без аргументов;
- показывает help;
- выполняет `apply --folder --icon` через `DesktopIniService`;
- выполняет `clear --folder` через `DesktopIniService`;
- для неготовых команд пишет понятное skeleton-сообщение;
- не выполняет registry operations без отдельной реализации.

Ограничения текущего CLI:

- `--style` пока не реализован.
- `import-pack` пока не реализован.
- Registry context menu будет следующим отдельным этапом после проверки CLI.
- Explorer restart и icon cache reset не выполняются.
- Explorer может не обновить иконку мгновенно из-за кэша.

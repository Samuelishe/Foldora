# CLI

Целевые команды:

```text
foldora create --target "<directory>" --style "<style-id>"
foldora apply --folder "<folder>" --icon "<absolute-icon-path>"
foldora clear --folder "<folder>"
foldora menu list
foldora menu add --icon "<absolute-icon-path>" [--name "<display-name>"]
foldora menu remove --entry-id "<entry-id>"
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
- выполняет `menu list`, создавая AppData/settings при первом запуске;
- выполняет `menu add --icon ... [--name ...]`, импортируя `.ico` в `%AppData%\Foldora\icons`;
- выполняет `menu remove --entry-id ...`, удаляя entry и принадлежащую ему копию `.ico`;
- для неготовых команд пишет понятное skeleton-сообщение;
- не выполняет registry operations без отдельной реализации.

Ограничения текущего CLI:

- `--style` пока не реализован.
- `create/apply by entry id` пока не реализованы.
- `import-pack` пока не реализован.
- Registry context menu будет следующим отдельным этапом после проверки CLI и будет генерироваться из сохранённых menu entries.
- Explorer restart и icon cache reset не выполняются.
- Explorer может не обновить иконку мгновенно из-за кэша.

Если `--name` не указан или пустой, `menu add` использует первое свободное fallback-имя `Вид N`.

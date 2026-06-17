# CLI

Целевые команды:

```text
foldora create --target "<directory>" --style "<style-id>"
foldora apply --folder "<folder>" --icon "<absolute-icon-path>"
foldora apply --folder "<folder>" --entry-id "<entry-id>"
foldora create --target "<directory>" --entry-id "<entry-id>"
foldora clear --folder "<folder>"
foldora menu list
foldora menu add --icon "<absolute-icon-path>" [--name "<display-name>"] [--folder-name "<default-folder-name>"]
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
- выполняет `apply --folder --entry-id`, применяя иконку сохранённого enabled entry;
- выполняет `create --target --entry-id`, создавая папку с `DefaultFolderName` entry и применяя его иконку;
- выполняет `clear --folder` через `DesktopIniService`;
- выполняет `menu list`, создавая AppData/settings при первом запуске;
- выполняет `menu add --icon ... [--name ...] [--folder-name ...]`, импортируя `.ico` в `%AppData%\Foldora\icons`;
- выполняет `menu remove --entry-id ...`, удаляя entry и принадлежащую ему копию `.ico`;
- для неготовых команд пишет понятное skeleton-сообщение;
- не выполняет registry operations без отдельной реализации.

Ограничения текущего CLI:

- `--style` пока не реализован.
- `import-pack` пока не реализован.
- Registry context menu будет следующим отдельным этапом после проверки CLI и будет генерироваться из сохранённых menu entries.
- Registry plan builder уже формирует будущие command values для `create --target --entry-id`, но `register-menu` и реальная запись в HKCU пока не реализованы.
- Explorer restart и icon cache reset не выполняются.
- Explorer может не обновить иконку мгновенно из-за кэша.

Если `--name` не указан или пустой, `menu add` использует первое свободное fallback-имя `Вид N`.
Если `--folder-name` не указан или пустой, используется `Новая папка`.
CLI не исправляет явно невалидный `--folder-name` молча: запрещённые Windows filename characters, reserved device names, trailing dot/space, control chars и слишком длинные значения дают понятную ошибку, entry не создаётся.

Для `apply` доступны два взаимоисключающих режима: `--icon` для прямого пути к `.ico` или `--entry-id` для сохранённого entry. Если указаны оба или не указан ни один, CLI возвращает ошибку.

`create --entry-id` создаёт папку внутри `--target`. Если имя занято файлом или папкой, используется схема `Name (2)`, `Name (3)` и так далее, максимум до разумного лимита попыток.

# Project State

Дата: 2026-06-17.

Состояние: реализован слой пользовательских пунктов меню Foldora и validation/model правила перед WPF/registry integration. Команды `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"` и `foldora clear --folder "<folder>"` работают через `desktop.ini`; команды `foldora menu list/add/remove` управляют пользовательским списком будущего submenu.

Текущий фокус: проверить устойчивость модели `DisplayName`/`DefaultFolderName`, validation rules и `.ico` import перед WPF editor и HKCU legacy context menu.

Открытые вопросы:

- Финальное публичное имя продукта.
- Точный UX для выбора custom style.
- Правила снятия атрибута `System` с папки после clear, если в будущем появится надёжное определение владельца shell-настроек.
- Команды создания/применения папки по `entry-id`.
- Draft/staged-save реализация в WPF.

# Project State

Дата: 2026-06-17.

Состояние: реализован слой пользовательских пунктов меню Foldora. Команды `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"` и `foldora clear --folder "<folder>"` работают через `desktop.ini`; команды `foldora menu list/add/remove` управляют пользовательским списком будущего submenu.

Текущий фокус: проверить хранение пользовательских menu entries в AppData и импорт произвольных `.ico` перед отдельным этапом HKCU legacy context menu.

Открытые вопросы:

- Финальное публичное имя продукта.
- Точный UX для выбора custom style.
- Правила снятия атрибута `System` с папки после clear, если в будущем появится надёжное определение владельца shell-настроек.
- Команды создания/применения папки по `entry-id`.

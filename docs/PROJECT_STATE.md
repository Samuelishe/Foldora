# Project State

Дата: 2026-06-17.

Состояние: добавлен безопасный writer для применения validated registry plan в HKCU. `foldora register-menu`, `foldora register-menu --dry-run` и `foldora unregister-menu` реализованы. Команды `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"`, `foldora apply --folder "<folder>" --entry-id "<entry-id>"`, `foldora create --target "<directory>" --entry-id "<entry-id>"` и `foldora clear --folder "<folder>"` работают через `desktop.ini`; команды `foldora menu list/add/remove` управляют пользовательским списком будущего submenu.

Текущий фокус: ручная проверка legacy context menu в Windows 11, особенно placeholder policy `%1` и `%V`.

Открытые вопросы:

- Финальное публичное имя продукта.
- Точный UX для выбора custom style.
- Правила снятия атрибута `System` с папки после clear, если в будущем появится надёжное определение владельца shell-настроек.
- Draft/staged-save реализация в WPF.
- Фактическое поведение Explorer placeholders `%1` и `%V` на Windows 11 после ручной проверки.

# Project State

Дата: 2026-06-17.

Состояние: добавлен testable registry plan builder для будущего HKCU legacy context menu. Реальной записи в реестр нет. Команды `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"`, `foldora apply --folder "<folder>" --entry-id "<entry-id>"`, `foldora create --target "<directory>" --entry-id "<entry-id>"` и `foldora clear --folder "<folder>"` работают через `desktop.ini`; команды `foldora menu list/add/remove` управляют пользовательским списком будущего submenu.

Текущий фокус: проверить registry plan safety и placeholder policy перед отдельным writer-этапом HKCU legacy context menu.

Открытые вопросы:

- Финальное публичное имя продукта.
- Точный UX для выбора custom style.
- Правила снятия атрибута `System` с папки после clear, если в будущем появится надёжное определение владельца shell-настроек.
- Draft/staged-save реализация в WPF.
- Ручная проверка Explorer placeholders `%1` и `%V` на Windows 11 перед реальной записью в реестр.

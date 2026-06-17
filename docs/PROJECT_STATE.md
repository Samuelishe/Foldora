# Project State

Дата: 2026-06-17.

Состояние: сохранённые пользовательские entries стали исполняемыми через CLI. Команды `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"`, `foldora apply --folder "<folder>" --entry-id "<entry-id>"`, `foldora create --target "<directory>" --entry-id "<entry-id>"` и `foldora clear --folder "<folder>"` работают через `desktop.ini`; команды `foldora menu list/add/remove` управляют пользовательским списком будущего submenu.

Текущий фокус: проверить прямые CLI-действия по сохранённым entries перед HKCU legacy context menu, который позже будет вызывать эти команды.

Открытые вопросы:

- Финальное публичное имя продукта.
- Точный UX для выбора custom style.
- Правила снятия атрибута `System` с папки после clear, если в будущем появится надёжное определение владельца shell-настроек.
- Draft/staged-save реализация в WPF.

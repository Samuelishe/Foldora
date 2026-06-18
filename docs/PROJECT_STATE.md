# Project State

Дата: 2026-06-18.

Состояние: legacy context menu генерируется в HKCU под Foldora-owned roots, но видимый верхний пункт теперь берётся из `FoldoraSettings.CreateFolderMenu.Title`. Текущий MVP-вид Explorer menu: `Создать папку -> <entry>`, без лишнего видимого слоя `Foldora`; технический registry key остаётся `Foldora`. `foldora register-menu`, `foldora register-menu --dry-run`, `foldora unregister-menu` и `foldora menu reset --yes` реализованы. Команды `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"`, `foldora apply --folder "<folder>" --entry-id "<entry-id>"`, `foldora create --target "<directory>" --entry-id "<entry-id>"` и `foldora clear --folder "<folder>"` работают через `desktop.ini`; команды `foldora menu list/add/remove/reset` управляют пользовательским списком меню.

Текущий фокус: повторная ручная проверка legacy context menu в Windows 11 после упрощения visible shape, особенно placeholder policy `%1` и `%V`.

Открытые вопросы:

- Финальное публичное имя продукта.
- Точный UX для выбора custom style.
- Правила снятия атрибута `System` с папки после clear, если в будущем появится надёжное определение владельца shell-настроек.
- Draft/staged-save реализация в WPF.
- Фактическое поведение Explorer placeholders `%1` и `%V` на Windows 11 после повторной ручной проверки.

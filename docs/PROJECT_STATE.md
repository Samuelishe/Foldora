# Project State

Дата: 2026-06-18.

Состояние: legacy context menu генерируется в HKCU под Foldora-owned roots, но видимый верхний пункт теперь берётся из `FoldoraSettings.CreateFolderMenu.Title`. Текущий MVP-вид Explorer menu: `Создать папку -> <entry>`, без лишнего видимого слоя `Foldora`; технический registry key остаётся `Foldora`. `foldora register-menu`, `foldora register-menu --dry-run`, `foldora unregister-menu` и `foldora menu reset --yes` реализованы. Команды `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"`, `foldora apply --folder "<folder>" --entry-id "<entry-id>"`, `foldora create --target "<directory>" --entry-id "<entry-id>"` и `foldora clear --folder "<folder>"` работают через `desktop.ini`; команды `foldora menu list/add/remove/reset` управляют пользовательским списком меню. WPF editor phase 2 реализует staged editing title/entries, add/remove draft entries и staged выбор `.ico`; импорт выбранных иконок в AppData происходит только по `Сохранить`. Registry rebuild из WPF пока не выполняется.

Documentation consolidation: продуктовая концепция вынесена в `docs/PRODUCT_VISION.md`, целевой WPF/staged-save flow - в `docs/UX_FLOW.md`. Главный MVP-объект зафиксирован как пользовательский `FolderMenuEntry`; packs остаются будущим import/export механизмом.

Текущий фокус: WPF editor MVP следующего этапа - icon preview и будущие registry controls без смешивания UI и Core/Shell logic.

Открытые вопросы:

- Финальное публичное имя продукта.
- Точный UX для выбора custom style.
- Правила снятия атрибута `System` с папки после clear, если в будущем появится надёжное определение владельца shell-настроек.
- Фактическое поведение Explorer placeholders `%1` и `%V` на Windows 11 после повторной ручной проверки.
- WPF controls для register/unregister/reset и момент registry rebuild после save/enable action.

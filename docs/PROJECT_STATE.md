# Project State

Дата: 2026-06-18.

Состояние: legacy context menu генерируется в HKCU под Foldora-owned roots, но видимый верхний пункт берётся из `FoldoraSettings.CreateFolderMenu.Title`. Текущий MVP-вид Explorer menu: `Создать папку -> <entry>`, без лишнего видимого слоя `Foldora`; технический registry key остаётся `Foldora`. `foldora register-menu`, `foldora register-menu --dry-run`, `foldora unregister-menu` и `foldora menu reset --yes` реализованы. Команды `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"`, `foldora apply --folder "<folder>" --entry-id "<entry-id>"`, `foldora create --target "<directory>" --entry-id "<entry-id>"` и `foldora clear --folder "<folder>"` работают через `desktop.ini`; команды `foldora menu list/add/remove/reset` управляют пользовательским списком меню. WPF editor phase 4 реализует staged editing title/entries, add/remove draft entries, staged выбор `.ico`, прямой preview из `.ico` около 50x50 и отдельные controls для Explorer integration: dry-run, register, unregister и reset. Импорт выбранных иконок в AppData происходит только по `Сохранить`; preview-файлы не генерируются. Обычный WPF `Сохранить` по-прежнему пишет только settings и не перестраивает registry menu.

Documentation consolidation: продуктовая концепция вынесена в `docs/PRODUCT_VISION.md`, целевой WPF/staged-save flow - в `docs/UX_FLOW.md`. Главный MVP-объект зафиксирован как пользовательский `FolderMenuEntry`; packs остаются будущим import/export механизмом.

Текущий фокус: ручная проверка WPF Explorer integration controls на Windows 11 и следующий UX-этап после базового MVP.

Открытые вопросы:

- Финальное публичное имя продукта.
- Точный UX для выбора custom style.
- Правила снятия атрибута `System` с папки после clear, если в будущем появится надёжное определение владельца shell-настроек.
- Фактическое поведение Explorer placeholders `%1` и `%V` на Windows 11 после повторной ручной проверки.
- Preview generation/cache policy, если прямой WPF preview из `.ico` окажется недостаточным.

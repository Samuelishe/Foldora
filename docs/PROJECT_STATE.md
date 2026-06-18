# Project State

Дата: 2026-06-18.

Состояние: legacy context menu генерируется в HKCU под Foldora-owned roots, но видимый верхний пункт берётся из `FoldoraSettings.CreateFolderMenu.Title`. Текущий MVP-вид Explorer menu: `Создать папку -> <entry>`, без лишнего видимого слоя `Foldora`; technical registry key остаётся `Foldora`. Entry keys получают registry `Icon = <entry.IconPath>`, если imported `.ico` существует. `foldora register-menu`, `foldora register-menu --dry-run`, `foldora register-menu --host-path`, legacy alias `--cli-path`, `foldora unregister-menu` и `foldora menu reset --yes` реализованы. Explorer registry commands должны запускать no-console `Foldora.MenuHost.exe`; `Foldora.Cli.exe` остаётся console tool для разработки и ручных команд. Команды `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"`, `foldora apply --folder "<folder>" --entry-id "<entry-id>"`, `foldora create --target "<directory>" --entry-id "<entry-id>"` и `foldora clear --folder "<folder>"` работают через `desktop.ini`; команды `foldora menu list/add/remove/reset` управляют пользовательским списком меню. WPF editor реализует staged editing title/entries, add/remove draft entries, staged выбор `.ico`, прямой preview из `.ico` около 50x50 и controls для Explorer integration. UI cleanup phase 1 заменил технический `DataGrid` на карточки пунктов меню, скрывает `EntryId` из основного flow, разделяет обычную интеграцию и dangerous reset, а technical registry details показывает только в раскрываемом блоке. Импорт выбранных иконок в AppData происходит только по `Сохранить`; preview-файлы не генерируются. Если `ExplorerIntegrationEnabled = true`, WPF `Сохранить` после успешного settings save rebuild-ит Foldora-owned registry menu; если integration disabled, `Сохранить` пишет только settings.

Documentation consolidation: продуктовая концепция вынесена в `docs/PRODUCT_VISION.md`, целевой WPF/staged-save flow - в `docs/UX_FLOW.md`. Главный MVP-объект зафиксирован как пользовательский `FolderMenuEntry`; packs остаются будущим import/export механизмом.

Текущий фокус: manual-test-driven investigation для `desktop.ini` attribute policies и Explorer UX observations. Default production behavior для folder icon apply остаётся `CompatibilitySystem`: folder `System`, `desktop.ini` `Hidden + System`. Добавлена diagnostic CLI-команда `foldora diagnostics desktop-ini-policy --target "<directory>" --icon "<ico>"`, которая создаёт тестовые папки для ручного выбора deletion-friendly policy. Также зафиксировано MVP-ограничение legacy registry menu: Foldora получает target directory path, но не cursor/icon-view coordinates, поэтому Explorer сам выбирает позицию нового значка на рабочем столе.

Открытые вопросы:

- Финальное публичное имя продукта.
- Точный UX для выбора custom style.
- Правила снятия атрибута `System` с папки после clear, если в будущем появится надёжное определение владельца shell-настроек.
- Какая `DesktopIniAttributePolicy` сохраняет custom icon, но убирает deletion warning про системный `desktop.ini`.
- Возможен ли user-grade UX создания desktop folder под курсором без COM/modern shell integration.
- Фактическое поведение Explorer placeholders `%1` и `%V` на Windows 11 после повторной ручной проверки.
- Preview generation/cache policy, если прямой WPF preview из `.ico` окажется недостаточным.

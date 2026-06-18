# Project State

Дата: 2026-06-18.

Состояние: legacy context menu генерируется в HKCU под Foldora-owned roots, но видимый верхний пункт берётся из `FoldoraSettings.CreateFolderMenu.Title`. Текущий MVP-вид Explorer menu: `Создать папку -> <entry>`, без лишнего видимого слоя `Foldora`; technical registry key остаётся `Foldora`. Entry keys получают registry `Icon = <entry.IconPath>`, если imported `.ico` существует. `foldora register-menu`, `foldora register-menu --dry-run`, `foldora register-menu --host-path`, legacy alias `--cli-path`, `foldora unregister-menu` и `foldora menu reset --yes` реализованы. Explorer registry commands должны запускать no-console `Foldora.MenuHost.exe`; `Foldora.Cli.exe` остаётся console tool для разработки и ручных команд. Команды `foldora apply --folder "<folder>" --icon "<absolute-icon-path>"`, `foldora apply --folder "<folder>" --entry-id "<entry-id>"`, `foldora create --target "<directory>" --entry-id "<entry-id>"` и `foldora clear --folder "<folder>"` работают через `desktop.ini`; команды `foldora menu list/add/remove/reset` управляют пользовательским списком меню. WPF editor реализует staged editing title/entries, add/remove draft entries, staged выбор `.ico`, прямой preview из `.ico` около 50x50 и controls для Explorer integration. UI cleanup phase 1 заменил технический `DataGrid` на карточки пунктов меню, скрывает `EntryId` из основного flow, разделяет обычную интеграцию и dangerous reset, а technical registry details показывает только в раскрываемом блоке. Импорт выбранных иконок в AppData происходит только по `Сохранить`; preview-файлы не генерируются. Если `ExplorerIntegrationEnabled = true`, WPF `Сохранить` после успешного settings save rebuild-ит Foldora-owned registry menu; если integration disabled, `Сохранить` пишет только settings.

Documentation consolidation: продуктовая концепция вынесена в `docs/PRODUCT_VISION.md`, целевой WPF/staged-save flow - в `docs/UX_FLOW.md`. Главный MVP-объект зафиксирован как пользовательский `FolderMenuEntry`; packs остаются будущим import/export механизмом.

Текущий фокус: WPF shell/settings foundation. Главное окно использует custom title bar на `WindowChrome`: settings gear, minimize, maximize/restore и close находятся в единой шапке окна, resize сохраняется, maximize должен respect Windows work area/taskbar без отдельного WinAPI layer. Добавлено settings UI для языка приложения и persisted `Language` setting (`ru`/`en`, default `ru`) в `%AppData%\Foldora\settings.json`; старые settings без language получают fallback `ru`, unsupported values нормализуются в `ru`. Добавлен минимальный localization foundation для основных WPF labels/buttons; часть status/error messages пока остаётся русской и требует будущего localization cleanup.

Открытые вопросы:

- Финальное публичное имя продукта.
- Точный UX для выбора custom style.
- Возможен ли user-grade UX создания desktop folder под курсором без COM/modern shell integration.
- Фактическое поведение Explorer placeholders `%1` и `%V` на Windows 11 после повторной ручной проверки.
- Preview generation/cache policy, если прямой WPF preview из `.ico` окажется недостаточным.
- One-level grouping через `FolderMenuEntry.GroupName` перед full tree migration.

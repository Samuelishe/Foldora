# Документация Foldora

Рекомендуемый порядок чтения:

1. `PROJECT_STATE.md` - текущее состояние и фокус.
2. `PRODUCT_VISION.md` - продуктовая концепция, главный объект MVP и safety-философия.
3. `UX_FLOW.md` - будущий WPF flow, staged-save, reset/cleanup UX и preview policy.
4. `ROADMAP.md` - ближайший MVP и будущие версии.
5. `ARCHITECTURE.md` - проекты, слои и зависимости.
6. `REQUIREMENTS.md` - платформа, локальные инструменты и ограничения.
7. `DESKTOP_INI.md` и `SHELL_INTEGRATION.md` - ключевые Windows-механизмы.
8. `SMOKE_TEST.md` - ручной checklist проверки MVP на Windows 11.
9. `TECH_DEBT.md` - known technical debt, accepted limitations and investigation items.
10. `MENU_MODEL.md`, `PACKS.md`, `SETTINGS.md`, `LOCALIZATION.md`, `CLI.md`, `ICON_CONVERSION_ROADMAP.md`, `UI_DESIGN.md`, `UI_AUDIT.md` - пользовательские форматы, интерфейсы, будущая image-to-ICO roadmap и UI/UX audit baseline.
11. `RESOURCE_POLICY.md` - правила добавления внешних и self-authored визуальных ресурсов.
12. `CODING_RULES.md` и `FILE_INDEX.md` - правила разработки и карта файлов.

Foldora пока является рабочим названием и может измениться перед публичным релизом.

Корневой `README.md` является публичным GitHub README с product landing верхом: pitch, README hero/mockup image, highlights, quick start, example menu, а затем текущие возможности, ограничения и базовые команды сборки/запуска.
Корневые `LICENSE` и `THIRD_PARTY_NOTICES.md` фиксируют 0BSD-лицензирование оригинальных материалов Foldora и правила/состояние сторонних материалов.

Dev/manual publish helper находится в `scripts/publish-dev.ps1` и создаёт `artifacts/publish/Foldora` без installer/MSIX.
Per-user install foundation находится в `scripts/install-user.ps1` и `scripts/uninstall-user.ps1`: binaries устанавливаются в `%LocalAppData%\Programs\Foldora`, а пользовательские settings/icons/logs остаются в `%AppData%\Foldora`.
Localization foundation описан в `LOCALIZATION.md`: complete/enabled locales `bg`, `cs`, `de`, `en`, `es`, `fr`, `hi`, `hu`, `id`, `it`, `ja`, `ko`, `nl`, `pl`, `pt-BR`, `pt-PT`, `ro`, `ru`, `th`, `tr`, `uk`, `vi`, `zh-Hans`, `zh-Hant`; Settings показывает native names в стабильном English-sort порядке, а смена языка не переводит сохранённые пользовательские menu data.
Settings/Explorer cleanup: главный WPF экран является focused menu editor, а Explorer integration, installation/path information and danger reset находятся в `SettingsWindow`.
UI/UX audit baseline находится в `UI_AUDIT.md`: он фиксирует найденные после ручной проверки design debt items и будущие polish passes.
`ICON_CONVERSION_ROADMAP.md` фиксирует feature priority: implemented image-to-ICO foundation, WPF picker integration and icon-preview drop support, implemented same-group entry reorder, then converter window/batch conversion, deeper ordering work, pack import/export, diagnostics/repair and later release/install polish.

Research notes находятся в `docs/research/`; текущий документ `docs/research/DESKTOP_ICON_PLACEMENT.md` фиксирует desktop placement spike и не является обещанием production support.

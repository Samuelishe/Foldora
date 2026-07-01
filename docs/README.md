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
9. `MENU_MODEL.md`, `PACKS.md`, `SETTINGS.md`, `CLI.md`, `UI_DESIGN.md` - пользовательские форматы и интерфейсы.
10. `RESOURCE_POLICY.md` - правила добавления внешних и self-authored визуальных ресурсов.
11. `CODING_RULES.md` и `FILE_INDEX.md` - правила разработки и карта файлов.

Foldora пока является рабочим названием и может измениться перед публичным релизом.

Корневой `README.md` является публичным GitHub README с кратким описанием продукта, текущих возможностей, ограничений и базовых команд сборки/запуска.
Корневые `LICENSE` и `THIRD_PARTY_NOTICES.md` фиксируют 0BSD-лицензирование оригинальных материалов Foldora и правила/состояние сторонних материалов.

Dev/manual publish helper находится в `scripts/publish-dev.ps1` и создаёт `artifacts/publish/Foldora` без installer/MSIX.

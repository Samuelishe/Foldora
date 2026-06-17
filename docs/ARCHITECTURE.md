# Architecture

Foldora разделена на пять проектов:

- `Foldora.Core` - доменная логика, модели, settings, AppData paths, desktop.ini, validation и тестируемые filesystem операции.
- `Foldora.Shell` - Windows Shell integration: HKCU legacy context menu, unregister flow и command line quoting.
- `Foldora.Cli` - тонкий console интерфейс для команд context menu и ручного запуска.
- `Foldora.App` - WPF-приложение настроек.
- `Foldora.Tests` - unit-тесты Core и Shell.

Зависимости:

- `Foldora.App` -> `Foldora.Core`, `Foldora.Shell`.
- `Foldora.Cli` -> `Foldora.Core`, `Foldora.Shell`.
- `Foldora.Shell` -> `Foldora.Core`.
- `Foldora.Tests` -> `Foldora.Core`, `Foldora.Shell`, `Foldora.Cli`.
- `Foldora.Core` не зависит от других проектов Foldora.

Границы: Core не знает о WPF, CLI, registry API и конкретном UI. Registry logic не находится в Core. WPF code-behind используется только для UI plumbing.

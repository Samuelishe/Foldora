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

Пользовательские menu entries являются главным MVP-объектом для будущего Explorer submenu. Пользователь сам выбирает любые `.ico` и любые подписи; Foldora копирует `.ico` в AppData и хранит metadata в `settings.json`. Registry context menu на следующем этапе должен генерироваться из этих сохранённых entries, а не из жёстко заданных категорий вроде Documents/Code/Photos.

Validation/model слой находится в `Foldora.Core`, а не в CLI/WPF. `DisplayName` отвечает только за подпись меню, `DefaultFolderName` - за имя создаваемой папки. Эти поля не смешиваются.

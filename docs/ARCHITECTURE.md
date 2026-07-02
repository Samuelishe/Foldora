# Architecture

Foldora разделена на восемь проектов:

- `Foldora.Core` - доменная логика, модели, settings, AppData paths, desktop.ini, validation и тестируемые filesystem операции.
- `Foldora.Shell` - Windows Shell integration: HKCU legacy context menu, unregister flow и command line quoting.
- `Foldora.Cli` - тонкий console интерфейс для команд context menu и ручного запуска.
- `Foldora.MenuHost` - no-console Windows executable для запуска Explorer context menu commands.
- `Foldora.Imaging` - чистая `net10.0` библиотека foundation for future image-to-ICO conversion: frame metadata, conversion result/options models, RGBA pixel buffer model, alpha-aware resize foundation and ICO container writer.
- `Foldora.Imaging.Windows` - Windows-specific `net10.0-windows` bridge for PNG/JPG/BMP decode and PNG frame encoding through WPF imaging APIs.
- `Foldora.App` - WPF-приложение настроек.
- `Foldora.Tests` - unit-тесты Core и Shell.

Зависимости:

- `Foldora.App` -> `Foldora.Core`, `Foldora.Shell`.
- `Foldora.Cli` -> `Foldora.Core`, `Foldora.Shell`.
- `Foldora.MenuHost` -> `Foldora.Core`, `Foldora.Shell`.
- `Foldora.Imaging` currently has no Foldora project dependencies.
- `Foldora.Imaging.Windows` -> `Foldora.Imaging`.
- `Foldora.Shell` -> `Foldora.Core`.
- `Foldora.Tests` -> `Foldora.Core`, `Foldora.Imaging`, `Foldora.Imaging.Windows`, `Foldora.Shell`, `Foldora.Cli`, `Foldora.App`, `Foldora.MenuHost`.
- `Foldora.Core` не зависит от других проектов Foldora.

Imaging dependency direction:

- `Foldora.Core` must not depend on `Foldora.Imaging`.
- `Foldora.Imaging` must stay pure `net10.0` and avoid Windows/WPF/App/UI dependencies.
- `Foldora.Imaging.Windows` isolates Windows/WPF imaging APIs and depends only on `Foldora.Imaging`.
- `Foldora.App` may use `Foldora.Imaging.Windows` later for picker/drop auto-conversion.
- `Foldora.Cli` may use `Foldora.Imaging.Windows` later for a future Windows-only `convert-icon` command.
- `Foldora.MenuHost` should not need `Foldora.Imaging` or `Foldora.Imaging.Windows`; it should stay focused on already-saved create/apply commands.

Границы: Core не знает о WPF, CLI, registry API и конкретном UI. Registry logic не находится в Core. WPF code-behind используется только для UI plumbing.

Пользовательские menu entries являются главным MVP-объектом для будущего Explorer submenu. Пользователь сам выбирает любые `.ico` и любые подписи; Foldora копирует `.ico` в AppData и хранит metadata в `settings.json`. Registry context menu на следующем этапе должен генерироваться из этих сохранённых entries, а не из жёстко заданных категорий вроде Documents/Code/Photos.

Validation/model слой находится в `Foldora.Core`, а не в CLI/WPF. `DisplayName` отвечает только за подпись меню, `DefaultFolderName` - за имя создаваемой папки, `GroupName` - за visible label одноуровневой группы. Эти поля не смешиваются; `GroupName` не является id и не используется как registry key. Core validation возвращает stable invariant issue codes и parameter dictionary; Core `Message` остаётся compatibility/debug fallback. WPF не показывает `Message` напрямую: `Foldora.App` рендерит validation issues через `ValidationMessageLocalizer` и embedded localization catalogs.

Исполнение сохранённых entries также находится в `Foldora.Core`: `FolderMenuEntryActionService` загружает settings, резолвит enabled entry, создаёт папку при необходимости и вызывает `DesktopIniService`. CLI и MenuHost не дублируют filesystem или `desktop.ini` логику.

`DesktopIniService` применяет атрибуты через `DesktopIniAttributePolicy`. Default policy после ручной проверки Windows 11: `ReadOnlyFolderHiddenDesktopIni`, то есть folder `ReadOnly`, `desktop.ini` `Hidden`. Выбор default централизован в `DesktopIniAttributePolicy.Default`; CLI/MenuHost/WPF production flows не выбирают policy сами. `CompatibilitySystem` и другие policies сохранены для diagnostics/manual verification.

`Foldora.Shell` содержит shell-specific planning/integration logic. `ExplorerMenuRegistryPlanBuilder` строит testable plan HKCU операций под Foldora-owned roots, но не пишет в реестр. Видимый legacy menu root берётся из effective `CreateFolderMenu.Title`, при этом technical registry root остаётся `Foldora` для safety boundary. `FolderMenuSettings.TitleIsCustom` отделяет localized default-title mode от user custom title: storage нормализует old default titles, WPF сохраняет текущий localized default при `TitleIsCustom = false`, а custom title не переводится. One-level grouping строится из flat entries через `FolderMenuEntry.GroupName`: group keys технические (`group-NNN`), а пользовательский group title пишется только в `MUIVerb`. Entry registry keys получают `Icon` value только из существующего imported `IconPath`; это shell menu icon, не WPF preview.
`ExplorerMenuRegistryWriter` применяет только validated plan через `IRegistryAccess`. `WindowsRegistryAccess` - единственное место, где используется `Microsoft.Win32.Registry`; тесты используют fake/in-memory registry access.

`Foldora.Shell.Desktop` содержит isolated API для desktop icon positioning. `IDesktopIconPositioningService` и `WindowsDesktopIconPositioningService` используются CLI diagnostic command `diagnostics desktop-icon-position` и best-effort `Foldora.MenuHost` desktop create flow. Core create/apply behavior при этом не меняется.

`Foldora.MenuHost` является Windows-subsystem executable (`WinExe`) без UI и console output. Explorer legacy menu должен запускать `Foldora.MenuHost.exe`, чтобы не мигало console window. Это не service, не tray/background helper и не autostart process: Explorer запускает MenuHost только на пользовательский клик в Foldora context menu, процесс выполняет одну command и завершается. Для `create` MenuHost захватывает текущую cursor screen position до Core action, вызывает `FolderMenuEntryActionService.CreateAsync`, а затем только для current user Desktop directory пытается best-effort reposition созданный desktop item. Positioning failure non-fatal и не превращает успешное создание папки в failure. MenuHost writes placement diagnostics to `%AppData%\Foldora\Logs\menuhost-placement.log` and boundedly retries only when the newly created desktop item is not found in Explorer view yet. MenuHost возвращает non-zero exit code при ошибках create/apply; user-facing diagnostics для context menu failures остаются future work.

Cleanup flow разделён на две операции. `unregister-menu` удаляет только Foldora-owned registry roots и ставит `ExplorerIntegrationEnabled = false`, не удаляя entries/settings. `menu reset --yes` удаляет те же owned roots, очищает `CreateFolderMenu.Entries`, возвращает default-title mode и сохраняет settings; CLI/Shell fallback остаётся `ru`, а WPF reset передаёт текущий localized default через App layer. AppData root, `settings.json`, packs и импортированные `.ico` не удаляются.

WPF editor использует staged-save слой в `Foldora.Core`: `FolderMenuDraftEditor` и `FolderMenuDraftEntry`. Этот слой загружает settings, держит draft-копии, валидирует через существующий validation layer и сохраняет settings только по явному `SaveAsync`. Phase 2 добавляет draft add/remove и pending icon source: выбранные `.ico` импортируются через `IconImportService` только во время save. Phase 3 добавляет `.ico` preview только в `Foldora.App`: WPF service загружает preview напрямую из saved/pending path и не создаёт preview-файлы.

Визуальная группировка WPF editor реализована как presentation layer: `FolderMenuEntryGroupViewModel` группирует существующие `FolderMenuEntryViewModel` по `GroupName` для секций `Без группы`/`<GroupName>`. Это не добавляет Core tree model и не создаёт persistent empty group entities. `FolderMenuEntryViewModel` также хранит presentation-only state compact/edit (`IsEditing`) и inline validation errors. Эти состояния не попадают в `settings.json` и не меняют Core-модель.

Phase 4 добавляет в `Foldora.App` App-level `ExplorerIntegrationController`: WPF ViewModel вызывает controller, controller использует существующий `ExplorerMenuRegistrationService` из `Foldora.Shell`, а real registry access по-прежнему проходит только через `WindowsRegistryAccess`. Dry-run/register/unregister/reset являются отдельными явными commands. Dry-run/register требуют clean draft, чтобы registry отражал saved settings. Unregister можно выполнять при unsaved draft changes, потому что он не зависит от draft entries и сохраняет пользовательские entries/settings. После UX hardening обычный WPF `SaveAsync` rebuild-ит registry только если `ExplorerIntegrationEnabled` уже был `true`; при disabled integration Save пишет только settings. Settings/Explorer cleanup переносит dry-run/register/unregister, technical details и danger reset из главного редактора в `SettingsWindow`; `MainWindow` остаётся focused menu editor с компактным статусом Explorer menu, а Settings открываются через gear в title bar. UX cleanup добавляет только presentation properties (`HasEntries`, status/error/detail visibility state) и card/list/settings XAML; business logic остаётся в Core/Shell/App services, а code-behind не содержит бизнес-логики и не пишет registry.

Dev/manual publish layout создаётся script-ом `scripts/publish-dev.ps1` в `artifacts/publish/Foldora`, где `Foldora.App.exe`, `Foldora.Cli.exe` и `Foldora.MenuHost.exe` лежат рядом. Per-user install layout создаётся script-ом `scripts/install-user.ps1` в `%LocalAppData%\Programs\Foldora` с тем же sibling executable layout. WPF `ExplorerCommandHostPathResolver` сначала ищет sibling `Foldora.MenuHost.exe` рядом с текущим executable, затем Debug build output для Rider/dev workflow. Поэтому installed `Foldora.App.exe` регистрирует installed sibling `%LocalAppData%\Programs\Foldora\Foldora.MenuHost.exe`. Если host не найден, resolver выдаёт controlled failure; он не должен silently fallback-ить на console `Foldora.Cli.exe` или несуществующий путь.

Installed binaries и пользовательские данные разделены:

```text
%LocalAppData%\Programs\Foldora\  - application binaries
%AppData%\Foldora\                - settings, imported icons, packs, logs
```

`scripts/uninstall-user.ps1` удаляет installed binaries и Foldora-owned HKCU roots, но сохраняет `%AppData%\Foldora` по умолчанию. Это важно, потому что уже созданные Foldora folders могут иметь `desktop.ini`, который ссылается на imported `.ico` в `%AppData%\Foldora\icons`.

WPF shell/settings foundation добавляет custom window chrome через WPF `WindowChrome`. Code-behind остаётся window plumbing: кнопки minimize/maximize/close, state glyph и initial load. Settings UI находится в `SettingsWindow` + `SettingsViewModel`; открытие идёт через `ISettingsDialogService`, чтобы `MainViewModel` не создавал окно напрямую. SettingsWindow layout использует WPF `TabControl` для UI-only category navigation: Application, Explorer menu, Installation, Help/About and Danger zone. Selected tab не сохраняется в settings JSON и не добавляет ViewModel/domain state. `SettingsViewModel` отвечает за language save, Help/About entry point, Explorer integration section, installation/path information и danger reset presentation. Explorer actions выполняются сразу через `ExplorerIntegrationController` и не являются staged вместе с language Save. Installation path Open/Copy actions идут через App-layer `IPathActionService`; Windows implementation использует Explorer/Clipboard, тесты используют fake service, Core не участвует. Help/About открывается через App-layer `IHelpDialogService`; WPF implementation создаёт `HelpWindow` с `HelpWindowViewModel`, без Core/Shell dependency и без business logic в code-behind. Если reset или Explorer action меняет saved menu state, dialog result сообщает `MainViewModel` перезагрузить draft после закрытия SettingsWindow. Локализация находится в `Foldora.App`: `ILocalizationService`, catalog-backed `InMemoryLocalizationService`, `LocalizationResources`, `ValidationMessageLocalizer`, `SettingsLanguageInitializer` и embedded JSON catalogs `src/Foldora.App/Localization/*.json`. Core storage может сообщить, был ли `Language` явно persisted и supported, но не определяет язык системы. WPF initializer использует `CultureInfo.CurrentUICulture` через `ISystemLanguageProvider`, выбирает только complete enabled locales `bg`, `cs`, `de`, `en`, `es`, `fr`, `hi`, `hu`, `id`, `it`, `ja`, `ko`, `nl`, `pl`, `pt-BR`, `pt-PT`, `ro`, `ru`, `th`, `tr`, `uk`, `vi`, `zh-Hans`, `zh-Hant`, сохраняет результат один раз и не переопределяет ручной выбор на следующих стартах. Settings language dropdown показывает native display names, но сортирует их по стабильному English/common-name sort order. Exact `pt-BR` maps to `pt-BR`, exact `pt-PT` maps to `pt-PT`, and bare/other `pt-*` remains `pt-BR` for backward-compatible MVP behavior. Core не владеет UI language и не должен зависеть от App localization; WPF передаёт локализованные defaults при создании новых draft entries и локализует validation issues на App boundary, а Core fallback defaults/messages остаются только compatibility/CLI safety path.

WPF design system foundation находится в `Foldora.App/Resources`. `DesignTokens.xaml` задаёт semantic palette, brushes, spacing/radius/control-size tokens; `Typography.xaml` задаёт reusable text styles; `Controls.xaml` задаёт reusable control/container styles. Окна должны ссылаться на semantic resources, а будущая dark theme должна заменять semantic brushes, не копируя layout.

WPF app icon foundation находится в `Foldora.App/Assets`. `FoldoraIcon.svg` является self-authored source vector for the folded blue/cyan folder mark with a broad light-cyan folded plane, а `Foldora.ico` - generated Windows icon, подключённый как `Foldora.App` `ApplicationIcon` and WPF window icon resource для MainWindow, SettingsWindow and HelpWindow. Это App-layer branding asset; Core/Shell/MenuHost behavior и пользовательские imported menu icons в `%AppData%\Foldora\icons` не меняются.

Image-to-ICO conversion belongs outside Core and MenuHost. IC1 in `Foldora.Imaging` writes ICO containers from already encoded frame payloads. IC2a keeps `Foldora.Imaging` pure and adds `Foldora.Imaging.Windows` for PNG/JPG/JPEG/BMP decode to non-WPF `RgbaImage` and PNG frame payload encoding through WPF imaging APIs. IC2b adds pure alpha-aware `RgbaImageResizer` with Lanczos3-style separable filtering and premultiplied-alpha processing. A full image-to-ICO conversion service is still not implemented. Future WPF/CLI workflows may accept `.png`, `.jpg`, `.jpeg` and `.bmp`, convert each source into a multi-size `.ico`, then import or stage the generated `.ico` like a normal menu icon. SVG support remains a separate research topic because WPF is not a complete SVG renderer and full SVG rendering is too large for the first conversion milestone.

WPF startup path не должен блокировать dispatcher синхронным ожиданием async storage. `MainViewModel.CreateDefault()` только собирает сервисы; загрузка settings выполняется в `MainViewModel.LoadAsync`. Startup exceptions обрабатываются в `App.OnStartup`, `DispatcherUnhandledException` и `AppDomain.CurrentDomain.UnhandledException`; минимальный `StartupDiagnosticsService` пишет diagnostic file в `%AppData%\Foldora\Logs\startup-error.log`. Это не полноценный logging framework и не используется для доменной логики.

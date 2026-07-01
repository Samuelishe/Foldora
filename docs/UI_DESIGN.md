# UI Design

WPF editor содержит user-facing редактор menu entries с карточками, staged выбором `.ico`, прямым preview и компактным Explorer menu status. Явные Explorer integration controls находятся в SettingsWindow; Settings открываются через gear в title/header area.

WPF shell/settings foundation переводит окно на custom title bar через `WindowChrome`. Видимый стандартный Windows title bar не используется; в шапке находятся название `Foldora`, кнопка настроек с gear glyph и window controls minimize/maximize/close. Resize должен сохраняться, а maximize должен respect Windows work area/taskbar.

Startup bugfix сохраняет custom title bar и settings gear. Окно создаётся вручную в `App.OnStartup` после установки обработчиков startup errors; UI/domain logic по-прежнему остаётся вне code-behind. Если startup падает до показа окна, пользователь видит простой error dialog, а подробности пишутся в `%AppData%\Foldora\Logs\startup-error.log`.

WPF UX cleanup после one-level grouping заменяет emoji/font-dependent settings glyph на self-authored XAML vector gear. Внешние icon packs не используются.

Design system foundation добавляет централизованные WPF resource dictionaries:

- `DesignTokens.xaml` - semantic colors/brushes, spacing, radius and sizing tokens, включая icon-inspired light palette and shared accent/status/danger surfaces.
- `Typography.xaml` - reusable text styles, app font family, path text and chip text roles.
- `Controls.xaml` - reusable button, text input, checkbox, tab, card, group container, status chip, path row, empty state and status banner styles.

Будущая dark theme должна переопределять semantic colors/brushes, а не дублировать layout XAML.

Целевой WPF MVP описан подробно в `UX_FLOW.md`. Этот документ фиксирует короткие UI-правила, которые должны соблюдаться при реализации.

Правила:

- WPF code-behind только для UI plumbing.
- Бизнес-логика не размещается в окне.
- Настройки и операции вызываются через сервисы Core/Shell.
- MainWindow должен оставаться focused menu editor: title, groups, entries, icons, save/discard and editor status/errors.
- SettingsWindow содержит system/admin/support actions: language/application settings, Help/About, Explorer integration, installation/path information and danger reset.
- SettingsWindow system actions must use user-facing wording: `Preview changes` for dry-run registry preview, explicit `Foldora Explorer menu: On/Off` style status, and short contextual action labels such as `Enable` / `Disable` when the tab/section already says Explorer menu. Tooltips/help text carry the full Explorer legacy-menu explanation.
- Installation/path rows should be actionable: visible path, compact `Open` and `Copy` actions, with failures shown as localized status messages and tooltips explaining folder/file-location behavior.
- Small help/info affordances may use self-authored XAML/text styling. If an affordance is only a hover/focus hint, it must look like a passive info glyph, not a broken clickable button. If it looks like a button, it needs real click behavior. Do not add external icon assets for this.
- Long technical help tooltips must wrap within a reasonable width instead of rendering as a single line across the screen.
- Longer instructions belong in the Settings Help/About window, not in crowded inline tooltips or on MainWindow.
- Main UI text must be user-facing, not debug-state text. Не показывать raw booleans вроде `True/False` как пользовательский статус.
- Buttons must account for localization and long labels: shared action buttons use consistent min-height, padding and min-width, and the base button template must apply `Padding`, `HorizontalContentAlignment` and `VerticalContentAlignment` instead of visually clipping content. Dense Settings rows use a separate compact inline action style so `Preview changes`, enable/disable and path Open/Copy actions do not inherit overly wide normal action geometry, while still keeping visible horizontal breathing room. Further per-locale polish remains future work.
- Primary buttons on accent/gradient backgrounds must explicitly use the light `OnAccentBrush` foreground in normal, hover and pressed states. The base button template must forward `Foreground` to content so localized button text remains readable.
- Settings tab headers and selected tab content must keep separate alignment contracts: tab headers may center their header text inside each header chrome, but selected tab content must stretch to the tab body so forms/cards can start at the left/top property-panel inset.
- Settings content should keep proper scroll gutter and section spacing; scrollbar не должен визуально прилипать к тексту.
- App/window/exe icon foundation uses the self-authored folded blue/cyan Foldora app icon from `src/Foldora.App/Assets`; the current mark uses a broad light-cyan folded plane rather than a thin diagonal stroke. README hero/mockup and broader branding remain future branding/assets work, not ad hoc UI code additions.
- Интерфейс MVP должен быть простым: список стилей, состояние integration, кнопки register/unregister и базовые настройки.
- Главный экран MVP должен быть редактором пользовательского меню, а не landing page.
- Минимальный редактор должен иметь title меню, список entries, user-facing поля `Название в меню` и `Имя создаваемой папки`, выбор `.ico`, preview около 50x50, checkbox `Показывать в меню`, `Сохранить`, `Отменить изменения` и компактный Explorer menu status. Gear в title/header area остаётся основным entry point в Settings.
- Для one-level grouping текущий основной UI управляет группами через visual group containers; пустой `GroupName` означает пункт в корне меню.
- Entries визуально группируются по `GroupName`: пустая группа показывается как `Без группы`, непустые значения показываются отдельными group containers. Пустые группы не являются persistent model object; текущая модель остаётся entry-based.
- `+ Добавить группу` создаёт обычный draft entry с новым `GroupName`, а UI показывает это как новую группу с первым пунктом. Отдельная persisted group entity не создаётся.
- `Без группы` - специальная root-секция, а не удаляемая группа.
- Non-empty group container имеет заголовок, inline rename, staged delete-group confirmation и вложенные entry cards.
- Header group container показывает entry count; это presentation state, а не поле Core model или JSON.
- Удаление группы означает staged удаление всех entries с этим `GroupName`; settings и registry меняются только после `Сохранить`.
- Основной entry UI должен быть карточным/list-style, а не технической таблицей.
- `EntryId` не показывать в основном пользовательском flow.
- Technical registry plan details скрывать по умолчанию за раскрываемым блоком.
- Dry-run/preview action should be named for users. Internal code may keep `DryRun`, but visible UI should say `Предпросмотр изменений` / `Preview changes` and explain that HKCU registry is not modified.
- Dangerous reset отделять визуально от обычных integration controls.
- Редактор пользовательского меню должен работать через draft state и применять изменения только по кнопке `Сохранить`.
- При редактировании списка registry не трогать. `Сохранить` rebuild-ит registry только если Explorer integration уже была включена; при disabled integration пишет settings only. Registry operations также выполняются отдельными кнопками integration.
- Если settings сохранены, но registry rebuild упал, показывать: `Настройки сохранены, но меню Проводника не обновлено.`
- Ошибки показывать через inline validation, status area или список ошибок/предупреждений. `MessageBox` не должен быть основным UX-механизмом ошибок.
- Для `DefaultFolderName` при ручном вводе желательно блокировать invalid Windows filename chars; при paste допустимо заменить/удалить их и показать предупреждение. Validator всё равно обязан проверить данные при сохранении.
- Для preview в WPF MVP можно показывать `.ico` напрямую. Генерация файлов в `%AppData%\Foldora\previews\` остаётся future-задачей.
- Settings gear находится в title/header area и открывает настройки приложения.
- Language setting выбирается в settings UI. First-run language выбирается из system UI culture только для complete/enabled values `bg`, `cs`, `de`, `en`, `es`, `fr`, `hi`, `hu`, `id`, `it`, `ja`, `ko`, `nl`, `pl`, `pt-BR`, `pt-PT`, `ro`, `ru`, `th`, `tr`, `uk`, `vi`, `zh-Hans`, `zh-Hant`; unsupported system languages получают `en`, а сохранённый ручной выбор не переопределяется.
- Settings language dropdown показывает native display names в стабильном English/common-name sort order, чтобы порядок не зависел от истории добавления локалей или текущего UI language.
- Labels/buttons/status defaults подключены к App localization foundation. Новые user-facing строки в XAML/ViewModels должны идти через localization keys; incomplete planned locales не показываются.
- Смена языка обновляет untouched/default menu title, но не переводит custom menu title, entries или group names; новые entry defaults используют текущий UI language.

## Design System Foundation

Semantic resources are the source of visual truth for WPF. MainWindow and SettingsWindow should use named brushes/styles instead of local random hex values.

Core roles:

- `PageBackgroundBrush`;
- `SurfaceBrush`;
- `SurfaceSecondaryBrush`;
- `SurfaceAccentBrush`;
- `BorderBrush`;
- `BorderSoftBrush`;
- `BorderStrongBrush`;
- `TextPrimaryBrush`;
- `TextSecondaryBrush`;
- `TextDisabledBrush`;
- `AccentBrush`;
- `AccentCyanBrush`;
- `AccentVioletBrush`;
- `AccentGradientBrush`;
- `AccentSoftGradientBrush`;
- `DangerBrush`;
- `SuccessBrush`;
- `WarningBrush`;
- `FocusBrush`.

Typography styles:

- `WindowTitleTextStyle`;
- `PageTitleTextStyle`;
- `SectionTitleStyle`;
- `GroupTitleTextStyle`;
- `BodyTextStyle`;
- `SecondaryBodyTextStyle`;
- `CaptionTextStyle`;
- `PathTextStyle`;
- `ChipTextStyle`;
- `FieldLabelStyle`;
- `ValidationErrorTextStyle`;
- `StatusTextStyle`.

Control and container styles:

- `PrimaryButtonStyle` for the main action, currently `Сохранить`;
- `SecondaryButtonStyle` for normal actions;
- `DangerButtonStyle` for confirmed destructive actions;
- `InlineActionButtonStyle` for compact Settings rows and path actions;
- `IconButtonStyle` / `DangerIconButtonStyle` for compact chrome/local actions;
- `HelpInfoGlyphStyle` and `HelpTooltipTextStyle` for passive help glyphs and wrapped help text;
- `TextBoxStyle`, `CheckBoxStyle`, `ComboBoxStyle`;
- `SettingsTabControlStyle` and `SettingsTabItemStyle` for category navigation inside SettingsWindow;
- `CardContainerStyle`, `GroupContainerStyle`, `SectionContainerStyle`;
- `PageHeaderContainerStyle` for calm page/window content headers;
- `StatusChipStyle`, `StatusChipInfoStyle`, `StatusChipSuccessStyle`, `StatusChipWarningStyle`, `StatusChipDangerStyle` and compatibility `StatusPillStyle` for compact user-facing state such as Explorer menu and saved/unsaved state;
- `EmptyStateContainerStyle` and `EmptyStateIconContainerStyle` for first-run/no-entry states;
- `PathRowContainerStyle` for installation/user-data/MenuHost path rows;
- `HelpStepContainerStyle` and `HelpStepTextStyle` for short Help/About step rows;
- `SettingsExpanderStyle` for less dominant technical details in Settings;
- `FooterBarStyle` for fixed action/status footer areas;
- `StatusBannerStyle`, `DangerBannerStyle`;
- `PreviewBoxStyle`.

Disabled states must be visibly different from enabled states. Hover/pressed/focus states are part of reusable control styles, not one-off window markup.

## Visual Polish v1

Visual polish v1 is a controlled refinement of the existing MVP windows, not a redesign. It keeps the same information architecture and behavior:

- MainWindow remains the menu editor and now uses a page header surface, compact Explorer status chip, framed menu settings/menu entries sections, a calmer empty state, group/card hierarchy and a footer bar for saved/unsaved plus save/discard actions.
- SettingsWindow keeps Application, Help/About, Explorer menu, Installation and Danger zone sections, with shared section rhythm, passive help glyphs, compact inline buttons, path row containers and a softer danger banner.
- HelpWindow uses the same header/section rhythm as SettingsWindow and renders the basic workflow steps as readable boxed rows while keeping the localized content unchanged.
- App/window/exe icon foundation is now handled through a self-authored folded blue/cyan project asset. README hero/mockup and any external visual assets remain out of scope for visual polish v1 and require a separate branding/resource-policy review.
- RU/EN remain primary manually verified locales; other enabled locales are catalog-complete and should continue to be checked through smoke/spot checks and user feedback for long-label/font-fallback issues.

## Visual Design Direction v2

Visual Design Direction v2 keeps the same WPF information architecture and staged-save behavior, but raises the MVP from a careful prototype to a calmer product UI:

- shared tokens now use a soft cool page background, white/light surfaces, soft borders and a Foldora-icon-inspired blue/cyan/violet accent system;
- primary buttons use a restrained accent gradient, secondary/inline buttons stay quieter, and destructive buttons use calm danger surfaces until pressed;
- MainWindow keeps its editor-first flow, but the page header/status chips, empty state, footer and conditional status banner now feel like one system;
- the MainWindow empty state includes a small self-authored XAML folder/menu mark made from local `Path` geometry, not an external asset;
- SettingsWindow keeps category tabs and left/top tab-body alignment, while tabs, path rows, technical details and footer are visually more deliberate;
- HelpWindow keeps the same localized content, but shares the v2 header/section/step/footer rhythm;
- no localization catalogs, ViewModel state, settings JSON, registry/MenuHost/install behavior, app icon or README hero assets were changed.

## Settings Responsive / Action Polish

The Settings responsive/action polish pass keeps Visual Design Direction v2 and does not redesign the tab bar or Application tab:

- SettingsWindow uses a practical fixed/resizable starting size instead of dynamic `SizeToContent`: current width is `940`, minimum width is `920`, and `SizeToContent` remains `Manual`.
- Dynamic sizing by active tab is intentionally not implemented. WPF `SizeToContent=Height` or `WidthAndHeight` would make the modal window jump between tabs and fight the fixed footer plus tab-local scrolling model.
- Explorer menu action labels are contextual and short: `Preview changes`, `Enable`, `Disable` in English and `Предпросмотр изменений`, `Включить`, `Выключить` in Russian. The tab/section and help tooltip provide the full Explorer menu context.
- The Settings Help/About tab label is shortened to `Help` / `Справка`; the content heading and HelpWindow can still use fuller Help/About wording.
- Application tab content remains intentionally unchanged; do not add filler controls to occupy blank space.

## Layout Correctness

Custom title bar остаётся единственным местом, где приложение показывает имя `Foldora` как application title. В content area главное окно использует semantic page header:

- `Меню папок`;
- краткий subtitle о настройке пунктов контекстного меню Проводника.

Это убирает визуальное дублирование `Foldora` между title bar и содержимым окна.

Action buttons одного ряда используют общую геометрию через `ActionButtonStyle`. `PrimaryButtonStyle`, `SecondaryButtonStyle` и `DangerButtonStyle` отличаются цветовой семантикой, но не высотой, padding, border thickness или focus behavior. Локальные margins допустимы только для расстояния между кнопками в конкретном ряду.

Settings inline action rows are denser than main editor action rows. They use `InlineActionButtonStyle` so path Open/Copy buttons and Explorer actions remain compact while keeping readable padding. Help hints in these rows use passive `?` glyphs with wrapped tooltip text; they are not styled as primary or secondary buttons unless click behavior is implemented.

Action rows must not rely on fixed button widths. Buttons should size to content through padding/min-width, and rows with multiple localized actions should either use wrapping containers or reserve auto-sized action columns. MainWindow and SettingsWindow should remain resizable, but not below the practical width where core editor/settings rows visually break. Current MVP minimum widths are intentionally higher than the earliest prototype values: MainWindow protects the card/editor layout, and SettingsWindow protects tab headers plus Explorer/path action rows. Vertical scrolling remains the overflow mechanism; horizontal overflow should not be introduced.

Settings window является resizable и подготовлен к будущему росту настроек. Layout:

- header/title в верхней `Auto`-строке;
- settings content в `TabControl` with Application, Explorer menu, Installation, Help/About and Danger zone categories;
- Settings tab headers must be content-sized and non-clipping. The header host may wrap tabs to a second row rather than squeeze localized labels; tab headers must not use fixed small widths, max widths or text trimming.
- Settings tab bodies must not render as centered islands. Tab content roots stretch to the body, while forms/cards align to the left/top content margin. Constrained cards such as Danger zone may use `MaxWidth`, but must keep `HorizontalAlignment=Left`.
- tab content may use an internal `ScrollViewer` when a category needs overflow, but the window should not behave like one long settings document;
- Explorer action buttons live in a wrapping row; Installation path rows use star-sized path content plus auto-sized Open/Copy actions so long paths wrap without squeezing buttons;
- footer actions в нижней fixed `Auto`-строке.

Footer buttons `Сохранить`/`Закрыть` не прокручиваются вместе с содержимым и остаются доступными при уменьшении окна. Открытие modal settings window проверяется вручную пользователем; UIAutomation не является acceptance criterion для modal/custom-chrome WPF dialog.

Danger zone belongs in its own tab so reset is not visible as primary content when Settings opens. Installation path rows use short visible `Open`/`Copy` labels; tooltips can explain whether the action opens a folder directly or opens the containing folder for an executable path.

## Compact Entry Cards

Entry cards используют два presentation-only состояния:

- compact view state;
- inline edit state.

Сохранённые entries после загрузки открываются в compact state. Compact card показывает preview, `Название в меню`, краткое имя создаваемой папки, icon status, enabled toggle и компактные actions `Редактировать`/`Удалить`.

Новые draft entries сразу открываются в inline edit state. Edit state показывает поля `Название в меню`, `Имя создаваемой папки`, `Показывать в меню`, выбор `.ico`, icon status, inline validation errors и actions `Готово`/`Удалить`.

`Готово` сворачивает только presentation state. Оно не сохраняет `settings.json`, не импортирует иконки и не перестраивает registry. Единственное persistence action остаётся глобальная кнопка `Сохранить`.

Validation errors из Core validation layer рендерятся через App localization catalogs и показываются в общей status/errors area и inline на affected entry card. Карточка с ошибкой раскрывается и не сворачивается через `Готово`, пока inline errors не будут очищены новой попыткой save. После смены языка уже показанные ошибки обновляются при следующей validation/save attempt.

Текущее поведение допускает один основной раскрытый entry для обычного редактирования; карточки с validation errors могут оставаться раскрытыми, чтобы пользователь видел проблемы.

## Реализовано в phase 4

- Загрузка settings из `%AppData%\Foldora\settings.json`.
- Показ и редактирование `CreateFolderMenu.Title`.
- Показ существующих entries.
- Редактирование `DisplayName`, `DefaultFolderName`, `IsEnabled`.
- Добавление draft entry через `+ Добавить пункт`.
- Удаление draft entry.
- Выбор `.ico` через file picker.
- Staged import выбранных `.ico` только при `Сохранить`.
- Preview `.ico` около 50x50 для saved и pending icon path.
- `Сохранить` с validation перед записью settings.
- `Отменить изменения` с возвратом draft к сохранённому состоянию.
- SettingsWindow section `Explorer menu` со статусом, dry-run, register, unregister and technical details.
- SettingsWindow danger zone с reset.
- `Сохранить` обновляет меню Проводника, если integration уже была включена.
- `Предпросмотр изменений` и `Включить меню Проводника` требуют отсутствия unsaved changes.
- `Отключить меню Проводника` сохраняет entries/settings и может выполняться при unsaved draft changes.
- `Сбросить меню` требует checkbox-подтверждения, очищает entries и не удаляет импортированные `.ico`.
- Status area и список ошибок без `MessageBox` как основного механизма.

## Реализовано в UX cleanup phase 1

- `DataGrid` заменён на `ItemsControl` с карточками entries.
- Технические labels `DisplayName`, `DefaultFolderName`, `EntryId` убраны из основного UI.
- Карточка entry показывает preview, `Название в меню`, `Имя создаваемой папки`, `Показывать в меню`, icon status и действия `Выбрать .ico`/`Удалить`.
- После grouping MVP карточка временно показывала поле `Группа` с подсказкой, что пустое значение оставляет entry в root menu.
- После WPF grouping container cleanup карточки показываются внутри visual group containers `Без группы` или `<GroupName>`. Постоянное поле `Группа` убрано из основной карточки; group rename выполняется в заголовке контейнера.
- Visual reference для grouping использован только как грубая структура `группа -> элементы`, без копирования цветов, размеров, шрифтов или геометрии.
- Пустой список entries показывает empty state и кнопку `+ Добавить пункт`.
- Главный экран больше не содержит большой блок `Интеграция с Проводником` или `Опасная зона`; он сфокусирован на редактировании menu title/groups/entries/icons и глобальных `Сохранить`/`Отменить изменения`.
- Компактный Explorer status на главном экране допустим только как non-dominant строка без дублирующей content-area Settings action.
- `SettingsWindow` содержит секции Application, Help/About, Explorer menu, Installation и Danger zone. Dry-run/register/unregister, technical details и reset находятся там.
- Reset находится в отдельной `Опасная зона` SettingsWindow с подтверждением.
- Technical plan details показываются через `Expander` только при наличии details.
- Status/errors area отделена от technical details.

## Не реализовано

- Preview file generation/cache в `%AppData%\Foldora\previews`.
- Full localization cleanup оставшихся CLI/startup messages.
- Full tree grouping UI.
- Drag-and-drop.
- Orphan icon cleanup.
- Explorer restart и icon cache reset.
- User-facing diagnostics для MenuHost failures из Explorer context menu.

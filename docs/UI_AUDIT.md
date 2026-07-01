# UI / UX Audit

Дата: 2026-07-01.

## Current Status

WPF UI уже функционален: есть staged menu editor, compact/edit cards, grouping, локализация, SettingsWindow и перенос Explorer integration из главного окна. Visual polish pass v1 улучшил текущие окна без redesign: более спокойные header/status surfaces, ровнее section rhythm, polished empty state, path rows and Help step rows. Это не полный branding/public-presentation pass.

Этот документ фиксирует найденные UX/design issues, чтобы они не остались только в чате. Это не план немедленного redesign и не обещание реализации без отдельного этапа.

## Confirmed Issues

### UIA-0001 Redundant Settings Entry Points

- Observed: в title bar уже есть gear-кнопка настроек, но в content area MainWindow также есть `Manage in Settings` / `Управлять в настройках`.
- Impact: пользователь видит два входа в одно и то же место, главный editor снова получает системный action и визуально теряет фокус.
- Suggested direction: оставить один понятный entry point. Вероятный small cleanup: убрать content-area кнопку и оставить compact Explorer status без действия, либо заменить на очень ненавязчивый status chip, если статус нужен.
- Status: Addressed in small UX correctness cleanup. MainWindow keeps the title-bar gear as the settings entry point and shows only compact Explorer menu status in content.

### UIA-0002 Developer-Facing Unsaved Changes Status

- Observed: строка `Unsaved changes: True/False` и локализованные аналоги показывают raw boolean.
- Impact: это выглядит как debug/dev text, а не как пользовательский статус.
- Suggested direction: заменить на user-facing формулировки вроде `All changes saved` / `Unsaved changes` или локализованный status chip. Не показывать `True`/`False` пользователю.
- Status: Addressed. MainWindow now renders localized saved/unsaved status strings and does not show raw `True`/`False`.

### UIA-0003 Cramped Action Buttons

- Observed: в MainWindow и SettingsWindow некоторые кнопки выглядят тесно, текст находится слишком близко к краям.
- Impact: UI ощущается менее аккуратным, а длинные локализованные labels повышают риск визуального сжатия/обрезки.
- Suggested direction: сделать button geometry pass: consistent horizontal padding, min-width, min-height, wrapping/trimming rules and localized-label checks.
- Status: Addressed for current Settings/Main layouts. Shared button template now applies padding/alignment, action/inline action buttons have non-clipping min-size/padding without fixed widths, and Settings action rows use wrapping or star/auto layout. Further per-locale visual polish remains future feedback-driven work.

### UIA-0004 SettingsWindow Scrollbar / Content Gutter

- Observed: scrollbar визуально слишком близко к содержимому SettingsWindow.
- Impact: окно настроек выглядит плотным и менее polished; длинные секции и локализации сильнее подчёркивают проблему.
- Suggested direction: добавить scroll gutter / right content padding / layout hardening для SettingsWindow.
- Status: Addressed. SettingsWindow scroll content now has a right-side gutter while keeping the single ScrollViewer and fixed footer.

### UIA-0005 Product-Grade Visual Polish Gap

- Observed: приложение уже функциональное и стало лучше, но визуально всё ещё воспринимается как прототип.
- Impact: GitHub/public presentation и пользовательское доверие будут слабее, чем функциональная готовность MVP.
- Suggested direction: отдельный visual polish pass: hierarchy, spacing, surfaces, section rhythm, empty states, status presentation, entry cards and Settings sections.
- Status: Addressed for visual polish v1. MainWindow, SettingsWindow, HelpWindow and shared WPF resources now have calmer surface/card hierarchy, status chips, improved empty/path/help rows and footer/header rhythm. Further refinements should be user-feedback-driven rather than broad redesign.

### UIA-0006 Missing Application / Build / Window Icon

- Observed: нет нормальной app/build/window icon; exe/window/settings icon выглядит недоделанной.
- Impact: продуктовая идентичность не сформирована; installed/published app выглядит незавершённой.
- Suggested direction: отдельный branding/assets pass: app icon, exe icon, window icon, README visual assets. Любые bundled assets должны соблюдать `docs/RESOURCE_POLICY.md` и обновлять notices/license docs.

### UIA-0007 Settings Clarity And Help

- Observed: SettingsWindow system sections can still be unclear for non-technical users: `Status: Disabled` did not say which menu is disabled, `Проверить план` sounded technical, and installation paths were visible but not actionable.
- Impact: Explorer integration is functionally correct but requires too much inference from the user.
- Suggested direction: use explicit Explorer menu status wording, rename dry-run UI to `Preview changes`, add small tooltip/help affordances and add Open/Copy actions for paths.
- Status: Addressed for Settings clarity. A small Help/About/Instructions window foundation now exists in Settings; deeper help content remains future polish.

### UIA-0008 Settings Help And Inline Action Layout Regression

- Observed: after adding Settings help/tooltips and Open/Copy actions, small `?` affordances looked like clickable buttons but only showed hover tooltips, long tooltip text rendered as a single wide line, and Settings inline action rows became too wide because they reused the normal action button geometry.
- Impact: users can misread the help affordance as a broken button, tooltips are hard to read, and Settings sections look visually unstable.
- Suggested direction: use passive info glyphs unless click behavior exists, wrap long help tooltip text, and use a compact inline button style for Settings action rows/path actions.
- Status: Addressed. Settings now uses passive `?` glyphs with wrapped tooltips and compact inline action buttons for Explorer/path actions.

### UIA-0009 Window Minimum Widths For Settings And Editor Layouts

- Observed: SettingsWindow could be narrowed until Explorer/path action rows wrapped into visually broken layouts.
- Impact: the window remained technically resizable but allowed a width below the practical content minimum.
- Suggested direction: keep windows resizable, but set reasonable `MinWidth` values so core editor/settings rows do not collapse into broken narrow states.
- Status: Addressed for current MVP. SettingsWindow minimum width is raised for its system/action rows, and MainWindow minimum width is raised for the editor layout.

### UIA-0010 SettingsWindow Long Document Layout

- Observed: after visual polish v1, SettingsWindow still felt too spacious and scroll-heavy because Application, Explorer menu, Installation, Help/About and Danger zone were stacked as one long vertical document.
- Impact: common settings required unnecessary scrolling, the window used space inefficiently, and the model would scale poorly as more settings categories are added.
- Suggested direction: replace the single settings document with compact categories/tabs, keep footer actions fixed and isolate dangerous reset away from the default view.
- Status: Addressed. SettingsWindow now uses category tabs; each tab owns its compact content, vertical scroll exists only inside tab content when needed, and Danger zone is isolated in its own tab.

### UIA-0011 Settings Button Clipping And Resize Robustness

- Observed: after the tabbed Settings cleanup, long RU labels such as `Предпросмотр изменений`, `Включить меню Проводника` and `Отключить меню Проводника` could still look clipped or cramped, especially when the window was narrowed.
- Impact: the UI looked broken at allowed sizes even though the underlying commands worked.
- Suggested direction: make button sizing a shared style/template contract, keep action rows wrapping or auto-sized, and prevent SettingsWindow from resizing below its practical content minimum.
- Status: Addressed. The shared button template applies padding/content alignment, inline actions measure by content, SettingsWindow minimum width was raised, Explorer actions remain in a `WrapPanel`, and path rows use star path text plus auto action buttons.

### UIA-0012 Settings Tab Header Clipping

- Observed: after the button/layout robustness fix, SettingsWindow path/action buttons looked better, but the top Settings tab row still visually squeezed or clipped labels such as `Справка / О программе`.
- Impact: the settings categories looked broken even though tab content and action rows were now stable.
- Suggested direction: treat tab header layout separately from action button layout; tab headers should be content-sized, free of fixed small widths/trimming, and wrap cleanly if they need more than one row.
- Status: Addressed. Settings tab headers now use a wrapping content-sized host instead of WPF `TabPanel` layout, and the tab item style has explicit content alignment with no fixed width/max width/trimming.

### UIA-0013 Settings Tab Body Centered Islands

- Observed: after fixing tab headers, Application and Danger zone content appeared horizontally centered inside the tab body, unlike the Explorer menu tab which started at the left content margin.
- Impact: SettingsWindow looked like a floating centered form instead of a practical desktop settings/property page.
- Suggested direction: tab body roots should stretch to the available width, while actual forms/cards stay left/top aligned; constrained cards such as Danger zone should keep readable width but not center themselves.
- Status: Addressed. Settings tab content roots now stretch and Application, Help/About and Danger zone inner content is left/top aligned. Danger zone keeps a constrained card width.

## Near-Term UI Cleanup

Small safe improvements for a future code pass:

- remove or redesign `Manage in Settings` on MainWindow - addressed;
- replace boolean unsaved changes text with user-facing saved/unsaved status - addressed;
- button padding/min-width/min-height pass across MainWindow and SettingsWindow - addressed for current MVP; shared button template now applies padding and Settings action rows have robustness tests;
- SettingsWindow scrollbar gutter and content padding pass - addressed;
- check SettingsWindow sections with long labels in German, Portuguese, Ukrainian and other longer locales.
- keep Settings wording understandable: Explorer menu status, preview/dry-run action and path actions are now clarified.
- help/info affordances must not look like broken buttons: passive glyphs now use wrapped tooltips, and longer instructions live in the Settings Help/About window.
- Settings inline action/path buttons use compact geometry instead of the normal wide action button style, with enough horizontal padding for labels.
- MainWindow and SettingsWindow now have higher minimum widths to prevent known broken narrow layouts.
- Visual polish pass v1 is addressed for the current MVP windows: MainWindow, SettingsWindow, HelpWindow and shared resource styles have calmer spacing, surfaces, status and empty/path/help presentation.
- SettingsWindow tabbed layout cleanup is addressed: the previous long vertical settings document was replaced by category tabs, and Installation path actions use short `Open`/`Copy` labels.
- Settings layout robustness pass is addressed: SettingsWindow no longer allows the known too-narrow state that clipped action labels, and action/path rows have a shared sizing contract.
- Settings tab header clipping fix is addressed: tab header clipping was separate from button clipping, and Settings tabs now wrap as content-sized headers.
- Settings tab body centering is addressed: tab content now starts from the left/top inset while keeping readable constrained cards where useful.

## Later Visual Polish

Future product-grade polish can include:

- user-feedback-driven refinements to entry cards and group containers;
- additional localization-sensitive layout tweaks after manual reports;
- Settings tab spacing/content tuning after manual RU/EN and long-locale checks;
- more polished status/banner variants if real workflows need them;
- README hero/mockup only after the UI is visually ready for public presentation.
- deeper Help/About content polish: screenshots, richer instructions, and full translation review for long help text across all enabled locales.

## Branding / Assets Pass

Future branding work:

- application icon;
- exe icon;
- window icon;
- Settings/window icon consistency;
- README hero image or mockup built from improved screenshots;
- explicit resource/license review for every bundled visual asset.

Do not add unknown-license icons, fonts, images or generated third-party-looking assets. Follow `docs/RESOURCE_POLICY.md` and update `THIRD_PARTY_NOTICES.md` if any external resource is bundled.

## Localization-Sensitive Layout Risks

Known layout risks:

- long German/Portuguese/Ukrainian labels;
- CJK/Thai/Hindi rendering and font fallback;
- Settings dropdown width;
- button wrapping/trimming with long localized action labels;
- status text wrapping inside narrow windows.

Manual spot-check already covered Ukrainian, Japanese and German with no blocking layout issues. RU/EN remain primary verified. Other enabled locales are catalog-complete and test-covered, but visual polish may need user/community feedback.

Visual polish v1 did not manually verify every enabled locale. RU/EN remain the primary manually verified languages; the wider locale set remains catalog-complete and smoke/spot-check based, with German/Portuguese/Ukrainian long labels and Hindi/Thai/CJK font fallback still worth manual checks during future polish.

## Out Of Scope For This Audit

- no production rewrite;
- no full redesign now;
- no XAML/ViewModel changes in this documentation pass;
- no new shell features;
- no registry/MenuHost/install changes;
- no new languages;
- no app icon or README hero generation in this pass.

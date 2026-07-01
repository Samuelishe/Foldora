# UI / UX Audit

Дата: 2026-07-01.

## Current Status

WPF UI уже функционален: есть staged menu editor, compact/edit cards, grouping, локализация, SettingsWindow и перенос Explorer integration из главного окна. Текущий интерфейс стал чище после Settings cleanup, но по ручной проверке всё ещё выглядит как аккуратный прототип, а не как product-grade UI.

Этот документ фиксирует найденные UX/design issues, чтобы они не остались только в чате. Это не план немедленного redesign и не обещание реализации без отдельного этапа.

## Confirmed Issues

### UIA-0001 Redundant Settings Entry Points

- Observed: в title bar уже есть gear-кнопка настроек, но в content area MainWindow также есть `Manage in Settings` / `Управлять в настройках`.
- Impact: пользователь видит два входа в одно и то же место, главный editor снова получает системный action и визуально теряет фокус.
- Suggested direction: оставить один понятный entry point. Вероятный small cleanup: убрать content-area кнопку и оставить compact Explorer status без действия, либо заменить на очень ненавязчивый status chip, если статус нужен.

### UIA-0002 Developer-Facing Unsaved Changes Status

- Observed: строка `Unsaved changes: True/False` и локализованные аналоги показывают raw boolean.
- Impact: это выглядит как debug/dev text, а не как пользовательский статус.
- Suggested direction: заменить на user-facing формулировки вроде `All changes saved` / `Unsaved changes` или локализованный status chip. Не показывать `True`/`False` пользователю.

### UIA-0003 Cramped Action Buttons

- Observed: в MainWindow и SettingsWindow некоторые кнопки выглядят тесно, текст находится слишком близко к краям.
- Impact: UI ощущается менее аккуратным, а длинные локализованные labels повышают риск визуального сжатия/обрезки.
- Suggested direction: сделать button geometry pass: consistent horizontal padding, min-width, min-height, wrapping/trimming rules and localized-label checks.

### UIA-0004 SettingsWindow Scrollbar / Content Gutter

- Observed: scrollbar визуально слишком близко к содержимому SettingsWindow.
- Impact: окно настроек выглядит плотным и менее polished; длинные секции и локализации сильнее подчёркивают проблему.
- Suggested direction: добавить scroll gutter / right content padding / layout hardening для SettingsWindow.

### UIA-0005 Product-Grade Visual Polish Gap

- Observed: приложение уже функциональное и стало лучше, но визуально всё ещё воспринимается как прототип.
- Impact: GitHub/public presentation и пользовательское доверие будут слабее, чем функциональная готовность MVP.
- Suggested direction: отдельный visual polish pass: hierarchy, spacing, surfaces, section rhythm, empty states, status presentation, entry cards and Settings sections.

### UIA-0006 Missing Application / Build / Window Icon

- Observed: нет нормальной app/build/window icon; exe/window/settings icon выглядит недоделанной.
- Impact: продуктовая идентичность не сформирована; installed/published app выглядит незавершённой.
- Suggested direction: отдельный branding/assets pass: app icon, exe icon, window icon, README visual assets. Любые bundled assets должны соблюдать `docs/RESOURCE_POLICY.md` и обновлять notices/license docs.

## Near-Term UI Cleanup

Small safe improvements for a future code pass:

- remove or redesign `Manage in Settings` on MainWindow;
- replace boolean unsaved changes text with user-facing saved/unsaved status;
- button padding/min-width/min-height pass across MainWindow and SettingsWindow;
- SettingsWindow scrollbar gutter and content padding pass;
- check SettingsWindow sections with long labels in German, Portuguese, Ukrainian and other longer locales.

## Later Visual Polish

Future product-grade polish can include:

- refined entry cards and group containers;
- clearer Settings sections and section hierarchy;
- stronger empty states;
- more polished status banners/chips;
- calmer spacing rhythm and surface hierarchy;
- README hero/mockup only after the UI is visually ready for public presentation.

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

## Out Of Scope For This Audit

- no production rewrite;
- no full redesign now;
- no XAML/ViewModel changes in this documentation pass;
- no new shell features;
- no registry/MenuHost/install changes;
- no new languages;
- no app icon or README hero generation in this pass.

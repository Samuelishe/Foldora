# Localization

## Purpose

Этот документ фиксирует правила локализации Foldora. Он отделяет UI/runtime строки от пользовательских данных, чтобы смена языка приложения не портила сохранённое меню и не меняла уже созданные сценарии пользователя.

## Supported locales

Complete and enabled locales:

- `ru` - Русский
- `en` - English
- `zh-Hans` - 简体中文
- `de` - Deutsch
- `es` - Español
- `fr` - Français
- `ja` - 日本語
- `pt-BR` - Português (Brasil)
- `ko` - 한국어

Settings UI must expose only complete locales. Incomplete planned locales are not shown unless a separate explicit experimental state is designed.

## Planned locales

Later candidates:

- `it`
- `pl`
- `tr`
- `uk`

RTL languages such as Arabic or Hebrew are not part of the current plan because RTL layout verification is a separate UI task.

## Language code policy

Foldora uses canonical BCP-47-style tags where practical:

- `ru`
- `en`
- `zh-Hans`
- `de`
- `es`
- `fr`
- `ja`
- `pt-BR`
- `ko`

`FoldoraLanguage` accepts and normalizes only complete enabled locales. Unsupported saved values normalize to `en` and are persisted as `en` by the WPF startup language initializer.

`pt-BR` is currently the only Portuguese catalog shipped by Foldora. First-run detection maps generic `pt` and `pt-*` cultures to `pt-BR` until a separate Portuguese locale is intentionally added.

## String categories

UI labels are visible interface text such as `Save`, `Discard changes`, `Choose .ico`, group buttons and settings labels. They must come from the localization layer.

Runtime/status messages are visible state messages such as settings loaded, settings saved, Explorer integration enabled/disabled and draft entry added. They must come from the localization layer.

Validation messages are localized at the App/UI boundary for the WPF editor. Core returns invariant issue codes and parameters; App maps those to localized text through the enabled catalogs.

New object defaults are localized values used when the user creates a new object in the current UI language:

- root menu title default, for example `Создать папку`, `Create folder`, `Ordner erstellen`
- new entry display name prefix, for example `Вид`, `View`, `Vista`
- new folder default name, for example `Новая папка`, `New folder`, `新しいフォルダー`

Existing user data is saved data such as custom menu title, entry display names, folder names and custom group names. Existing user data is not auto-translated when the application language changes.

## Core boundary

Core must not own UI language or create localized user-facing text for normal UI flows.

Core may keep compatibility fallback values where needed for old settings, CLI behavior or deserialization safety, but App should pass localized defaults when creating new UI draft entries.

Core returns invariant validation codes, issue types and parameters. App/ViewModels convert those into localized messages. Core `Message` values are compatibility/debug fallback text and are not the preferred WPF rendering path.

Core must not depend on `Foldora.App`.

## ViewModel and XAML rules

ViewModels should not hardcode user-facing strings outside the localization layer.

XAML should not hardcode user-facing strings except temporary placeholders tracked as technical debt.

New UI text requires:

- a localization key;
- values in every complete catalog;
- tests or catalog-completeness coverage.

## Saved user data

Changing application language changes UI labels, status messages and defaults for objects created after the change.

Changing application language does not rewrite custom saved menu title, entry display names, default folder names or group names.

Menu title is user-editable, but it has a default-title mode. If the title has not been customized, it follows the current complete locale catalog.

`FolderMenuSettings.TitleIsCustom` tracks whether the user explicitly edited the title. If `TitleIsCustom = true`, language changes do not rewrite the title even if it equals a known default string.

Migration/inference for old settings without `titleIsCustom`:

- null/empty/missing title: default-title mode;
- title equal to any known complete-locale default: default-title mode;
- any other title: custom title.

Edge case: an old pre-flag settings file with title exactly equal to a known product default is treated as untouched default, even if the user had intentionally typed that same value before the flag existed. After this migration, explicitly editing the title marks it custom and preserves it across language changes.

## First-run locale detection

WPF first-run language selection happens before the draft menu is loaded:

1. If `settings.json` contains a supported complete `language`, Foldora uses it and does not inspect the system language.
2. If `settings.json` is missing or does not contain `language`, Foldora reads `CultureInfo.CurrentUICulture`.
3. `ru`, `ru-RU` and other `ru-*` cultures map to `ru`.
4. `en`, `en-US`, `en-GB` and other `en-*` cultures map to `en`.
5. `zh`, `zh-CN`, `zh-SG`, `zh-Hans` and `zh-Hans-*` map to `zh-Hans`.
6. `de`, `es`, `fr`, `ja` and `ko` culture families map to their matching enabled locale.
7. `pt`, `pt-BR` and `pt-*` map to `pt-BR`.
8. Any other system culture maps to `en`.
9. The selected language is saved to `settings.json`, so later startups do not auto-detect again.
10. If `settings.json` contains an unsupported language value such as `it`, Foldora normalizes and saves `en`; it does not keep re-running system detection.

Manual language selection in Settings always wins over system language after it is saved. Later candidate locales such as `it`, `pl`, `tr` and `uk` are not selected automatically until their catalogs are complete and exposed.

## New entry defaults

When the WPF editor creates a new entry, it uses the current UI language:

- `ru`: `Вид N` and `Новая папка`
- `en`: `View N` and `New folder`
- `zh-Hans`: `视图 N` and `新建文件夹`
- `de`: `Ansicht N` and `Neuer Ordner`
- `es`: `Vista N` and `Nueva carpeta`
- `fr`: `Vue N` and `Nouveau dossier`
- `ja`: `ビュー N` and `新しいフォルダー`
- `pt-BR`: `Visualização N` and `Nova pasta`
- `ko`: `보기 N` and `새 폴더`

The `N` suffix is still generated from existing draft entries. Existing entries keep their saved values.

CLI currently keeps Core fallback/default behavior and is tracked as localization debt. It must not introduce a dependency on WPF/App localization.

## Menu title default policy

Default menu title is localized for complete enabled locales. WPF displays and saves the current localized default while `TitleIsCustom = false`. Menu reset restores default-title mode. Registry/menu plan uses the saved effective title.

Core/Shell contain only a small compatibility list of known complete-locale default titles so storage/registry flows can infer old settings and avoid depending on `Foldora.App`.

## Validation localization policy

Core validation issues contain:

- stable invariant `Code`;
- `Severity`;
- optional `EntryId`;
- optional parameter dictionary for values such as invalid character, max length, actual length, group name, file path or extension;
- compatibility/debug `Message` fallback.

WPF summary errors and inline entry-card errors render validation issues through `ValidationMessageLocalizer` in `Foldora.App`. Catalog keys use the format:

```text
Validation.<issue_code>
```

Example:

```text
Code: folder_name_invalid_chars
Parameters:
  character: ":"
```

renders as:

- `ru`: `Имя создаваемой папки содержит недопустимый символ ":".`
- `en`: `Created folder name contains invalid character ":".`

Validation templates support simple `{parameterName}` replacement. There is no pluralization framework yet. If a validation key is missing, App falls back to Core `Message`; catalog completeness tests should prevent that for complete WPF locales.

Visible validation messages already shown in the UI are string snapshots. After a language switch, new validation attempts render in the new language; existing shown errors are refreshed on the next validation/save action.

## Fallback policy

The current WPF localization engine loads embedded JSON catalogs from `src/Foldora.App/Localization/*.json`.

`ru` is the fallback catalog. Non-Russian catalogs are merged over `ru`, so a missing key has a controlled fallback. Tests require every enabled catalog to have the same keys as `en` and no empty values.

Unsupported requested application language codes normalize to `en`. This is separate from catalog key fallback: complete non-Russian catalogs still merge over `ru` for missing keys.

No third-party localization libraries are used.

## Completeness tests

Localization tests must cover:

- enabled catalogs have the same keys;
- fallback behavior is controlled;
- known default/status keys exist;
- settings UI exposes only complete locales;
- first-run language detection persists every enabled locale and maps unsupported locales to `en`;
- new draft entries use localized defaults;
- existing user data is not auto-translated on language switch.

## Hardcoded string audit

Useful audit commands:

```powershell
rg -n --glob '!bin/**' --glob '!obj/**' --glob '!artifacts/**' "[А-Яа-яЁё]" src tests README.md docs AGENTS.md
rg -n --glob '!bin/**' --glob '!obj/**' --glob '!artifacts/**' '"[^"]*[А-Яа-яЁё][^"]*"' src tests
```

The first command is intentionally broad and includes docs/tests. The second command is more useful for production source literals, but tests and localization catalogs must be interpreted with context.

Current audit result:

- localization catalogs intentionally contain localized strings;
- tests intentionally contain Russian sample user data;
- Core/Shell still contain compatibility defaults for menu title inference and old CLI/default paths;
- CLI diagnostic output has Russian manual instructions;
- startup fatal error dialog still uses hardcoded Russian;
- Core validation `Message` fallbacks remain English debug/CLI text, but WPF renders validation issues through localized catalog keys.

## Adding a language

To add a complete language:

1. Add a catalog with the canonical language tag.
2. Keep the same keys as `ru` and `en`.
3. Add native display name for future settings UI.
4. Add/extend tests for key completeness and selected known strings.
5. Expose the locale in Settings and first-run detection only after the catalog is complete and reviewed.
6. Update this document, `docs/SETTINGS.md`, `docs/ROADMAP.md` and `docs/FILE_INDEX.md`.

## Current known gaps

- Core/Shell compatibility fallbacks remain for entry/folder defaults and startup/CLI debt; menu title default now has explicit complete-locale default-title tracking.
- CLI default text and diagnostic messages are not fully localized.
- CLI validation output still uses Core fallback messages.
- Startup fatal error dialog is hardcoded Russian.
- Technical plan/details text in Explorer integration is not fully localized.
- `InMemoryLocalizationService` is now catalog-backed but still has its early MVP name.

## Future work

- CLI validation localization strategy.
- CLI localization strategy.
- Add later candidate locale catalogs when they are complete.
- Pluralization rules.
- RTL support after dedicated layout testing.
- External translator review for enabled catalogs before a stable public release.
- External translator review before exposing additional languages.

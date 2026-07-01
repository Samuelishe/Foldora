# Localization

## Purpose

Этот документ фиксирует правила локализации Foldora. Он отделяет UI/runtime строки от пользовательских данных, чтобы смена языка приложения не портила сохранённое меню и не меняла уже созданные сценарии пользователя.

## Supported locales

Complete and enabled locales:

- `ru` - Русский
- `en` - English

Settings UI must expose only complete locales. Incomplete planned locales are not shown unless a separate explicit experimental state is designed.

## Planned locales

Next planned batch:

- `zh-Hans` - 简体中文
- `de` - Deutsch
- `es` - Español
- `fr` - Français
- `ja` - 日本語
- `pt-BR` - Português (Brasil)
- `ko` - 한국어

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

Current `FoldoraLanguage` accepts and normalizes only complete enabled locales `ru` and `en`. Unsupported saved values fall back to `ru` to keep old or corrupted settings loadable.

## String categories

UI labels are visible interface text such as `Save`, `Discard changes`, `Choose .ico`, group buttons and settings labels. They must come from the localization layer.

Runtime/status messages are visible state messages such as settings loaded, settings saved, Explorer integration enabled/disabled and draft entry added. They must come from the localization layer.

Validation messages should be localized at the App/UI boundary. Core should prefer invariant issue codes and parameters; App maps those to localized text. Full validation-message localization is not complete yet.

New object defaults are localized values used when the user creates a new object in the current UI language:

- root menu title default: `Создать папку` / `Create folder`
- new entry display name prefix: `Вид` / `View`
- new folder default name: `Новая папка` / `New folder`

Existing user data is saved data such as menu title, entry display names, folder names and custom group names. Existing user data is not auto-translated when the application language changes.

## Core boundary

Core must not own UI language or create localized user-facing text for normal UI flows.

Core may keep compatibility fallback values where needed for old settings, CLI behavior or deserialization safety, but App should pass localized defaults when creating new UI draft entries.

Core may return invariant validation codes, issue types and parameters. App/ViewModels convert those into localized messages.

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

Changing application language does not rewrite saved menu title, entry display names, default folder names or group names.

Menu title is user data after it is saved. A future first-run setup may choose a locale-aware initial title, but must not corrupt existing settings.

## New entry defaults

When the WPF editor creates a new entry, it uses the current UI language:

- `ru`: `Вид N` and `Новая папка`
- `en`: `View N` and `New folder`

The `N` suffix is still generated from existing draft entries. Existing entries keep their saved values.

CLI currently keeps Core fallback/default behavior and is tracked as localization debt. It must not introduce a dependency on WPF/App localization.

## Menu title default policy

The current persisted default menu title in Core remains `Создать папку` for compatibility. App-level locale-aware first-run title behavior is future work because changing it carelessly could rewrite user data.

## Validation localization policy

Validation should move toward invariant issue codes with localized App-level rendering. Current Core validation messages are partly user-facing and remain technical debt.

## Fallback policy

The current WPF localization engine loads embedded JSON catalogs from `src/Foldora.App/Localization/*.json`.

`ru` is the fallback catalog. Non-Russian catalogs are merged over `ru`, so a missing key has a controlled fallback. Tests require complete `ru` and `en` catalogs to have the same keys.

No third-party localization libraries are used.

## Completeness tests

Localization tests must cover:

- enabled catalogs have the same keys;
- fallback behavior is controlled;
- known default/status keys exist;
- settings UI exposes only complete locales;
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
- Core/Shell still contain compatibility defaults `Создать папку`, `Вид`, `Новая папка`;
- CLI diagnostic output has Russian manual instructions;
- startup fatal error dialog still uses hardcoded Russian;
- validation messages are not fully App-localized yet.

## Adding a language

To add a complete language:

1. Add a catalog with the canonical language tag.
2. Keep the same keys as `ru` and `en`.
3. Add native display name for future settings UI.
4. Add/extend tests for key completeness and selected known strings.
5. Expose the locale in Settings only after the catalog is complete and reviewed.
6. Update this document, `docs/SETTINGS.md`, `docs/ROADMAP.md` and `docs/FILE_INDEX.md`.

## Current known gaps

- Core/Shell compatibility fallbacks are still Russian.
- CLI default text and diagnostic messages are not fully localized.
- Validation messages are not fully localized.
- Startup fatal error dialog is hardcoded Russian.
- First-run menu title is not locale-aware.
- Technical plan/details text in Explorer integration is not fully localized.
- `InMemoryLocalizationService` is now catalog-backed but still has its early MVP name.

## Future work

- Full validation message localization.
- CLI localization strategy.
- First-run locale-aware menu title without rewriting existing user data.
- Pluralization rules.
- RTL support after dedicated layout testing.
- Complete translation pass for planned locales.
- External translator review before exposing additional languages.

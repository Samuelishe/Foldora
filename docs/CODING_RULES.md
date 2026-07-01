# Coding Rules

- C# nullable включен.
- Использовать строгую типизацию.
- Публичные контракты документировать XML-doc там, где это полезно.
- Не использовать широкие `catch` без причины.
- Не глотать исключения молча.
- Не блокировать UI-поток.
- Использовать `async`/`await` для потенциально долгих операций.
- Использовать `CancellationToken`, где уместно.
- WPF code-behind только для UI plumbing.
- Core не зависит от WPF.
- Registry logic не находится в Core.
- CLI тонкий, вызывает Core/Shell сервисы.
- Filesystem operations должны быть тестируемыми.
- Пути строить через `Path.Combine`, `Environment.GetFolderPath`, `FileInfo`, `DirectoryInfo`.
- Не собирать пути конкатенацией строк.
- Учитывать пробелы, кириллицу и спецсимволы.
- Command line strings для registry собирать с корректным quoting.
- Не хранить пользовательские настройки рядом с exe.
- Не добавлять сторонние ресурсы без явной лицензии.
- Перед добавлением ресурса проверять право на redistribution, modification, commercial use, attribution requirements и необходимость bundled license text.
- Новые bundled third-party resources должны одновременно обновлять `THIRD_PARTY_NOTICES.md`, `docs/FILE_INDEX.md` и license files, если они требуются.

## Localization rules

- Не добавлять новые user-facing string literals в ViewModels, XAML, App services или Shell/App presentation code вне localization layer.
- Новый UI/status text должен иметь localization key, значения во всех complete catalogs и тестовое покрытие key completeness.
- Core не должен зависеть от `Foldora.App` и не должен решать UI language; Core может возвращать invariant codes/params или compatibility fallback values.
- Core validation issues должны иметь stable invariant code и параметры для dynamic values; WPF/App обязаны рендерить их через localization layer, а не через Core fallback `Message`.
- Saved user data не переводится автоматически при смене языка.
- New object defaults для WPF создаются в текущем UI language на App layer.
- Incomplete planned locales не показываются в Settings UI без отдельного explicit experimental state.

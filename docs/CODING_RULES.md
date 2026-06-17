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

# UI Design

Bootstrap WPF UI содержит только окно-заглушку `Foldora`.

Правила:

- WPF code-behind только для UI plumbing.
- Бизнес-логика не размещается в окне.
- Настройки и операции вызываются через сервисы Core/Shell.
- Интерфейс MVP должен быть простым: список стилей, состояние integration, кнопки register/unregister и базовые настройки.

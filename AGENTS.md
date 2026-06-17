# AGENTS.md

Правила для будущих агентных сессий Foldora:

- Отвечать пользователю на русском языке.
- Комментарии и XML-doc писать на русском языке.
- Перед работой читать:
  - `docs/PROJECT_STATE.md`;
  - `docs/ROADMAP.md`;
  - `docs/WORK_LOG.md`;
  - `docs/ARCHITECTURE.md`;
  - `docs/REQUIREMENTS.md`;
  - `docs/SETTINGS.md`;
  - `docs/SHELL_INTEGRATION.md`;
  - `docs/DESKTOP_INI.md`;
  - `docs/PACKS.md`;
  - `docs/CLI.md`;
  - `docs/CODING_RULES.md`;
  - `docs/FILE_INDEX.md`.
- После значимых изменений обновлять `docs/PROJECT_STATE.md`.
- После значимых изменений добавлять запись в `docs/WORK_LOG.md`.
- При добавлении, удалении или переносе файлов обновлять `docs/FILE_INDEX.md`.
- При изменении архитектуры обновлять `docs/ARCHITECTURE.md`.
- При изменении настроек обновлять `docs/SETTINGS.md`.
- При изменении shell integration обновлять `docs/SHELL_INTEGRATION.md`.
- При изменении desktop.ini механизма обновлять `docs/DESKTOP_INI.md`.
- При изменении CLI обновлять `docs/CLI.md`.
- Не делать commit без прямого запроса пользователя.
- В конце значимого шага предлагать commit message на английском, 1-2 предложения.
- Не делать крупные изменения без краткого плана.
- Не смешивать UI, filesystem, registry и доменную логику.
- Не помещать бизнес-логику в WPF code-behind.
- Не делать COM shell extension без отдельного явного этапа.
- Не патчить системные DLL.
- Не добавлять фоновые сервисы без отдельного решения.
- Сначала рабочий MVP, потом shell-extension и визуальная полировка.

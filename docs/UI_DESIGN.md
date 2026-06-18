# UI Design

WPF editor phase 4 содержит минимальный редактор пользовательских menu entries с add/remove, staged выбором `.ico`, прямым preview и явными Explorer integration controls.

Целевой WPF MVP описан подробно в `UX_FLOW.md`. Этот документ фиксирует короткие UI-правила, которые должны соблюдаться при реализации.

Правила:

- WPF code-behind только для UI plumbing.
- Бизнес-логика не размещается в окне.
- Настройки и операции вызываются через сервисы Core/Shell.
- Интерфейс MVP должен быть простым: список стилей, состояние integration, кнопки register/unregister и базовые настройки.
- Главный экран MVP должен быть редактором пользовательского меню, а не landing page.
- Минимальный редактор должен иметь title меню, список entries, `DisplayName`, `DefaultFolderName`, выбор `.ico`, preview около 50x50, `IsEnabled`, `Сохранить`, `Отменить изменения`, `Включить меню Проводника`, `Отключить меню Проводника`, `Сбросить меню`.
- Редактор пользовательского меню должен работать через draft state и применять изменения только по кнопке `Сохранить`.
- При редактировании списка registry не трогать; обычный `Сохранить` пишет settings only. Registry operations выполняются только отдельными кнопками integration.
- Если settings сохранены, но registry rebuild в будущем упал, показывать: `Настройки сохранены, но меню Проводника не обновлено.`
- Ошибки показывать через inline validation, status area или список ошибок/предупреждений. `MessageBox` не должен быть основным UX-механизмом ошибок.
- Для `DefaultFolderName` при ручном вводе желательно блокировать invalid Windows filename chars; при paste допустимо заменить/удалить их и показать предупреждение. Validator всё равно обязан проверить данные при сохранении.
- Для preview в WPF MVP можно показывать `.ico` напрямую. Генерация файлов в `%AppData%\Foldora\previews\` остаётся future-задачей.

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
- Блок `Интеграция с Проводником` со статусом, dry-run, register, unregister и reset.
- `Проверить план` и `Включить меню Проводника` требуют отсутствия unsaved changes.
- `Отключить меню Проводника` сохраняет entries/settings и может выполняться при unsaved draft changes.
- `Сбросить меню` требует checkbox-подтверждения, очищает entries и не удаляет импортированные `.ico`.
- Status area и список ошибок без `MessageBox` как основного механизма.

## Не реализовано в phase 4

- Preview file generation/cache в `%AppData%\Foldora\previews`.
- Registry rebuild после обычного `Сохранить`.
- Drag-and-drop.
- Orphan icon cleanup.
- Explorer restart и icon cache reset.

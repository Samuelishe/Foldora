# UI Design

WPF editor phase 2 содержит минимальный редактор пользовательских menu entries с add/remove и staged выбором `.ico`.

Целевой WPF MVP описан подробно в `UX_FLOW.md`. Этот документ фиксирует короткие UI-правила, которые должны соблюдаться при реализации.

Правила:

- WPF code-behind только для UI plumbing.
- Бизнес-логика не размещается в окне.
- Настройки и операции вызываются через сервисы Core/Shell.
- Интерфейс MVP должен быть простым: список стилей, состояние integration, кнопки register/unregister и базовые настройки.
- Главный экран MVP должен быть редактором пользовательского меню, а не landing page.
- Минимальный редактор должен иметь title меню, список entries, `DisplayName`, `DefaultFolderName`, выбор `.ico`, preview около 50x50, `IsEnabled`, `Сохранить`, `Отменить изменения`, `Включить меню Проводника`, `Отключить меню Проводника`, `Сбросить меню`.
- Редактор пользовательского меню должен работать через draft state и применять изменения только по кнопке `Сохранить`.
- При редактировании списка registry не трогать; registry rebuild делать только после успешного validate/save.
- Если settings сохранены, но registry rebuild в будущем упал, показывать: `Настройки сохранены, но меню Проводника не обновлено.`
- Ошибки показывать через inline validation, status area или список ошибок/предупреждений. `MessageBox` не должен быть основным UX-механизмом ошибок.
- Для `DefaultFolderName` при ручном вводе желательно блокировать invalid Windows filename chars; при paste допустимо заменить/удалить их и показать предупреждение. Validator всё равно обязан проверить данные при сохранении.
- Для preview в WPF MVP можно показывать `.ico` напрямую. Генерация файлов в `%AppData%\Foldora\previews\` остаётся future-задачей.

## Реализовано в phase 2

- Загрузка settings из `%AppData%\Foldora\settings.json`.
- Показ и редактирование `CreateFolderMenu.Title`.
- Показ существующих entries.
- Редактирование `DisplayName`, `DefaultFolderName`, `IsEnabled`.
- Добавление draft entry через `+ Добавить пункт`.
- Удаление draft entry.
- Выбор `.ico` через file picker.
- Staged import выбранных `.ico` только при `Сохранить`.
- `Сохранить` с validation перед записью settings.
- `Отменить изменения` с возвратом draft к сохранённому состоянию.
- Status area и список ошибок без `MessageBox` как основного механизма.

## Не реализовано в phase 2

- Preview иконок.
- Registry rebuild после save.
- Кнопки `Включить меню Проводника`, `Отключить меню Проводника`, `Сбросить меню`.
- Drag-and-drop.
- Orphan icon cleanup.

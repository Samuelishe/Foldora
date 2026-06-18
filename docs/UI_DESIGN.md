# UI Design

WPF editor после UX cleanup phase 1 содержит user-facing редактор menu entries с карточками, staged выбором `.ico`, прямым preview и явными Explorer integration controls.

WPF shell/settings foundation переводит окно на custom title bar через `WindowChrome`. Видимый стандартный Windows title bar не используется; в шапке находятся название `Foldora`, кнопка настроек с gear glyph и window controls minimize/maximize/close. Resize должен сохраняться, а maximize должен respect Windows work area/taskbar.

Startup bugfix сохраняет custom title bar и settings gear. Окно создаётся вручную в `App.OnStartup` после установки обработчиков startup errors; UI/domain logic по-прежнему остаётся вне code-behind. Если startup падает до показа окна, пользователь видит простой error dialog, а подробности пишутся в `%AppData%\Foldora\Logs\startup-error.log`.

WPF UX cleanup после one-level grouping заменяет emoji/font-dependent settings glyph на self-authored XAML vector gear. Внешние icon packs не используются.

Целевой WPF MVP описан подробно в `UX_FLOW.md`. Этот документ фиксирует короткие UI-правила, которые должны соблюдаться при реализации.

Правила:

- WPF code-behind только для UI plumbing.
- Бизнес-логика не размещается в окне.
- Настройки и операции вызываются через сервисы Core/Shell.
- Интерфейс MVP должен быть простым: список стилей, состояние integration, кнопки register/unregister и базовые настройки.
- Главный экран MVP должен быть редактором пользовательского меню, а не landing page.
- Минимальный редактор должен иметь title меню, список entries, user-facing поля `Название в меню` и `Имя создаваемой папки`, выбор `.ico`, preview около 50x50, checkbox `Показывать в меню`, `Сохранить`, `Отменить изменения`, `Включить меню Проводника`, `Отключить меню Проводника`, `Сбросить меню`.
- Для one-level grouping карточка entry имеет поле `Группа`; пустое значение означает пункт в корне меню.
- Entries визуально группируются по `GroupName`: пустая группа показывается как `Без группы`, непустые значения показываются отдельными group containers. Пустые группы не являются persistent model object; текущая модель остаётся entry-based.
- `+ Добавить группу` создаёт обычный draft entry с новым `GroupName`, а UI показывает это как новую группу с первым пунктом. Отдельная persisted group entity не создаётся.
- `Без группы` - специальная root-секция, а не удаляемая группа.
- Non-empty group container имеет заголовок, inline rename, staged delete-group confirmation и вложенные entry cards.
- Удаление группы означает staged удаление всех entries с этим `GroupName`; settings и registry меняются только после `Сохранить`.
- Основной entry UI должен быть карточным/list-style, а не технической таблицей.
- `EntryId` не показывать в основном пользовательском flow.
- Technical registry plan details скрывать по умолчанию за раскрываемым блоком.
- Dangerous reset отделять визуально от обычных integration controls.
- Редактор пользовательского меню должен работать через draft state и применять изменения только по кнопке `Сохранить`.
- При редактировании списка registry не трогать. `Сохранить` rebuild-ит registry только если Explorer integration уже была включена; при disabled integration пишет settings only. Registry operations также выполняются отдельными кнопками integration.
- Если settings сохранены, но registry rebuild упал, показывать: `Настройки сохранены, но меню Проводника не обновлено.`
- Ошибки показывать через inline validation, status area или список ошибок/предупреждений. `MessageBox` не должен быть основным UX-механизмом ошибок.
- Для `DefaultFolderName` при ручном вводе желательно блокировать invalid Windows filename chars; при paste допустимо заменить/удалить их и показать предупреждение. Validator всё равно обязан проверить данные при сохранении.
- Для preview в WPF MVP можно показывать `.ico` напрямую. Генерация файлов в `%AppData%\Foldora\previews\` остаётся future-задачей.
- Settings gear находится в title/header area и открывает настройки приложения.
- Language setting выбирается в settings UI. Default language: `ru`; supported values: `ru`, `en`.
- Основные labels/buttons подключены к минимальному localization foundation. Полная локализация всех status/error strings остаётся future cleanup.

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
- `Сохранить` обновляет меню Проводника, если integration уже была включена.
- `Проверить план` и `Включить меню Проводника` требуют отсутствия unsaved changes.
- `Отключить меню Проводника` сохраняет entries/settings и может выполняться при unsaved draft changes.
- `Сбросить меню` требует checkbox-подтверждения, очищает entries и не удаляет импортированные `.ico`.
- Status area и список ошибок без `MessageBox` как основного механизма.

## Реализовано в UX cleanup phase 1

- `DataGrid` заменён на `ItemsControl` с карточками entries.
- Технические labels `DisplayName`, `DefaultFolderName`, `EntryId` убраны из основного UI.
- Карточка entry показывает preview, `Название в меню`, `Имя создаваемой папки`, `Показывать в меню`, icon status и действия `Выбрать .ico`/`Удалить`.
- После grouping MVP карточка также показывает поле `Группа` с подсказкой, что пустое значение оставляет entry в root menu.
- После WPF grouping container cleanup карточки показываются внутри visual group containers `Без группы` или `<GroupName>`. Постоянное поле `Группа` убрано из основной карточки; group rename выполняется в заголовке контейнера.
- Visual reference для grouping использован только как грубая структура `группа -> элементы`, без копирования цветов, размеров, шрифтов или геометрии.
- Пустой список entries показывает empty state и кнопку `+ Добавить пункт`.
- `Интеграция с Проводником` содержит только normal controls: dry-run/register/unregister.
- Reset вынесен в отдельную `Опасная зона` с подтверждением.
- Technical plan details показываются через `Expander` только при наличии details.
- Status/errors area отделена от technical details.

## Не реализовано

- Preview file generation/cache в `%AppData%\Foldora\previews`.
- Full localization cleanup всех status/error messages.
- Full tree grouping UI.
- Drag-and-drop.
- Orphan icon cleanup.
- Explorer restart и icon cache reset.
- User-facing diagnostics для MenuHost failures из Explorer context menu.

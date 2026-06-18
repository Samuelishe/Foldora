# Roadmap

## MVP 0.1

1. Применить иконку к выбранной папке.
2. Убрать иконку с выбранной папки.
3. Создать новую папку с выбранной иконкой.
4. Управлять пользовательскими menu entries: `.ico`, подпись, имя создаваемой папки, enabled state.
5. Зарегистрировать legacy context menu в HKCU под Foldora-owned roots.
6. Удалить legacy context menu без удаления entries/settings.
7. Сбросить пользовательское меню к пустому дефолту.
8. Открыть простое WPF-окно настроек.
9. Реализовать staged-save WPF-редактор пользовательского меню.
10. Хранить настройки в AppData.

## WPF MVP

- Поле title top-level menu, default `Создать папку`.
- Список пользовательских entries.
- Добавление и удаление entries.
- Поля `DisplayName` и `DefaultFolderName`.
- Выбор `.ico`.
- Preview иконки примерно 50x50.
- Checkbox `IsEnabled`.
- Кнопки `Сохранить`, `Отменить изменения`, `Включить меню Проводника`, `Отключить меню Проводника`, `Сбросить меню`.
- Inline validation/status area вместо опоры на `MessageBox` как основной механизм ошибок.
- Registry rebuild только после явного save/enable action.

## Не делать в MVP

- Полноценную Windows 11 modern context menu integration.
- COM shell extension.
- Фоновый сервис.
- Автозапуск.
- Marketplace паков.
- Синхронизацию.
- Сложный редактор иконок.
- Preview generation файлов, если WPF может показать `.ico` напрямую.
- PNG conversion.
- Патчинг системных DLL.
- Глобальную замену стандартной иконки всех папок Windows.
- Зависимость от сторонних shell-кастомайзеров.
- Explorer restart и icon cache reset по умолчанию.

## Будущее

Modern context menu, installer/MSIX, pack import/export, preview generation, nested menu runtime/storage и расширенная валидация паков рассматриваются после рабочего MVP.

Текущая ручная разработка может указывать registry menu на Debug CLI path:

```text
src\Foldora.Cli\bin\Debug\net10.0\Foldora.Cli.exe
```

Будущий installer/publish должен дать стабильные installed paths:

```text
Foldora.App.exe
Foldora.Cli.exe
```

Production registry menu должен указывать на стабильный installed `Foldora.Cli.exe`, а не на Debug build output.

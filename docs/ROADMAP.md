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

- Custom title bar/window shell без видимой стандартной Windows title bar.
- Settings UI с выбором языка.
- Поле title top-level menu, default `Создать папку`.
- Список пользовательских entries.
- Добавление и удаление entries.
- Поля `DisplayName` и `DefaultFolderName`.
- Выбор `.ico`.
- Preview иконки примерно 50x50.
- Checkbox `IsEnabled`.
- Кнопки `Сохранить`, `Отменить изменения`, `Включить меню Проводника`, `Отключить меню Проводника`, `Сбросить меню`.
- Inline validation/status area вместо опоры на `MessageBox` как основной механизм ошибок.
- Registry operations только после явного user action: integration buttons или `Сохранить`, когда integration уже включена.

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

Near future:

- One-level grouping через `FolderMenuEntry.GroupName` перед full tree migration:

```text
Создать папку
  Цветные
    Синяя
    Красная
  Готические
    Череп
  Музыка
```

Отдельные post-MVP investigation tracks:

- Исследовать создание desktop icon под курсором только через advanced shell integration layer (`IExplorerCommand`, COM shell extension, Explorer view positioning или другой явный path). MVP legacy registry menu получает только target directory path и не решает позиционирование.

Текущая ручная разработка должна указывать registry menu на Debug MenuHost path:

```text
src\Foldora.MenuHost\bin\Debug\net10.0-windows\Foldora.MenuHost.exe
```

Будущий installer/publish должен дать стабильные installed paths:

```text
Foldora.App.exe
Foldora.Cli.exe
Foldora.MenuHost.exe
```

Production registry menu должен указывать на стабильный installed `Foldora.MenuHost.exe`, а не на Debug build output и не на console `Foldora.Cli.exe`.

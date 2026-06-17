# Desktop.ini

MVP применяет иконку папки через `desktop.ini` внутри целевой папки.

Формат:

```ini
[.ShellClassInfo]
IconResource=<absolute-path-to-icon>,0
```

Алгоритм:

1. Создать или обновить `desktop.ini`.
2. Записать секцию `[.ShellClassInfo]`.
3. Записать `IconResource=<absolute-path-to-icon>,0`.
4. Поставить атрибуты `desktop.ini`: Hidden + System.
5. Поставить атрибут System целевой папке.

В MVP не делать агрессивный reset icon cache и не перезапускать Explorer по умолчанию.

Windows лучше работает с `.ico`, а не `.png`. `.ico` должен содержать размеры 16, 32, 48, 64, 128, 256 px. Нельзя переименовывать PNG в ICO. Explorer может кэшировать иконки, поэтому визуальное обновление не всегда мгновенное.

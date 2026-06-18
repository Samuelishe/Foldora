# Future Ideas

- Modern Windows 11 context menu через отдельный исследовательский этап.
- Import/export icon packs.
- Full tree menu runtime/storage beyond current one-level `GroupName`:

```text
Создать папку
  Цветные
    Красная
    Синяя
  Готические
    Череп
    Скелет
  Фото
```

- Ограничения будущей nested model: depth after root = 2, max children per group = 30, max total nodes = 100, max enabled create entries = 50.
- Preview grid для стилей.
- Preview generation в `%AppData%\Foldora\previews\`, если прямой WPF preview из `.ico` будет недостаточен.
- Валидация `.ico` размеров.
- Orphan icon cleanup для импортированных `.ico`, отдельная команда/UX после `menu reset --yes`.
- Полная локализация всех status/error/detail strings и runtime language switching без перезапуска.
- Low-priority optional repair command, например `foldora repair-folder --folder "<folder>"` или `foldora normalize-attributes --folder "<folder>"`, только если появится реальная потребность чинить старые Foldora-created папки с прежними `System` attributes. Это не planned MVP task.
- Создание папки под cursor position на рабочем столе через отдельное shell integration исследование. Legacy registry menu даёт только target directory path, не desktop icon-view coordinates.
- `unregister-menu --dry-run`.
- Installer/MSIX/publish flow со стабильным installed path для `Foldora.App.exe`, `Foldora.Cli.exe` и `Foldora.MenuHost.exe`.
- Автоматическая миграция настроек при смене рабочего имени продукта.

using Foldora.App.Services;
using Foldora.Core.Settings;

namespace Foldora.Tests.App;

public sealed class LocalizationServiceTests
{
    [Fact]
    public void EnabledCatalogsHaveSameKeysAsEnglish()
    {
        var english = InMemoryLocalizationService.LoadCatalog("en");
        var expectedKeys = english.Keys.Order(StringComparer.Ordinal).ToArray();

        foreach (var locale in FoldoraLanguage.SupportedLocales)
        {
            var catalog = InMemoryLocalizationService.LoadCatalog(locale);
            Assert.Equal(expectedKeys, catalog.Keys.Order(StringComparer.Ordinal).ToArray());
        }
    }

    [Fact]
    public void EnabledCatalogsDoNotHaveEmptyValues()
    {
        foreach (var locale in FoldoraLanguage.SupportedLocales)
        {
            var catalog = InMemoryLocalizationService.LoadCatalog(locale);
            Assert.DoesNotContain(catalog, pair => string.IsNullOrWhiteSpace(pair.Value));
        }
    }

    [Fact]
    public void CatalogContainsKnownDefaultAndStatusKeys()
    {
        var english = new InMemoryLocalizationService("en");

        Assert.Equal("View", english.Resources.DefaultEntryDisplayNamePrefix);
        Assert.Equal("New folder", english.Resources.DefaultFolderName);
        Assert.Equal("Create folder", english.Resources.CreateFolderMenuTitle);
        Assert.Equal("none", english.Resources.IconNone);
        Assert.Equal("All changes saved", english.Resources.AllChangesSaved);
        Assert.Equal("Unsaved changes", english.Resources.UnsavedChanges);
        Assert.Equal("Settings loaded.", english.Resources.SettingsLoaded);
        Assert.Equal("Draft entry added. Choose an icon or image before saving.", english.Resources.DraftEntryAddedChooseIcon);
        Assert.Equal("Application", english.Resources.ApplicationSection);
        Assert.Equal("Explorer menu", english.Resources.ExplorerMenuSection);
        Assert.Equal("Installation", english.Resources.InstallationSection);
        Assert.Equal("Manage in Settings", english.Resources.ManageInSettings);
        Assert.Equal("Preview changes", english.Resources.PreviewChanges);
        Assert.Equal("Enable", english.Resources.RegisterExplorer);
        Assert.Equal("Disable", english.Resources.UnregisterExplorer);
        Assert.Equal("Foldora Explorer menu: On", english.Resources.ExplorerMenuStatusOn);
        Assert.Equal("Foldora Explorer menu: Off", english.Resources.ExplorerMenuStatusOff);
        Assert.Equal("Application", english.Resources.SettingsTabApplication);
        Assert.Equal("Explorer menu", english.Resources.SettingsTabExplorerMenu);
        Assert.Equal("Installation", english.Resources.SettingsTabInstallation);
        Assert.Equal("Help", english.Resources.SettingsTabHelpAbout);
        Assert.Equal("Danger zone", english.Resources.SettingsTabDangerZone);
        Assert.Equal("Open", english.Resources.OpenFolder);
        Assert.Equal("Open", english.Resources.OpenLocation);
        Assert.Equal("Copy", english.Resources.CopyPath);
        Assert.Equal("Opens this folder.", english.Resources.OpenFolderTooltip);
        Assert.Equal("Opens the folder that contains this file.", english.Resources.OpenLocationTooltip);
        Assert.Equal("Copies this path.", english.Resources.CopyPathTooltip);
        Assert.Equal("Help / About", english.Resources.HelpAboutSection);
        Assert.Equal("Help and About", english.Resources.OpenHelpAbout);
        Assert.Equal("Help / About Foldora", english.Resources.HelpWindowTitle);
        Assert.Equal("What Foldora does", english.Resources.HelpWhatFoldoraDoesTitle);
        Assert.Equal("How to use", english.Resources.HelpHowToUseTitle);
        Assert.Equal("Version: {0}", english.Resources.HelpVersionFormat);
        Assert.Equal("Save or discard menu changes before changing Explorer integration.", english.Resources.SaveOrDiscardBeforeExplorer);
        Assert.Equal("Created folder name contains invalid character \"{character}\".", english.Resources["Validation.folder_name_invalid_chars"]);
        Assert.Equal("Choose an .ico for the menu entry before saving.", english.Resources["Validation.entry_icon_path_empty"]);
        Assert.Equal("Choose icon/image", english.Resources.ChooseIcon);
        Assert.Equal("Icon/image files (*.ico;*.png;*.jpg;*.jpeg;*.bmp)", english.Resources.IconPickerFilterIconImages);
        Assert.Equal("ICO icons (*.ico)", english.Resources.IconPickerFilterIco);
        Assert.Equal("Images (*.png;*.jpg;*.jpeg;*.bmp)", english.Resources.IconPickerFilterImages);
        Assert.Equal("All files (*.*)", english.Resources.IconPickerFilterAllFiles);
        Assert.Equal("Could not convert the selected image to an ICO file.", english.Resources.IconImageConversionFailed);
        Assert.Equal("Drop an ICO, PNG, JPG, or BMP file here to use it as the folder icon.", english.Resources.IconDropTooltip);
        Assert.Equal("Only one icon file can be dropped at a time.", english.Resources.IconDropMultipleFilesRejected);
        Assert.Equal("The dropped file is not a supported icon or image format.", english.Resources.IconDropUnsupportedFile);
        Assert.Equal("Drop a file, not a folder.", english.Resources.IconDropDirectoryRejected);
        Assert.Equal("Could not use the dropped file as an icon.", english.Resources.IconDropCouldNotUseFile);
    }

    [Fact]
    public void RussianStringLookupWorks()
    {
        var service = new InMemoryLocalizationService("ru");

        Assert.Equal("Настройки", service.Resources.Settings);
        Assert.Equal("Приложение", service.Resources.SettingsTabApplication);
        Assert.Equal("Меню Проводника", service.Resources.SettingsTabExplorerMenu);
        Assert.Equal("Установка", service.Resources.SettingsTabInstallation);
        Assert.Equal("Справка", service.Resources.SettingsTabHelpAbout);
        Assert.Equal("Опасная зона", service.Resources.SettingsTabDangerZone);
        Assert.Equal("Включить", service.Resources.RegisterExplorer);
        Assert.Equal("Выключить", service.Resources.UnregisterExplorer);
        Assert.Equal("Открыть", service.Resources.OpenLocation);
        Assert.Equal("Открывает папку, где находится файл.", service.Resources.OpenLocationTooltip);
        Assert.Equal("Выбрать иконку/изображение", service.Resources.ChooseIcon);
        Assert.Equal("Не удалось преобразовать выбранное изображение в файл ICO.", service.Resources.IconImageConversionFailed);
        Assert.Equal("Перетащите сюда файл ICO, PNG, JPG или BMP, чтобы использовать его как иконку папки.", service.Resources.IconDropTooltip);
        Assert.Equal("За один раз можно перетащить только один файл иконки.", service.Resources.IconDropMultipleFilesRejected);
        Assert.Equal("Перетащенный файл не является поддерживаемой иконкой или изображением.", service.Resources.IconDropUnsupportedFile);
        Assert.Equal("Перетащите файл, а не папку.", service.Resources.IconDropDirectoryRejected);
        Assert.Equal("Не удалось использовать перетащенный файл как иконку.", service.Resources.IconDropCouldNotUseFile);
    }

    [Fact]
    public void EnglishStringLookupWorks()
    {
        var service = new InMemoryLocalizationService("en");

        Assert.Equal("Settings", service.Resources.Settings);
    }

    [Fact]
    public void MissingKeyFallsBackToKey()
    {
        var service = new InMemoryLocalizationService("ru");

        Assert.Equal("Missing.Key", service.Resources["Missing.Key"]);
    }

    [Fact]
    public void UnsupportedLanguageFallsBackToEnglish()
    {
        var service = new InMemoryLocalizationService("ar");

        Assert.Equal("en", service.CurrentLanguage);
        Assert.Equal("Settings", service.Resources.Settings);
    }

    [Theory]
    [MemberData(nameof(EnabledLocaleExpectations))]
    public void EnabledLocalesExposeLocalizedDefaults(
        string locale,
        string expectedTitle,
        string expectedEntryPrefix,
        string expectedFolderName)
    {
        var service = new InMemoryLocalizationService(locale);

        Assert.Equal(locale, service.CurrentLanguage);
        Assert.Equal(expectedTitle, service.Resources.CreateFolderMenuTitle);
        Assert.Equal(expectedEntryPrefix, service.Resources.DefaultEntryDisplayNamePrefix);
        Assert.Equal(expectedFolderName, service.Resources.DefaultFolderName);
    }

    [Fact]
    public void GroupLabelsHaveRussianAndEnglishValues()
    {
        var russian = new InMemoryLocalizationService("ru");
        var english = new InMemoryLocalizationService("en");

        Assert.Equal("Группа:", russian.Resources.EntryGroupName);
        Assert.Equal("Group:", english.Resources.EntryGroupName);
        Assert.Equal("Оставьте пустым, чтобы пункт был в корне меню.", russian.Resources.EntryGroupHelp);
        Assert.Equal("Leave empty to place the entry in the root menu.", english.Resources.EntryGroupHelp);
        Assert.Equal("+ Добавить группу", russian.Resources.AddGroup);
        Assert.Equal("+ Add group", english.Resources.AddGroup);
        Assert.Equal("Без группы", russian.Resources.RootGroupTitle);
        Assert.Equal("No group", english.Resources.RootGroupTitle);
        Assert.Equal("+ Добавить пункт в эту группу", russian.Resources.AddEntryToThisGroup);
        Assert.Equal("+ Add entry to this group", english.Resources.AddEntryToThisGroup);
        Assert.Equal("Удалить группу", russian.Resources.DeleteGroup);
        Assert.Equal("Delete group", english.Resources.DeleteGroup);
        Assert.Equal("Удалить группу и все её пункты?", russian.Resources.DeleteGroupPrompt);
        Assert.Equal("Delete the group and all its entries?", english.Resources.DeleteGroupPrompt);
        Assert.Equal("Элементов: {0}", russian.Resources.EntryCountFormat);
        Assert.Equal("{0} entries", english.Resources.EntryCountFormat);
    }

    [Fact]
    public void PageHeaderLabelsHaveRussianAndEnglishValues()
    {
        var russian = new InMemoryLocalizationService("ru");
        var english = new InMemoryLocalizationService("en");

        Assert.Equal("Меню папок", russian.Resources.PageTitle);
        Assert.Equal("Создавайте и настраивайте пункты контекстного меню Проводника.", russian.Resources.PageSubtitle);
        Assert.Equal("Folder menu", english.Resources.PageTitle);
        Assert.Equal("Create and configure File Explorer context menu entries.", english.Resources.PageSubtitle);
    }

    [Fact]
    public void CompactEntryLabelsHaveRussianAndEnglishValues()
    {
        var russian = new InMemoryLocalizationService("ru");
        var english = new InMemoryLocalizationService("en");

        Assert.Equal("Редактировать", russian.Resources.Edit);
        Assert.Equal("Готово", russian.Resources.Done);
        Assert.Equal("Имя папки: ", russian.Resources.FolderNameSummaryLabel);
        Assert.Equal("Edit", english.Resources.Edit);
        Assert.Equal("Done", english.Resources.Done);
        Assert.Equal("Folder name: ", english.Resources.FolderNameSummaryLabel);
    }

    public static TheoryData<string, string, string, string> EnabledLocaleExpectations()
    {
        return new TheoryData<string, string, string, string>
        {
            { FoldoraLanguage.Russian, "Создать папку", "Вид", "Новая папка" },
            { FoldoraLanguage.English, "Create folder", "View", "New folder" },
            { FoldoraLanguage.SimplifiedChinese, "创建文件夹", "视图", "新建文件夹" },
            { FoldoraLanguage.German, "Ordner erstellen", "Ansicht", "Neuer Ordner" },
            { FoldoraLanguage.Spanish, "Crear carpeta", "Vista", "Nueva carpeta" },
            { FoldoraLanguage.French, "Créer un dossier", "Vue", "Nouveau dossier" },
            { FoldoraLanguage.Japanese, "フォルダーを作成", "ビュー", "新しいフォルダー" },
            { FoldoraLanguage.BrazilianPortuguese, "Criar pasta", "Visualização", "Nova pasta" },
            { FoldoraLanguage.Korean, "폴더 만들기", "보기", "새 폴더" },
            { FoldoraLanguage.Ukrainian, "Створити папку", "Вигляд", "Нова папка" },
            { FoldoraLanguage.Polish, "Utwórz folder", "Widok", "Nowy folder" },
            { FoldoraLanguage.Turkish, "Klasör oluştur", "Görünüm", "Yeni klasör" },
            { FoldoraLanguage.Romanian, "Creează folder", "Vizualizare", "Folder nou" },
            { FoldoraLanguage.Czech, "Vytvořit složku", "Zobrazení", "Nová složka" },
            { FoldoraLanguage.Hungarian, "Mappa létrehozása", "Nézet", "Új mappa" },
            { FoldoraLanguage.Bulgarian, "Създай папка", "Изглед", "Нова папка" },
            { FoldoraLanguage.Italian, "Crea cartella", "Vista", "Nuova cartella" },
            { FoldoraLanguage.Dutch, "Map maken", "Weergave", "Nieuwe map" },
            { FoldoraLanguage.Indonesian, "Buat folder", "Tampilan", "Folder baru" },
            { FoldoraLanguage.Vietnamese, "Tạo thư mục", "Chế độ xem", "Thư mục mới" },
            { FoldoraLanguage.Hindi, "फ़ोल्डर बनाएँ", "दृश्य", "नया फ़ोल्डर" },
            { FoldoraLanguage.Thai, "สร้างโฟลเดอร์", "มุมมอง", "โฟลเดอร์ใหม่" },
            { FoldoraLanguage.TraditionalChinese, "建立資料夾", "檢視", "新資料夾" },
            { FoldoraLanguage.PortuguesePortugal, "Criar pasta", "Vista", "Nova pasta" }
        };
    }
}

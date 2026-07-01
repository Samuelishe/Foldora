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
        Assert.Equal("Settings loaded.", english.Resources.SettingsLoaded);
        Assert.Equal("Draft entry added. Choose an .ico before saving.", english.Resources.DraftEntryAddedChooseIcon);
        Assert.Equal("Created folder name contains invalid character \"{character}\".", english.Resources["Validation.folder_name_invalid_chars"]);
        Assert.Equal("Choose an .ico for the menu entry before saving.", english.Resources["Validation.entry_icon_path_empty"]);
    }

    [Fact]
    public void RussianStringLookupWorks()
    {
        var service = new InMemoryLocalizationService("ru");

        Assert.Equal("Настройки", service.Resources.Settings);
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
        var service = new InMemoryLocalizationService("it");

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
            { FoldoraLanguage.Korean, "폴더 만들기", "보기", "새 폴더" }
        };
    }
}

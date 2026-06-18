using Foldora.App.Services;

namespace Foldora.Tests.App;

public sealed class LocalizationServiceTests
{
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
}

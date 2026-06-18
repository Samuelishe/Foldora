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
    }
}

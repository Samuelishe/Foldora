using Foldora.App.Services;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;

namespace Foldora.Tests.App;

public sealed class SettingsLanguageInitializerTests
{
    [Theory]
    [InlineData("ru", "ru")]
    [InlineData("ru-RU", "ru")]
    [InlineData("en", "en")]
    [InlineData("en-US", "en")]
    [InlineData("de-DE", "en")]
    [InlineData("ja-JP", "en")]
    [InlineData("zh-Hans", "en")]
    public void DetectStartupLanguage_SelectsOnlyCompleteLocales(string cultureName, string expected)
    {
        Assert.Equal(expected, SettingsLanguageInitializer.DetectStartupLanguage(cultureName));
    }

    [Fact]
    public async Task InitializeAsync_NoSettingsFileAndRussianSystemLanguage_PersistsRussian()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraLanguageInit-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var storage = new FoldoraSettingsStorage(paths);
            var initializer = new SettingsLanguageInitializer(storage, new FixedSystemLanguageProvider("ru-RU"));

            await initializer.InitializeAsync();

            var loaded = await storage.LoadWithLanguageMetadataAsync();
            Assert.Equal("ru", loaded.Settings.Language);
            Assert.True(loaded.LanguageWasPersisted);
            Assert.True(loaded.LanguageWasSupported);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task InitializeAsync_NoSettingsFileAndUnsupportedSystemLanguage_PersistsEnglish()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraLanguageInit-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var storage = new FoldoraSettingsStorage(paths);
            var initializer = new SettingsLanguageInitializer(storage, new FixedSystemLanguageProvider("de-DE"));

            await initializer.InitializeAsync();

            var loaded = await storage.LoadWithLanguageMetadataAsync();
            Assert.Equal("en", loaded.Settings.Language);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData("en", "ru-RU", "en")]
    [InlineData("ru", "en-US", "ru")]
    public async Task InitializeAsync_PersistedSupportedLanguageWinsOverSystemLanguage(
        string savedLanguage,
        string systemLanguage,
        string expectedLanguage)
    {
        var root = Directory.CreateTempSubdirectory("FoldoraLanguageInit-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var storage = new FoldoraSettingsStorage(paths);
            await storage.SaveAsync(new FoldoraSettings { Language = savedLanguage });
            var initializer = new SettingsLanguageInitializer(storage, new FixedSystemLanguageProvider(systemLanguage));

            await initializer.InitializeAsync();

            var loaded = await storage.LoadAsync();
            Assert.Equal(expectedLanguage, loaded.Language);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData("ru-RU", "ru", "Создать папку")]
    [InlineData("ja-JP", "en", "Create folder")]
    public async Task InitializeAsync_OldSettingsWithoutLanguage_DetectsAndPreservesMenuData(
        string systemLanguage,
        string expectedLanguage,
        string expectedTitle)
    {
        var root = Directory.CreateTempSubdirectory("FoldoraLanguageInit-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            Directory.CreateDirectory(paths.RootDirectory);
            await File.WriteAllTextAsync(
                paths.SettingsFile,
                """
                {
                  "explorerIntegrationEnabled": false,
                  "createFolderMenu": {
                    "title": "Создать папку",
                    "entries": [
                      {
                        "id": "entry-old",
                        "displayName": "Череп",
                        "defaultFolderName": "Череп",
                        "groupName": "Готические",
                        "iconPath": "C:\\Foldora\\icons\\entry-old.ico",
                        "sortOrder": 0,
                        "isEnabled": true
                      }
                    ]
                  }
                }
                """);
            var storage = new FoldoraSettingsStorage(paths);
            var initializer = new SettingsLanguageInitializer(storage, new FixedSystemLanguageProvider(systemLanguage));

            await initializer.InitializeAsync();

            var loaded = await storage.LoadAsync();
            var entry = Assert.Single(loaded.CreateFolderMenu.Entries);
            Assert.Equal(expectedLanguage, loaded.Language);
            Assert.Equal(expectedTitle, loaded.CreateFolderMenu.Title);
            Assert.Equal("Череп", entry.DisplayName);
            Assert.Equal("Череп", entry.DefaultFolderName);
            Assert.Equal("Готические", entry.GroupName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task InitializeAsync_UnsupportedPersistedLanguage_NormalizesToEnglishAndDoesNotDetectAgain()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraLanguageInit-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            Directory.CreateDirectory(paths.RootDirectory);
            await File.WriteAllTextAsync(
                paths.SettingsFile,
                """
                {
                  "language": "de",
                  "explorerIntegrationEnabled": false,
                  "createFolderMenu": {
                    "title": "Создать папку",
                    "entries": []
                  }
                }
                """);
            var storage = new FoldoraSettingsStorage(paths);
            var initializer = new SettingsLanguageInitializer(storage, new FixedSystemLanguageProvider("ru-RU"));

            await initializer.InitializeAsync();

            var loaded = await storage.LoadAsync();
            Assert.Equal("en", loaded.Language);
            Assert.Equal("Create folder", loaded.CreateFolderMenu.Title);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private sealed class FixedSystemLanguageProvider : ISystemLanguageProvider
    {
        public FixedSystemLanguageProvider(string currentUiCultureName)
        {
            CurrentUiCultureName = currentUiCultureName;
        }

        public string CurrentUiCultureName { get; }
    }
}

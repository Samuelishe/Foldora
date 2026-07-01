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
    [InlineData("zh", "zh-Hans")]
    [InlineData("zh-CN", "zh-Hans")]
    [InlineData("zh-SG", "zh-Hans")]
    [InlineData("zh-Hans", "zh-Hans")]
    [InlineData("zh-Hans-CN", "zh-Hans")]
    [InlineData("zh-TW", "en")]
    [InlineData("de", "de")]
    [InlineData("de-DE", "de")]
    [InlineData("es", "es")]
    [InlineData("es-MX", "es")]
    [InlineData("fr", "fr")]
    [InlineData("fr-FR", "fr")]
    [InlineData("ja", "ja")]
    [InlineData("ja-JP", "ja")]
    [InlineData("pt", "pt-BR")]
    [InlineData("pt-BR", "pt-BR")]
    [InlineData("pt-PT", "pt-BR")]
    [InlineData("ko", "ko")]
    [InlineData("ko-KR", "ko")]
    [InlineData("it-IT", "en")]
    public void DetectStartupLanguage_SelectsEnabledCompleteLocales(string cultureName, string expected)
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
            var initializer = new SettingsLanguageInitializer(storage, new FixedSystemLanguageProvider("it-IT"));

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
    [InlineData("de", "ru-RU", "de")]
    [InlineData("zh-Hans", "en-US", "zh-Hans")]
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
    [InlineData("ja-JP", "ja", "フォルダーを作成")]
    [InlineData("it-IT", "en", "Create folder")]
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
                  "language": "it",
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

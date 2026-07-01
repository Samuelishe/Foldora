using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;

namespace Foldora.Tests.Settings;

public sealed class FoldoraSettingsStorageTests
{
    [Fact]
    public async Task EnsureCreatedAsync_CreatesAppDataLayoutAndDefaultSettings()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var storage = new FoldoraSettingsStorage(paths);

            await storage.EnsureCreatedAsync();

            Assert.True(Directory.Exists(paths.RootDirectory));
            Assert.True(Directory.Exists(paths.IconsDirectory));
            Assert.True(Directory.Exists(paths.PreviewsDirectory));
            Assert.True(Directory.Exists(paths.PacksDirectory));
            Assert.True(File.Exists(paths.SettingsFile));

            var settings = await storage.LoadAsync();

            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.Equal("ru", settings.Language);
            Assert.Equal("Создать папку", settings.CreateFolderMenu.Title);
            Assert.False(settings.CreateFolderMenu.TitleIsCustom);
            Assert.Empty(settings.CreateFolderMenu.Entries);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_UsesRussianLanguageForOldSettingsWithoutLanguage()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

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
                    "entries": []
                  }
                }
                """);

            var loaded = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.Equal("ru", loaded.Language);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAndLoadAsync_PreservesLanguage()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var storage = new FoldoraSettingsStorage(paths);

            await storage.SaveAsync(new FoldoraSettings { Language = "en" });

            var loaded = await storage.LoadAsync();

            Assert.Equal("en", loaded.Language);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_InfersOldRussianDefaultTitleAsNonCustom()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            Directory.CreateDirectory(paths.RootDirectory);
            await File.WriteAllTextAsync(
                paths.SettingsFile,
                """
                {
                  "language": "en",
                  "explorerIntegrationEnabled": false,
                  "createFolderMenu": {
                    "title": "Создать папку",
                    "entries": []
                  }
                }
                """);

            var loaded = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.Equal("en", loaded.Language);
            Assert.Equal("Create folder", loaded.CreateFolderMenu.Title);
            Assert.False(loaded.CreateFolderMenu.TitleIsCustom);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_InfersOldCustomTitleAsCustom()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            Directory.CreateDirectory(paths.RootDirectory);
            await File.WriteAllTextAsync(
                paths.SettingsFile,
                """
                {
                  "language": "en",
                  "explorerIntegrationEnabled": false,
                  "createFolderMenu": {
                    "title": "Мои папки",
                    "entries": []
                  }
                }
                """);

            var loaded = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.Equal("Мои папки", loaded.CreateFolderMenu.Title);
            Assert.True(loaded.CreateFolderMenu.TitleIsCustom);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_NormalizesUnsupportedLanguageToEnglish()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

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

            var loaded = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.Equal("en", loaded.Language);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadWithLanguageMetadataAsync_ReportsMissingLanguage()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

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
                    "entries": []
                  }
                }
                """);

            var loaded = await new FoldoraSettingsStorage(paths).LoadWithLanguageMetadataAsync(createSettingsIfMissing: false);

            Assert.False(loaded.LanguageWasPersisted);
            Assert.False(loaded.LanguageWasSupported);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadWithLanguageMetadataAsync_ReportsUnsupportedLanguage()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

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

            var loaded = await new FoldoraSettingsStorage(paths).LoadWithLanguageMetadataAsync(createSettingsIfMissing: false);

            Assert.True(loaded.LanguageWasPersisted);
            Assert.False(loaded.LanguageWasSupported);
            Assert.Equal("en", loaded.Settings.Language);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAndLoadAsync_PreservesEntryWithCyrillicDisplayName()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var storage = new FoldoraSettingsStorage(paths);
            var settings = new FoldoraSettings();
            settings.CreateFolderMenu.Entries.Add(new FolderMenuEntry
            {
                Id = "entry-stable-id",
                DisplayName = "Череп",
                DefaultFolderName = "Новая папка",
                IconPath = Path.Combine(paths.IconsDirectory, "entry-stable-id.ico"),
                SortOrder = 0
            });

            await storage.SaveAsync(settings);

            var loaded = await storage.LoadAsync();
            var entry = Assert.Single(loaded.CreateFolderMenu.Entries);

            Assert.Equal("entry-stable-id", entry.Id);
            Assert.Equal("Череп", entry.DisplayName);
            Assert.Equal("Новая папка", entry.DefaultFolderName);
            Assert.Equal(string.Empty, entry.GroupName);
            Assert.NotEqual(entry.DisplayName, entry.Id);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_UsesDefaultFolderNameForOldEntries()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

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
                        "iconPath": "C:\\Foldora\\icons\\entry-old.ico",
                        "sortOrder": 0,
                        "isEnabled": true
                      }
                    ]
                  }
                }
                """);

            var loaded = await new FoldoraSettingsStorage(paths).LoadAsync();
            var entry = Assert.Single(loaded.CreateFolderMenu.Entries);

            Assert.Equal("Новая папка", entry.DefaultFolderName);
            Assert.Equal(string.Empty, entry.GroupName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAndLoadAsync_PreservesGroupName()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var storage = new FoldoraSettingsStorage(paths);
            var settings = new FoldoraSettings();
            settings.CreateFolderMenu.Entries.Add(new FolderMenuEntry
            {
                Id = "entry-blue",
                DisplayName = "Синяя",
                DefaultFolderName = "Синяя",
                GroupName = "Цветные",
                IconPath = Path.Combine(paths.IconsDirectory, "entry-blue.ico"),
                SortOrder = 0
            });

            await storage.SaveAsync(settings);

            var loaded = await storage.LoadAsync();
            var entry = Assert.Single(loaded.CreateFolderMenu.Entries);

            Assert.Equal("Цветные", entry.GroupName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_NormalizesWhitespaceGroupNameToRoot()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettings-");

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
                        "groupName": "   ",
                        "iconPath": "C:\\Foldora\\icons\\entry-old.ico",
                        "sortOrder": 0,
                        "isEnabled": true
                      }
                    ]
                  }
                }
                """);

            var loaded = await new FoldoraSettingsStorage(paths).LoadAsync();
            var entry = Assert.Single(loaded.CreateFolderMenu.Entries);

            Assert.Equal(string.Empty, entry.GroupName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}

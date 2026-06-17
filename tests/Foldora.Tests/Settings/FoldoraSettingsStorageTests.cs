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
            Assert.Equal("Создать папку", settings.CreateFolderMenu.Title);
            Assert.Empty(settings.CreateFolderMenu.Entries);
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
                IconPath = Path.Combine(paths.IconsDirectory, "entry-stable-id.ico"),
                SortOrder = 0
            });

            await storage.SaveAsync(settings);

            var loaded = await storage.LoadAsync();
            var entry = Assert.Single(loaded.CreateFolderMenu.Entries);

            Assert.Equal("entry-stable-id", entry.Id);
            Assert.Equal("Череп", entry.DisplayName);
            Assert.NotEqual(entry.DisplayName, entry.Id);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}

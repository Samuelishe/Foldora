using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;

namespace Foldora.Tests.Menu;

public sealed class FolderMenuServiceTests
{
    [Fact]
    public async Task AddAsync_UsesFallbackNameWhenDisplayNameIsEmpty()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMenu-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var service = CreateService(paths);
            var sourceIconPath = await CreateSourceIconAsync(root.FullName, "icon.ico");

            var entry = await service.AddAsync(sourceIconPath, "");

            Assert.Equal("Вид 1", entry.DisplayName);
            Assert.NotEqual(entry.DisplayName, entry.Id);
            Assert.True(File.Exists(entry.IconPath));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task AddAsync_SavesCustomNameAndRemoveAsyncDeletesOwnedIcon()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMenu-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var service = CreateService(paths);
            var sourceIconPath = await CreateSourceIconAsync(root.FullName, "skull.ico");

            var entry = await service.AddAsync(sourceIconPath, "папка череп");

            Assert.Equal("папка череп", entry.DisplayName);
            Assert.True(File.Exists(entry.IconPath));

            var removed = await service.RemoveAsync(entry.Id);
            var entries = await service.ListAsync();

            Assert.Equal(entry.Id, removed.Id);
            Assert.Empty(entries);
            Assert.False(File.Exists(entry.IconPath));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RemoveAsync_ReportsMissingEntry()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMenu-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var service = CreateService(paths);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RemoveAsync("entry-missing"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private static FolderMenuService CreateService(FoldoraDataPaths paths)
    {
        return new FolderMenuService(new FoldoraSettingsStorage(paths), paths);
    }

    private static async Task<string> CreateSourceIconAsync(string root, string fileName)
    {
        var path = Path.Combine(root, "Source Icons", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, "ico placeholder");
        return path;
    }
}

using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Core.Validation;
using Foldora.Tests.Fixtures;

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
            Assert.Equal(FolderNameValidator.DefaultFolderName, entry.DefaultFolderName);
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

            var entry = await service.AddAsync(sourceIconPath, "папка череп", "Новая папка");

            Assert.Equal("папка череп", entry.DisplayName);
            Assert.Equal("Новая папка", entry.DefaultFolderName);
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
    public async Task AddAsync_RejectsInvalidFolderNameBeforeImport()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMenu-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var service = CreateService(paths);
            var sourceIconPath = await CreateSourceIconAsync(root.FullName, "skull.ico");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AddAsync(sourceIconPath, "Череп", "Bad<Name"));

            Assert.Contains("Default folder name", exception.Message);
            Assert.Empty(Directory.EnumerateFiles(paths.IconsDirectory));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task AddAsync_UsesDefaultFolderNameWhenFolderNameIsEmpty()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMenu-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var service = CreateService(paths);
            var sourceIconPath = await CreateSourceIconAsync(root.FullName, "skull.ico");

            var entry = await service.AddAsync(sourceIconPath, "Череп", "");

            Assert.Equal(FolderNameValidator.DefaultFolderName, entry.DefaultFolderName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task AddAsync_AllowsDuplicateDisplayNames()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMenu-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var service = CreateService(paths);
            var firstIconPath = await CreateSourceIconAsync(root.FullName, "skull1.ico");
            var secondIconPath = await CreateSourceIconAsync(root.FullName, "skull2.ico");

            await service.AddAsync(firstIconPath, "Череп", "Новая папка");
            await service.AddAsync(secondIconPath, "Череп", "Другая папка");

            var entries = await service.ListAsync();
            Assert.Equal(2, entries.Count(entry => entry.DisplayName == "Череп"));
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
        await IcoTestFile.WriteValidAsync(path);
        return path;
    }
}

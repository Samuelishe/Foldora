using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Core.Validation;
using Foldora.Tests.Fixtures;

namespace Foldora.Tests.Menu;

public sealed class FolderMenuEntryActionServiceTests
{
    [Fact]
    public async Task ApplyAsync_AppliesImportedIconFromEntry()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var iconPath = await CreateIconAsync(paths, "entry-apply");
            await SaveSettingsAsync(paths, CreateEntry("entry-apply", iconPath, "Череп"));
            var targetFolder = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));

            await CreateService(paths).ApplyAsync(targetFolder.FullName, "entry-apply");

            var desktopIniPath = Path.Combine(targetFolder.FullName, "desktop.ini");
            var content = await File.ReadAllTextAsync(desktopIniPath);
            Assert.Contains($"IconResource={Path.GetFullPath(iconPath)},0", content);
            Assert.True(targetFolder.Attributes.HasFlag(FileAttributes.ReadOnly));
            Assert.False(targetFolder.Attributes.HasFlag(FileAttributes.System));
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ApplyAsync_ThrowsForUnknownEntry()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await new FoldoraSettingsStorage(paths).EnsureCreatedAsync();
            var targetFolder = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService(paths).ApplyAsync(targetFolder.FullName, "entry-missing"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ApplyAsync_ThrowsForDisabledEntry()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var iconPath = await CreateIconAsync(paths, "entry-disabled");
            var entry = CreateEntry("entry-disabled", iconPath, "Череп");
            entry.IsEnabled = false;
            await SaveSettingsAsync(paths, entry);
            var targetFolder = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService(paths).ApplyAsync(targetFolder.FullName, "entry-disabled"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ApplyAsync_ThrowsForMissingIconFile()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, CreateEntry("entry-missing-icon", Path.Combine(paths.IconsDirectory, "missing.ico"), "Череп"));
            var targetFolder = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));

            await Assert.ThrowsAsync<FileNotFoundException>(
                () => CreateService(paths).ApplyAsync(targetFolder.FullName, "entry-missing-icon"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task CreateAsync_CreatesFolderWithDefaultFolderNameAndIcon()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var iconPath = await CreateIconAsync(paths, "entry-create");
            await SaveSettingsAsync(paths, CreateEntry("entry-create", iconPath, "Череп"));
            var target = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));

            var createdPath = await CreateService(paths).CreateAsync(target.FullName, "entry-create");

            Assert.Equal(Path.Combine(target.FullName, "Череп"), createdPath);
            Assert.True(Directory.Exists(createdPath));
            var content = await File.ReadAllTextAsync(Path.Combine(createdPath, "desktop.ini"));
            Assert.Contains($"IconResource={Path.GetFullPath(iconPath)},0", content);
            Assert.True(new DirectoryInfo(createdPath).Attributes.HasFlag(FileAttributes.ReadOnly));
            Assert.False(new DirectoryInfo(createdPath).Attributes.HasFlag(FileAttributes.System));
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task CreateAsync_UsesFallbackFolderNameWhenEntryFolderNameIsEmpty()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var iconPath = await CreateIconAsync(paths, "entry-fallback");
            await SaveSettingsAsync(paths, CreateEntry("entry-fallback", iconPath, ""));
            var target = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));

            var createdPath = await CreateService(paths).CreateAsync(target.FullName, "entry-fallback");

            Assert.Equal(Path.Combine(target.FullName, FolderNameValidator.DefaultFolderName), createdPath);
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task CreateAsync_CreatesSecondNameWhenDirectoryExists()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var iconPath = await CreateIconAsync(paths, "entry-second");
            await SaveSettingsAsync(paths, CreateEntry("entry-second", iconPath, "Череп"));
            var target = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));
            Directory.CreateDirectory(Path.Combine(target.FullName, "Череп"));

            var createdPath = await CreateService(paths).CreateAsync(target.FullName, "entry-second");

            Assert.Equal(Path.Combine(target.FullName, "Череп (2)"), createdPath);
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task CreateAsync_ConsidersFileNameConflict()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var iconPath = await CreateIconAsync(paths, "entry-file-conflict");
            await SaveSettingsAsync(paths, CreateEntry("entry-file-conflict", iconPath, "Череп"));
            var target = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));
            await File.WriteAllTextAsync(Path.Combine(target.FullName, "Череп"), "file conflict");

            var createdPath = await CreateService(paths).CreateAsync(target.FullName, "entry-file-conflict");

            Assert.Equal(Path.Combine(target.FullName, "Череп (2)"), createdPath);
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task CreateAsync_ThrowsForMissingTargetDirectory()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var iconPath = await CreateIconAsync(paths, "entry-missing-target");
            await SaveSettingsAsync(paths, CreateEntry("entry-missing-target", iconPath, "Череп"));

            await Assert.ThrowsAsync<DirectoryNotFoundException>(
                () => CreateService(paths).CreateAsync(Path.Combine(root.FullName, "Missing"), "entry-missing-target"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task CreateAsync_ThrowsForUnknownEntry()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await new FoldoraSettingsStorage(paths).EnsureCreatedAsync();
            var target = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService(paths).CreateAsync(target.FullName, "entry-missing"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }


    [Fact]
    public async Task CreateAsync_ThrowsForDisabledEntry()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var iconPath = await CreateIconAsync(paths, "entry-disabled-create");
            var entry = CreateEntry("entry-disabled-create", iconPath, "Череп");
            entry.IsEnabled = false;
            await SaveSettingsAsync(paths, entry);
            var target = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService(paths).CreateAsync(target.FullName, "entry-disabled-create"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task CreateAsync_ThrowsForMissingIconFile()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAction-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, CreateEntry("entry-missing-icon-create", Path.Combine(paths.IconsDirectory, "missing.ico"), "Череп"));
            var target = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));

            await Assert.ThrowsAsync<FileNotFoundException>(
                () => CreateService(paths).CreateAsync(target.FullName, "entry-missing-icon-create"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private static FolderMenuEntryActionService CreateService(FoldoraDataPaths paths)
    {
        return new FolderMenuEntryActionService(new FoldoraSettingsStorage(paths));
    }

    private static async Task<string> CreateIconAsync(FoldoraDataPaths paths, string entryId)
    {
        var iconPath = Path.Combine(paths.IconsDirectory, $"{entryId}.ico");
        await IcoTestFile.WriteValidAsync(iconPath);
        return iconPath;
    }

    private static FolderMenuEntry CreateEntry(string id, string iconPath, string defaultFolderName)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = "Череп",
            DefaultFolderName = defaultFolderName,
            IconPath = iconPath,
            IsEnabled = true
        };
    }

    private static async Task SaveSettingsAsync(FoldoraDataPaths paths, params FolderMenuEntry[] entries)
    {
        var settings = new FoldoraSettings();
        foreach (var entry in entries)
        {
            settings.CreateFolderMenu.Entries.Add(entry);
        }

        await new FoldoraSettingsStorage(paths).SaveAsync(settings);
    }

    private static void ClearAttributes(DirectoryInfo directory)
    {
        foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            file.Attributes = FileAttributes.Normal;
        }

        foreach (var childDirectory in directory.EnumerateDirectories("*", SearchOption.AllDirectories))
        {
            childDirectory.Attributes = FileAttributes.Normal;
        }

        directory.Attributes = FileAttributes.Normal;
    }
}

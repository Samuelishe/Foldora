using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Tests.Fixtures;

namespace Foldora.Tests.Menu;

public sealed class FolderMenuDraftEditorTests
{
    [Fact]
    public async Task LoadAsync_CreatesDefaultSettingsWhenMissing()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var editor = CreateEditor(root.FullName);

            await editor.LoadAsync();

            Assert.Equal("Создать папку", editor.Title);
            Assert.Empty(editor.Entries);
            Assert.False(editor.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_ExposesExistingEntries()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, true, "Мои папки", CreateEntry("entry-skull", "Череп", "Череп", true));
            var editor = CreateEditor(paths);

            await editor.LoadAsync();

            Assert.Equal("Мои папки", editor.Title);
            var entry = Assert.Single(editor.Entries);
            Assert.Equal("entry-skull", entry.Id);
            Assert.Equal("Череп", entry.DisplayName);
            Assert.Equal("Череп", entry.DefaultFolderName);
            Assert.Equal(string.Empty, entry.GroupName);
            Assert.True(entry.IsEnabled);
            Assert.True(editor.ExplorerIntegrationEnabled);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task EditingDraftEntry_DoesNotImmediatelyWriteSettings()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку", CreateEntry("entry-skull", "Череп", "Череп", true));
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            editor.Entries[0].DisplayName = "Скелет";
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.Equal("Череп", saved.CreateFolderMenu.Entries[0].DisplayName);
            Assert.True(editor.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_WritesChangedDisplayNameDefaultFolderNameGroupNameAndIsEnabled()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, true, "Создать папку", CreateEntry("entry-skull", "Череп", "Череп", true));
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            editor.Title = "Мои папки";
            editor.Entries[0].DisplayName = "Скелет";
            editor.Entries[0].DefaultFolderName = "Скелет";
            editor.Entries[0].GroupName = "Готические";
            editor.Entries[0].IsEnabled = false;
            var result = await editor.SaveAsync();
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Saved);
            Assert.Equal("Мои папки", saved.CreateFolderMenu.Title);
            Assert.Equal("Скелет", saved.CreateFolderMenu.Entries[0].DisplayName);
            Assert.Equal("Скелет", saved.CreateFolderMenu.Entries[0].DefaultFolderName);
            Assert.Equal("Готические", saved.CreateFolderMenu.Entries[0].GroupName);
            Assert.False(saved.CreateFolderMenu.Entries[0].IsEnabled);
            Assert.True(saved.ExplorerIntegrationEnabled);
            Assert.False(editor.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Reload_DiscardsUnsavedChanges()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку", CreateEntry("entry-skull", "Череп", "Череп", true));
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            editor.Title = "Draft";
            editor.Entries[0].DisplayName = "Draft";
            editor.Entries[0].GroupName = "Draft";
            editor.Reload();

            Assert.Equal("Создать папку", editor.Title);
            Assert.Equal("Череп", editor.Entries[0].DisplayName);
            Assert.Equal(string.Empty, editor.Entries[0].GroupName);
            Assert.False(editor.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_BlocksInvalidDefaultFolderName()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку", CreateEntry("entry-skull", "Череп", "Череп", true));
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            editor.Entries[0].DefaultFolderName = "bad:name";
            var result = await editor.SaveAsync();
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.False(result.Saved);
            Assert.Contains(result.Issues, issue => issue.Code == "folder_name_invalid_chars");
            Assert.Equal("Череп", saved.CreateFolderMenu.Entries[0].DefaultFolderName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_BlocksTooLongDisplayName()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку", CreateEntry("entry-skull", "Череп", "Череп", true));
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            editor.Entries[0].DisplayName = new string('А', 81);
            var result = await editor.SaveAsync();

            Assert.False(result.Saved);
            Assert.Contains(result.Issues, issue => issue.Code == "display_name_too_long");
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_BlocksInvalidGroupName()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку", CreateEntry("entry-skull", "Череп", "Череп", true));
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            editor.Entries[0].GroupName = "Готические/Черепа";
            var result = await editor.SaveAsync();
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.False(result.Saved);
            Assert.Contains(result.Issues, issue => issue.Code == "group_name_nested_not_supported");
            Assert.Equal(string.Empty, saved.CreateFolderMenu.Entries[0].GroupName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_AllowsDuplicateDisplayNames()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(
                paths,
                false,
                "Создать папку",
                CreateEntry("entry-skull-1", "Череп", "Череп", true),
                CreateEntry("entry-skull-2", "Скелет", "Скелет", true));
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            editor.Entries[1].DisplayName = "Череп";
            var result = await editor.SaveAsync();
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Saved);
            Assert.Equal(["Череп", "Череп"], saved.CreateFolderMenu.Entries.Select(entry => entry.DisplayName).ToArray());
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_AllowsEmptyEntriesList()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var editor = CreateEditor(root.FullName);
            await editor.LoadAsync();

            editor.Title = "Пустое меню";
            var result = await editor.SaveAsync();

            Assert.True(result.Saved);
            Assert.Empty(result.Issues);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task AddEntry_CreatesDraftEntryOnly()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку");
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            var entry = editor.AddEntry();
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.StartsWith("entry-", entry.Id, StringComparison.Ordinal);
            Assert.Equal("Вид 1", entry.DisplayName);
            Assert.Equal("Новая папка", entry.DefaultFolderName);
            Assert.Equal(string.Empty, entry.GroupName);
            Assert.True(entry.IsEnabled);
            Assert.True(editor.HasUnsavedChanges);
            Assert.Empty(saved.CreateFolderMenu.Entries);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task AddEntry_UsesNextFallbackName()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку", CreateEntry("entry-view-1", "Вид 1", "Новая папка", true));
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            var entry = editor.AddEntry();

            Assert.Equal("Вид 2", entry.DisplayName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Reload_DiscardsAddedEntryAndPendingIcon()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            var sourceIcon = Path.Combine(root.FullName, "source.ico");
            await IcoTestFile.WriteValidAsync(sourceIcon);
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            var entry = editor.AddEntry();
            var iconResult = editor.SetPendingIconSource(entry.Id, sourceIcon);
            editor.Reload();

            Assert.True(iconResult.IsValid);
            Assert.Empty(editor.Entries);
            Assert.False(editor.HasUnsavedChanges);
            Assert.Empty(Directory.Exists(paths.IconsDirectory)
                ? Directory.GetFiles(paths.IconsDirectory, "*.ico")
                : []);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RemoveEntry_IsStagedUntilSave()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку", CreateEntry("entry-skull", "Череп", "Череп", true));
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            var removed = editor.RemoveEntry("entry-skull");
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(removed);
            Assert.Empty(editor.Entries);
            Assert.Single(saved.CreateFolderMenu.Entries);
            Assert.True(editor.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Reload_RestoresRemovedEntry()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папка", CreateEntry("entry-skull", "Череп", "Череп", true));
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            editor.RemoveEntry("entry-skull");
            editor.Reload();

            Assert.Single(editor.Entries);
            Assert.Equal("entry-skull", editor.Entries[0].Id);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_PersistsAddedEntryWithPendingIcon()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            var sourceIcon = Path.Combine(root.FullName, "source.ico");
            await IcoTestFile.WriteValidAsync(sourceIcon);
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            var entry = editor.AddEntry();
            editor.SetPendingIconSource(entry.Id, sourceIcon);
            var result = await editor.SaveAsync();
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Saved);
            var savedEntry = Assert.Single(saved.CreateFolderMenu.Entries);
            Assert.Equal(entry.Id, savedEntry.Id);
            Assert.Equal(Path.Combine(paths.IconsDirectory, $"{entry.Id}.ico"), savedEntry.IconPath);
            Assert.Null(savedEntry.PreviewPath);
            Assert.True(File.Exists(savedEntry.IconPath));
            Assert.False(editor.Entries[0].PendingIconSourcePath is not null);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_PersistsRemovalButDoesNotDeleteImportedIcon()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            var entry = CreateEntry("entry-skull", "Череп", "Череп", true);
            await SaveSettingsAsync(paths, false, "Создать папку", entry);
            var savedBefore = await new FoldoraSettingsStorage(paths).LoadAsync();
            var iconPath = savedBefore.CreateFolderMenu.Entries[0].IconPath;
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            editor.RemoveEntry("entry-skull");
            var result = await editor.SaveAsync();
            var savedAfter = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Saved);
            Assert.Empty(savedAfter.CreateFolderMenu.Entries);
            Assert.True(File.Exists(iconPath));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SetPendingIconSource_DoesNotImmediatelyChangeSettings()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку", CreateEntry("entry-skull", "Череп", "Череп", true));
            var original = await new FoldoraSettingsStorage(paths).LoadAsync();
            var originalIconPath = original.CreateFolderMenu.Entries[0].IconPath;
            var sourceIcon = Path.Combine(root.FullName, "new.ico");
            await IcoTestFile.WriteValidAsync(sourceIcon);
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            var validation = editor.SetPendingIconSource("entry-skull", sourceIcon);
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(validation.IsValid);
            Assert.Equal(originalIconPath, saved.CreateFolderMenu.Entries[0].IconPath);
            Assert.Equal(sourceIcon, editor.Entries[0].PendingIconSourcePath);
            Assert.True(editor.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Reload_ClearsPendingIconSourceAndDoesNotChangeSettings()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку", CreateEntry("entry-skull", "Череп", "Череп", true));
            var original = await new FoldoraSettingsStorage(paths).LoadAsync();
            var sourceIcon = Path.Combine(root.FullName, "new.ico");
            await IcoTestFile.WriteValidAsync(sourceIcon);
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            editor.SetPendingIconSource("entry-skull", sourceIcon);
            editor.Reload();
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.Null(editor.Entries[0].PendingIconSourcePath);
            Assert.Equal(original.CreateFolderMenu.Entries[0].IconPath, saved.CreateFolderMenu.Entries[0].IconPath);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SetPendingIconSource_RejectsInvalidIcon()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var paths = CreatePaths(root.FullName);
            await SaveSettingsAsync(paths, false, "Создать папку", CreateEntry("entry-skull", "Череп", "Череп", true));
            var invalidIcon = Path.Combine(root.FullName, "bad.ico");
            await File.WriteAllTextAsync(invalidIcon, "not ico");
            var editor = CreateEditor(paths);
            await editor.LoadAsync();

            var validation = editor.SetPendingIconSource("entry-skull", invalidIcon);

            Assert.False(validation.IsValid);
            Assert.Contains(validation.Issues, issue => issue.Code is "icon_header_invalid" or "icon_header_too_small");
            Assert.Null(editor.Entries[0].PendingIconSourcePath);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_BlocksAddedEntryWithoutIcon()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDraft-");

        try
        {
            var editor = CreateEditor(root.FullName);
            await editor.LoadAsync();

            editor.AddEntry();
            var result = await editor.SaveAsync();

            Assert.False(result.Saved);
            Assert.Contains(result.Issues, issue => issue.Code == "entry_icon_path_empty");
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private static FolderMenuDraftEditor CreateEditor(string root)
    {
        return CreateEditor(CreatePaths(root));
    }

    private static FolderMenuDraftEditor CreateEditor(FoldoraDataPaths paths)
    {
        return new FolderMenuDraftEditor(new FoldoraSettingsStorage(paths), paths);
    }

    private static FoldoraDataPaths CreatePaths(string root)
    {
        return new FoldoraDataPaths(Path.Combine(root, "Foldora"));
    }

    private static async Task SaveSettingsAsync(
        FoldoraDataPaths paths,
        bool explorerIntegrationEnabled,
        string title,
        params FolderMenuEntry[] entries)
    {
        var settings = new FoldoraSettings
        {
            ExplorerIntegrationEnabled = explorerIntegrationEnabled,
            CreateFolderMenu = new FolderMenuSettings { Title = title }
        };
        foreach (var entry in entries)
        {
            var iconPath = Path.Combine(paths.IconsDirectory, $"{entry.Id}.ico");
            await IcoTestFile.WriteValidAsync(iconPath);
            entry.IconPath = iconPath;
        }

        settings.CreateFolderMenu.Entries.AddRange(entries);

        await new FoldoraSettingsStorage(paths).SaveAsync(settings);
    }

    private static FolderMenuEntry CreateEntry(
        string id,
        string displayName,
        string defaultFolderName,
        bool isEnabled)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = displayName,
            DefaultFolderName = defaultFolderName,
            IconPath = $@"C:\Foldora\icons\{id}.ico",
            IsEnabled = isEnabled
        };
    }
}

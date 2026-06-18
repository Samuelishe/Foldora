using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;

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
    public async Task SaveAsync_WritesChangedDisplayNameDefaultFolderNameAndIsEnabled()
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
            editor.Entries[0].IsEnabled = false;
            var result = await editor.SaveAsync();
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Saved);
            Assert.Equal("Мои папки", saved.CreateFolderMenu.Title);
            Assert.Equal("Скелет", saved.CreateFolderMenu.Entries[0].DisplayName);
            Assert.Equal("Скелет", saved.CreateFolderMenu.Entries[0].DefaultFolderName);
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
            editor.Reload();

            Assert.Equal("Создать папку", editor.Title);
            Assert.Equal("Череп", editor.Entries[0].DisplayName);
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

    private static FolderMenuDraftEditor CreateEditor(string root)
    {
        return CreateEditor(CreatePaths(root));
    }

    private static FolderMenuDraftEditor CreateEditor(FoldoraDataPaths paths)
    {
        return new FolderMenuDraftEditor(new FoldoraSettingsStorage(paths));
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

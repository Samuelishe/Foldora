using Foldora.App.Services;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Shell.Registry;
using Foldora.Shell.RegistryPlan;
using Foldora.Tests.Shell.Fakes;

namespace Foldora.Tests.App;

public sealed class ExplorerIntegrationControllerTests
{
    [Fact]
    public async Task DryRun_BuildsValidatedPlanButDoesNotWriteRegistryOrChangeSettings()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: false, CreateEntry("entry-skull", "Череп"));
            var cliPath = await CreateFakeHostAsync(root.FullName);
            var registry = new FakeRegistryAccess();
            var controller = await CreateControllerAsync(paths, registry, cliPath);

            var result = await controller.DryRunAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Success);
            Assert.Equal("План проверен.", result.Message);
            Assert.False(result.ExplorerIntegrationEnabled);
            Assert.Empty(registry.Calls);
            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.Contains(result.Details, detail => detail.Contains("Operations:", StringComparison.Ordinal));
            Assert.Contains(result.Details, detail => detail.Contains("HKCU\\Software\\Classes\\Directory\\shell\\Foldora", StringComparison.Ordinal));
            Assert.Contains(result.Details, detail => detail.Contains("--entry-id", StringComparison.Ordinal));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task DryRun_BlocksWhenDraftHasUnsavedChanges()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, CreateEntry("entry-skull", "Череп"));
            var cliPath = await CreateFakeHostAsync(root.FullName);
            var registry = new FakeRegistryAccess();
            var (controller, editor) = await CreateControllerAndEditorAsync(paths, registry, cliPath);
            editor.Title = "Черновик";

            var result = await controller.DryRunAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.False(result.Success);
            Assert.Equal("Сначала сохраните изменения.", result.Message);
            Assert.True(result.ExplorerIntegrationEnabled);
            Assert.Empty(registry.Calls);
            Assert.Equal("Создать папку", settings.CreateFolderMenu.Title);
            Assert.True(settings.ExplorerIntegrationEnabled);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Register_BlocksWhenDraftHasUnsavedChanges()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: false, CreateEntry("entry-skull", "Череп"));
            var cliPath = await CreateFakeHostAsync(root.FullName);
            var registry = new FakeRegistryAccess();
            var (controller, editor) = await CreateControllerAndEditorAsync(paths, registry, cliPath);
            editor.Title = "Черновик";

            var result = await controller.RegisterAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.False(result.Success);
            Assert.Equal("Сначала сохраните изменения.", result.Message);
            Assert.Empty(registry.Calls);
            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.Equal("Создать папку", settings.CreateFolderMenu.Title);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Register_AppliesPlanAndEnablesIntegrationWhenDraftIsClean()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: false, CreateEntry("entry-skull", "Череп"));
            var cliPath = await CreateFakeHostAsync(root.FullName);
            var registry = new FakeRegistryAccess();
            var controller = await CreateControllerAsync(paths, registry, cliPath);

            var result = await controller.RegisterAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Success);
            Assert.Equal("Меню Проводника включено.", result.Message);
            Assert.True(result.ExplorerIntegrationEnabled);
            Assert.True(settings.ExplorerIntegrationEnabled);
            Assert.Contains(registry.Calls, call => call.StartsWith("delete:CurrentUser\\Software\\Classes\\Directory\\shell\\Foldora", StringComparison.Ordinal));
            Assert.Contains(registry.Calls, call => call.StartsWith("set:CurrentUser\\Software\\Classes\\Directory\\shell\\Foldora", StringComparison.Ordinal));
            Assert.All(registry.Keys, key => Assert.True(IsOwnedRegistryKey(key), key));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Register_WithNoEnabledEntriesRemovesOwnedRootsAndDisablesIntegration()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, CreateEntry("entry-disabled", "Череп", isEnabled: false));
            var cliPath = await CreateFakeHostAsync(root.FullName);
            var registry = new FakeRegistryAccess();
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryBackgroundRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, @"Software\Classes\Directory\shell\OtherApp");
            var controller = await CreateControllerAsync(paths, registry, cliPath);

            var result = await controller.RegisterAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Success);
            Assert.Equal("Нет включённых пунктов меню. Меню Проводника не создано.", result.Message);
            Assert.False(result.ExplorerIntegrationEnabled);
            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryRoot));
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryBackgroundRoot));
            Assert.True(registry.ContainsKey(@"Software\Classes\Directory\shell\OtherApp"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Unregister_RemovesOwnedRootsPreservesEntriesAndDisablesIntegration()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "en", explorerIntegrationEnabled: true, CreateEntry("entry-skull", "Череп"));
            var registry = new FakeRegistryAccess();
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryBackgroundRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, @"Software\Classes\Directory\shell\OtherApp");
            var controller = await CreateControllerAsync(paths, registry, await CreateFakeHostAsync(root.FullName));

            var result = await controller.UnregisterAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Success);
            Assert.Equal("Меню Проводника отключено.", result.Message);
            Assert.False(result.ExplorerIntegrationEnabled);
            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.Single(settings.CreateFolderMenu.Entries);
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryRoot));
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryBackgroundRoot));
            Assert.True(registry.ContainsKey(@"Software\Classes\Directory\shell\OtherApp"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Unregister_IsAllowedWithUnsavedDraftChangesAndPreservesDraft()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, CreateEntry("entry-skull", "Череп"));
            var registry = new FakeRegistryAccess();
            var (controller, editor) = await CreateControllerAndEditorAsync(paths, registry, await CreateFakeHostAsync(root.FullName));
            editor.Title = "Черновик";

            var result = await controller.UnregisterAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Success);
            Assert.False(result.ExplorerIntegrationEnabled);
            Assert.True(editor.HasUnsavedChanges);
            Assert.Equal("Черновик", editor.Title);
            Assert.Equal("Создать папку", settings.CreateFolderMenu.Title);
            Assert.False(settings.ExplorerIntegrationEnabled);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Reset_ClearsEntriesResetsTitleDisablesIntegrationAndPreservesAppDataFiles()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, CreateEntry("entry-skull", "Череп"));
            Directory.CreateDirectory(paths.IconsDirectory);
            Directory.CreateDirectory(paths.PacksDirectory);
            var iconPath = Path.Combine(paths.IconsDirectory, "entry-skull.ico");
            await File.WriteAllTextAsync(iconPath, "fake icon");
            var registry = new FakeRegistryAccess();
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryBackgroundRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, @"Software\Classes\Directory\shell\OtherApp");
            var controller = await CreateControllerAsync(paths, registry, await CreateFakeHostAsync(root.FullName));

            var result = await controller.ResetMenuAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Success);
            Assert.Equal("Список пунктов меню сброшен.", result.Message);
            Assert.False(result.ExplorerIntegrationEnabled);
            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.Equal("Создать папку", settings.CreateFolderMenu.Title);
            Assert.Empty(settings.CreateFolderMenu.Entries);
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryRoot));
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryBackgroundRoot));
            Assert.True(registry.ContainsKey(@"Software\Classes\Directory\shell\OtherApp"));
            Assert.True(Directory.Exists(paths.RootDirectory));
            Assert.True(File.Exists(paths.SettingsFile));
            Assert.True(File.Exists(iconPath));
            Assert.True(Directory.Exists(paths.PacksDirectory));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Reset_UsesCurrentLocalizedDefaultTitle()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "en", explorerIntegrationEnabled: true, CreateEntry("entry-skull", "Череп"));
            var localization = new InMemoryLocalizationService("en");
            var (controller, _) = await CreateControllerAndEditorAsync(
                paths,
                new FakeRegistryAccess(),
                await CreateFakeHostAsync(root.FullName),
                localization);

            var result = await controller.ResetMenuAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Success);
            Assert.Equal("Create folder", settings.CreateFolderMenu.Title);
            Assert.False(settings.CreateFolderMenu.TitleIsCustom);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RebuildAfterSave_AppliesPlanWithCommandHostAndUpdatesStatus()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var hostPath = await CreateFakeHostAsync(root.FullName);
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, CreateEntry("entry-skull", "Череп"));
            var registry = new FakeRegistryAccess();
            var controller = await CreateControllerAsync(paths, registry, hostPath);

            var result = await controller.RebuildAfterSaveAsync();

            Assert.True(result.Success);
            Assert.Equal("Настройки сохранены. Меню Проводника обновлено.", result.Message);
            Assert.True(result.ExplorerIntegrationEnabled);
            Assert.Contains(
                registry.Values.Values,
                value => value.StartsWith($@"""{hostPath}""", StringComparison.Ordinal)
                         && value.Contains("create", StringComparison.Ordinal));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RebuildAfterSave_WithNoEnabledEntriesRemovesMenuAndDisablesIntegration()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, CreateEntry("entry-disabled", "Череп", isEnabled: false));
            var registry = new FakeRegistryAccess();
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryBackgroundRoot);
            var controller = await CreateControllerAsync(paths, registry, await CreateFakeHostAsync(root.FullName));

            var result = await controller.RebuildAfterSaveAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Success);
            Assert.Equal("Настройки сохранены. Включённых пунктов нет, меню Проводника отключено.", result.Message);
            Assert.False(result.ExplorerIntegrationEnabled);
            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryRoot));
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryBackgroundRoot));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RebuildAfterSave_WhenRegistryFailsKeepsSavedSettingsAndReportsFailure()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraAppIntegration-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, CreateEntry("entry-skull", "Череп"));
            var missingHostPath = Path.Combine(root.FullName, "Missing", "Foldora.MenuHost.exe");
            var registry = new FakeRegistryAccess();
            var controller = await CreateControllerAsync(paths, registry, missingHostPath);

            var result = await controller.RebuildAfterSaveAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.False(result.Success);
            Assert.Equal("Настройки сохранены, но меню Проводника не обновлено.", result.Message);
            Assert.True(settings.ExplorerIntegrationEnabled);
            Assert.Single(settings.CreateFolderMenu.Entries);
            Assert.Empty(registry.Calls);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private static async Task<ExplorerIntegrationController> CreateControllerAsync(
        FoldoraDataPaths paths,
        FakeRegistryAccess registry,
        string cliPath)
    {
        var (controller, _) = await CreateControllerAndEditorAsync(paths, registry, cliPath);
        return controller;
    }

    private static async Task<(ExplorerIntegrationController Controller, FolderMenuDraftEditor Editor)> CreateControllerAndEditorAsync(
        FoldoraDataPaths paths,
        FakeRegistryAccess registry,
        string cliPath,
        ILocalizationService? localizationService = null)
    {
        var storage = new FoldoraSettingsStorage(paths);
        var editor = new FolderMenuDraftEditor(storage, paths);
        await editor.LoadAsync();

        var service = new ExplorerMenuRegistrationService(
            storage,
            new ExplorerMenuRegistryPlanBuilder(),
            new ExplorerMenuRegistryWriter(registry));

        return (new ExplorerIntegrationController(editor, service, new FixedHostPathResolver(cliPath), localizationService), editor);
    }

    private static async Task SaveSettingsAsync(
        FoldoraDataPaths paths,
        bool explorerIntegrationEnabled,
        params FolderMenuEntry[] entries)
    {
        await SaveSettingsAsync(paths, FoldoraLanguage.Russian, explorerIntegrationEnabled, entries);
    }

    private static async Task SaveSettingsAsync(
        FoldoraDataPaths paths,
        string language,
        bool explorerIntegrationEnabled,
        params FolderMenuEntry[] entries)
    {
        var settings = new FoldoraSettings
        {
            Language = language,
            ExplorerIntegrationEnabled = explorerIntegrationEnabled,
            CreateFolderMenu = FolderMenuSettings.CreateDefault(language)
        };
        foreach (var entry in entries)
        {
            settings.CreateFolderMenu.Entries.Add(entry);
        }

        await new FoldoraSettingsStorage(paths).SaveAsync(settings);
    }

    private static FolderMenuEntry CreateEntry(string id, string displayName, bool isEnabled = true)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = displayName,
            DefaultFolderName = "Череп",
            IconPath = $@"C:\Foldora\icons\{id}.ico",
            IsEnabled = isEnabled
        };
    }

    private static async Task<string> CreateFakeHostAsync(string root)
    {
        var path = Path.Combine(root, "Program Files", "Foldora", "Foldora.MenuHost.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, "fake exe");
        return path;
    }

    private static bool IsOwnedRegistryKey(string fullKey)
    {
        return fullKey.StartsWith($@"{ExplorerMenuRegistryHive.CurrentUser}\{ExplorerMenuRegistryPaths.DirectoryRoot}", StringComparison.OrdinalIgnoreCase)
               || fullKey.StartsWith($@"{ExplorerMenuRegistryHive.CurrentUser}\{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FixedHostPathResolver : IExplorerCommandHostPathResolver
    {
        private readonly string hostPath;

        public FixedHostPathResolver(string hostPath)
        {
            this.hostPath = hostPath;
        }

        public string ResolveCommandHostPath()
        {
            return hostPath;
        }
    }
}

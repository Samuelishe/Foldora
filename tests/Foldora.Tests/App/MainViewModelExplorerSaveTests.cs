using Foldora.App.Services;
using Foldora.App.ViewModels;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Shell.Registry;
using Foldora.Shell.RegistryPlan;
using Foldora.Tests.Fixtures;
using Foldora.Tests.Shell.Fakes;

namespace Foldora.Tests.App;

public sealed class MainViewModelExplorerSaveTests
{
    [Fact]
    public async Task SaveDraft_WithExplorerIntegrationDisabledDoesNotWriteRegistry()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMainVmSave-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var iconPath = await CreateIconAsync(paths, "entry-skull");
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: false, CreateEntry("entry-skull", "Череп", iconPath));
            var registry = new FakeRegistryAccess();
            var viewModel = await CreateViewModelAsync(paths, registry, await CreateFakeHostAsync(root.FullName));

            viewModel.Title = "Мои папки";
            await viewModel.SaveDraftAsync();

            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();
            Assert.Equal("Настройки сохранены.", viewModel.StatusMessage);
            Assert.Empty(registry.Calls);
            Assert.Equal("Мои папки", settings.CreateFolderMenu.Title);
            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.False(viewModel.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveDraft_WithExplorerIntegrationEnabledRebuildsRegistry()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMainVmSave-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var hostPath = await CreateFakeHostAsync(root.FullName);
            var iconPath = await CreateIconAsync(paths, "entry-skull");
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, CreateEntry("entry-skull", "Череп", iconPath));
            var registry = new FakeRegistryAccess();
            var viewModel = await CreateViewModelAsync(paths, registry, hostPath);

            viewModel.Title = "Мои папки";
            await viewModel.SaveDraftAsync();

            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();
            Assert.Equal("Настройки сохранены. Меню Проводника обновлено.", viewModel.StatusMessage);
            Assert.True(settings.ExplorerIntegrationEnabled);
            Assert.True(viewModel.ExplorerIntegrationEnabled);
            Assert.False(viewModel.HasUnsavedChanges);
            Assert.Contains(
                registry.Values.Values,
                value => value.StartsWith($@"""{hostPath}""", StringComparison.Ordinal));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveDraft_WithExplorerIntegrationEnabledRebuildsGroupedRegistryMenu()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMainVmSave-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var hostPath = await CreateFakeHostAsync(root.FullName);
            var iconPath = await CreateIconAsync(paths, "entry-blue");
            var entry = CreateEntry("entry-blue", "Синяя", iconPath);
            entry.GroupName = "Цветные";
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, entry);
            var registry = new FakeRegistryAccess();
            var viewModel = await CreateViewModelAsync(paths, registry, hostPath);

            viewModel.Title = "Мои папки";
            await viewModel.SaveDraftAsync();

            Assert.True(registry.ContainsKey($@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\group-001"));
            Assert.True(registry.ContainsKey($@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\group-001\shell\entry-001-entry-blue"));
            Assert.Contains(
                registry.Values,
                value => value.Key.KeyPath == $@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\group-001"
                         && value.Key.ValueName == "MUIVerb"
                         && value.Value == "Цветные");
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private static async Task<MainViewModel> CreateViewModelAsync(
        FoldoraDataPaths paths,
        FakeRegistryAccess registry,
        string hostPath)
    {
        var storage = new FoldoraSettingsStorage(paths);
        var draftEditor = new FolderMenuDraftEditor(storage, paths);
        var registrationService = new ExplorerMenuRegistrationService(
            storage,
            new ExplorerMenuRegistryPlanBuilder(),
            new ExplorerMenuRegistryWriter(registry));
        var controller = new ExplorerIntegrationController(
            draftEditor,
            registrationService,
            new FixedHostPathResolver(hostPath));
        var viewModel = new MainViewModel(
            draftEditor,
            new NoopIconFilePicker(),
            new NoopIconPreviewService(),
            controller);

        await viewModel.LoadAsync();
        return viewModel;
    }

    private static async Task SaveSettingsAsync(
        FoldoraDataPaths paths,
        bool explorerIntegrationEnabled,
        params FolderMenuEntry[] entries)
    {
        var settings = new FoldoraSettings { ExplorerIntegrationEnabled = explorerIntegrationEnabled };
        foreach (var entry in entries)
        {
            settings.CreateFolderMenu.Entries.Add(entry);
        }

        await new FoldoraSettingsStorage(paths).SaveAsync(settings);
    }

    private static FolderMenuEntry CreateEntry(string id, string displayName, string iconPath)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = displayName,
            DefaultFolderName = displayName,
            IconPath = iconPath,
            IsEnabled = true
        };
    }

    private static async Task<string> CreateIconAsync(FoldoraDataPaths paths, string entryId)
    {
        var iconPath = Path.Combine(paths.IconsDirectory, $"{entryId}.ico");
        await IcoTestFile.WriteValidAsync(iconPath);
        return iconPath;
    }

    private static async Task<string> CreateFakeHostAsync(string root)
    {
        var path = Path.Combine(root, "Program Files", "Foldora", "Foldora.MenuHost.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, "fake exe");
        return path;
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

    private sealed class NoopIconFilePicker : IIconFilePicker
    {
        public IconFilePickerResult PickIcon()
        {
            return IconFilePickerResult.Cancelled;
        }
    }

    private sealed class NoopIconPreviewService : IIconPreviewService
    {
        public IconPreviewResult LoadPreview(string? iconPath)
        {
            return IconPreviewResult.NoIcon;
        }
    }
}

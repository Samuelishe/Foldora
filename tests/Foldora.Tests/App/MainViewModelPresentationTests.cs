using Foldora.App.Services;
using Foldora.App.ViewModels;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Shell.Registry;
using Foldora.Shell.RegistryPlan;
using Foldora.Tests.Shell.Fakes;

namespace Foldora.Tests.App;

public sealed class MainViewModelPresentationTests
{
    [Fact]
    public async Task HasEntries_IsFalseWhenMenuIsEmpty()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var viewModel = await CreateViewModelAsync(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora")));

            Assert.False(viewModel.HasEntries);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task HasEntries_IsTrueWhenMenuHasEntries()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, CreateEntry("entry-skull", "Череп"));

            var viewModel = await CreateViewModelAsync(paths);

            Assert.True(viewModel.HasEntries);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ShowTechnicalDetails_CanBeToggled()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var viewModel = await CreateViewModelAsync(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora")));

            Assert.False(viewModel.ShowTechnicalDetails);

            viewModel.ShowTechnicalDetails = true;

            Assert.True(viewModel.ShowTechnicalDetails);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ResetCommandAvailabilityFollowsConfirmation()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var viewModel = await CreateViewModelAsync(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora")));

            Assert.False(viewModel.CanResetMenu);
            Assert.False(viewModel.ResetMenuCommand.CanExecute(null));

            viewModel.IsResetConfirmed = true;

            Assert.True(viewModel.CanResetMenu);
            Assert.True(viewModel.ResetMenuCommand.CanExecute(null));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task OpenSettingsAsync_UsesSettingsDialogService()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var settingsDialog = new RecordingSettingsDialogService();
            var viewModel = await CreateViewModelAsync(paths, settingsDialog);

            await viewModel.OpenSettingsAsync();

            Assert.True(settingsDialog.WasCalled);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private static Task<MainViewModel> CreateViewModelAsync(FoldoraDataPaths paths)
    {
        return CreateViewModelAsync(paths, new RecordingSettingsDialogService());
    }

    private static async Task<MainViewModel> CreateViewModelAsync(
        FoldoraDataPaths paths,
        ISettingsDialogService settingsDialogService)
    {
        var storage = new FoldoraSettingsStorage(paths);
        var draftEditor = new FolderMenuDraftEditor(storage, paths);
        var registrationService = new ExplorerMenuRegistrationService(
            storage,
            new ExplorerMenuRegistryPlanBuilder(),
            new ExplorerMenuRegistryWriter(new FakeRegistryAccess()));
        var viewModel = new MainViewModel(
            draftEditor,
            new NoopIconFilePicker(),
            new NoopIconPreviewService(),
            new ExplorerIntegrationController(
                draftEditor,
                registrationService,
                new FixedHostPathResolver(Path.Combine(paths.RootDirectory, "Foldora.MenuHost.exe"))),
            settingsDialogService);

        await viewModel.LoadAsync();
        return viewModel;
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

    private static FolderMenuEntry CreateEntry(string id, string displayName)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = displayName,
            DefaultFolderName = displayName,
            IconPath = string.Empty,
            IsEnabled = true
        };
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

    private sealed class RecordingSettingsDialogService : ISettingsDialogService
    {
        public bool WasCalled { get; private set; }

        public Task<SettingsDialogResult> ShowSettingsAsync()
        {
            WasCalled = true;
            return Task.FromResult(new SettingsDialogResult(false, "ru"));
        }
    }
}

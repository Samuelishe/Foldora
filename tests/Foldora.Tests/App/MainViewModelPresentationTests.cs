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
    public void CreateDefault_DoesNotSynchronouslyLoadSettings()
    {
        var viewModel = MainViewModel.CreateDefault();

        Assert.NotNull(viewModel);
    }

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
    public async Task EntriesExposeEditableGroupName()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var entry = CreateEntry("entry-blue", "Синяя");
            entry.GroupName = "Цветные";
            await SaveSettingsAsync(paths, entry);

            var viewModel = await CreateViewModelAsync(paths);
            viewModel.Entries[0].GroupName = "Готические";

            Assert.Equal("Готические", viewModel.Entries[0].GroupName);
            Assert.True(viewModel.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task EntryGroups_ExposeRootAndNamedSections()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var rootEntry = CreateEntry("entry-music", "Музыка");
            var groupedEntry = CreateEntry("entry-blue", "Синяя");
            groupedEntry.GroupName = "Цветные";
            await SaveSettingsAsync(paths, rootEntry, groupedEntry);

            var viewModel = await CreateViewModelAsync(paths);

            Assert.Equal(2, viewModel.EntryGroups.Count);
            Assert.Equal("Без группы", viewModel.EntryGroups[0].Title);
            Assert.Equal("Элементов: 1", viewModel.EntryGroups[0].EntryCountText);
            Assert.Single(viewModel.EntryGroups[0].Entries);
            Assert.Equal("Цветные", viewModel.EntryGroups[1].Title);
            Assert.Equal("Элементов: 1", viewModel.EntryGroups[1].EntryCountText);
            Assert.Single(viewModel.EntryGroups[1].Entries);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task AddGroupCommand_CreatesDraftEntryInNewGroup()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var viewModel = await CreateViewModelAsync(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora")));

            viewModel.AddGroupCommand.Execute(null);

            Assert.True(viewModel.HasEntries);
            Assert.Single(viewModel.Entries);
            Assert.Equal("Группа 1", viewModel.Entries[0].GroupName);
            Assert.Equal("Группа 1", viewModel.EntryGroups[0].Title);
            Assert.True(viewModel.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ChangingGroupName_RebuildsEntryGroups()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, CreateEntry("entry-music", "Музыка"));

            var viewModel = await CreateViewModelAsync(paths);
            viewModel.Entries[0].GroupName = "Музыка";

            Assert.Single(viewModel.EntryGroups);
            Assert.Equal("Музыка", viewModel.EntryGroups[0].Title);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task DeleteGroup_RemovesAllEntriesInGroupFromDraft()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var blue = CreateEntry("entry-blue", "Синяя");
            blue.GroupName = "Цветные";
            var red = CreateEntry("entry-red", "Красная");
            red.GroupName = "Цветные";
            var music = CreateEntry("entry-music", "Музыка");
            await SaveSettingsAsync(paths, blue, red, music);

            var viewModel = await CreateViewModelAsync(paths);
            var group = viewModel.EntryGroups.Single(section => section.Title == "Цветные");

            group.DeleteGroupCommand.Execute(null);

            Assert.True(group.IsDeleteConfirmationVisible);

            group.ConfirmDeleteGroupCommand.Execute(null);

            Assert.DoesNotContain(viewModel.Entries, entry => entry.GroupName == "Цветные");
            Assert.Single(viewModel.Entries);
            Assert.Equal("Музыка", viewModel.Entries[0].DisplayName);
            Assert.True(viewModel.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task DeleteEntry_RemovesOnlyThatEntryFromDraft()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var blue = CreateEntry("entry-blue", "Синяя");
            blue.GroupName = "Цветные";
            var red = CreateEntry("entry-red", "Красная");
            red.GroupName = "Цветные";
            await SaveSettingsAsync(paths, blue, red);

            var viewModel = await CreateViewModelAsync(paths);

            viewModel.Entries.First(entry => entry.Id == "entry-blue").RemoveCommand.Execute(null);

            Assert.Single(viewModel.Entries);
            Assert.Equal("entry-red", viewModel.Entries[0].Id);
            Assert.Single(viewModel.EntryGroups);
            Assert.Equal("Цветные", viewModel.EntryGroups[0].Title);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task GroupSection_DisappearsWhenLastEntryIsRemoved()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var blue = CreateEntry("entry-blue", "Синяя");
            blue.GroupName = "Цветные";
            await SaveSettingsAsync(paths, blue);

            var viewModel = await CreateViewModelAsync(paths);

            viewModel.Entries[0].RemoveCommand.Execute(null);

            Assert.Empty(viewModel.Entries);
            Assert.Empty(viewModel.EntryGroups);
            Assert.False(viewModel.HasEntries);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RenameGroup_UpdatesAllEntriesInGroup()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var blue = CreateEntry("entry-blue", "Синяя");
            blue.GroupName = "Цветные";
            var red = CreateEntry("entry-red", "Красная");
            red.GroupName = "Цветные";
            await SaveSettingsAsync(paths, blue, red);

            var viewModel = await CreateViewModelAsync(paths);

            viewModel.EntryGroups.Single(section => section.Title == "Цветные").Title = "Палитра";

            Assert.All(viewModel.Entries, entry => Assert.Equal("Палитра", entry.GroupName));
            Assert.Single(viewModel.EntryGroups);
            Assert.Equal("Палитра", viewModel.EntryGroups[0].Title);
            Assert.True(viewModel.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ExistingSavedEntry_StartsCompact()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, CreateEntry("entry-music", "Музыка"));

            var viewModel = await CreateViewModelAsync(paths);

            Assert.False(viewModel.Entries[0].IsEditing);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task NewDraftEntry_StartsInEditMode()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var viewModel = await CreateViewModelAsync(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora")));

            viewModel.AddEntryCommand.Execute(null);

            Assert.True(viewModel.Entries[0].IsEditing);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task EditAndFinishCommands_TogglePresentationEditStateWithoutSaving()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, CreateEntry("entry-music", "Музыка"));
            var viewModel = await CreateViewModelAsync(paths);
            var entry = viewModel.Entries[0];

            entry.EditCommand.Execute(null);

            Assert.True(entry.IsEditing);
            Assert.False(viewModel.HasUnsavedChanges);

            entry.FinishEditingCommand.Execute(null);

            Assert.False(entry.IsEditing);
            Assert.False(viewModel.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task DraftValuesSurviveCollapseAndExpand()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, CreateEntry("entry-music", "Музыка"));
            var viewModel = await CreateViewModelAsync(paths);
            var entry = viewModel.Entries[0];

            entry.EditCommand.Execute(null);
            entry.DisplayName = "Видео";
            entry.DefaultFolderName = "Видео";
            entry.FinishEditingCommand.Execute(null);
            entry.EditCommand.Execute(null);

            Assert.Equal("Видео", entry.DisplayName);
            Assert.Equal("Видео", entry.DefaultFolderName);
            Assert.True(viewModel.HasUnsavedChanges);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ValidationError_ReopensAffectedEntryAndShowsInlineError()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, CreateEntry("entry-invalid", "Музыка"));
            var viewModel = await CreateViewModelAsync(paths);
            var entry = viewModel.Entries[0];

            entry.FinishEditingCommand.Execute(null);
            await viewModel.SaveDraftAsync();

            Assert.True(entry.IsEditing);
            Assert.True(entry.HasInlineErrors);
            Assert.NotEmpty(entry.InlineErrors);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task OpeningOneEntry_CollapsesAnotherEntryWithoutLosingDraftValues()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, CreateEntry("entry-one", "Один"), CreateEntry("entry-two", "Два"));
            var viewModel = await CreateViewModelAsync(paths);
            var first = viewModel.Entries[0];
            var second = viewModel.Entries[1];

            first.EditCommand.Execute(null);
            first.DisplayName = "Первый";
            second.EditCommand.Execute(null);

            Assert.False(first.IsEditing);
            Assert.True(second.IsEditing);
            Assert.Equal("Первый", first.DisplayName);
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

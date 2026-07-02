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
    public async Task NewDraftEntry_UsesEnglishDefaultsWhenUiLanguageIsEnglish()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "en");
            var viewModel = await CreateViewModelAsync(paths);

            viewModel.AddEntryCommand.Execute(null);

            Assert.Equal("View 1", viewModel.Entries[0].DisplayName);
            Assert.Equal("New folder", viewModel.Entries[0].DefaultFolderName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task NewDraftEntry_UsesRussianDefaultsWhenUiLanguageIsRussian()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "ru");
            var viewModel = await CreateViewModelAsync(paths);

            viewModel.AddEntryCommand.Execute(null);

            Assert.Equal("Вид 1", viewModel.Entries[0].DisplayName);
            Assert.Equal("Новая папка", viewModel.Entries[0].DefaultFolderName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task UnsavedChangesText_UsesUserFacingCleanAndDirtyStates()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "en");
            var viewModel = await CreateViewModelAsync(paths, new RecordingSettingsDialogService(), "en-US");

            Assert.Equal("All changes saved", viewModel.UnsavedChangesText);
            Assert.DoesNotContain("True", viewModel.UnsavedChangesText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("False", viewModel.UnsavedChangesText, StringComparison.OrdinalIgnoreCase);

            viewModel.AddEntryCommand.Execute(null);

            Assert.Equal("Unsaved changes", viewModel.UnsavedChangesText);
            Assert.DoesNotContain("True", viewModel.UnsavedChangesText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("False", viewModel.UnsavedChangesText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task UnsavedChangesText_UsesRussianUserFacingStates()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "ru");
            var viewModel = await CreateViewModelAsync(paths);

            Assert.Equal("Все изменения сохранены", viewModel.UnsavedChangesText);

            viewModel.AddEntryCommand.Execute(null);

            Assert.Equal("Есть несохранённые изменения", viewModel.UnsavedChangesText);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData("ru-RU", "ru", "Создать папку")]
    [InlineData("de-DE", "de", "Ordner erstellen")]
    [InlineData("it-IT", "it", "Crea cartella")]
    public async Task LoadAsync_FirstRunDetectsAndPersistsSystemLanguage(
        string systemLanguage,
        string expectedLanguage,
        string expectedTitle)
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var viewModel = await CreateViewModelAsync(
                paths,
                new RecordingSettingsDialogService(),
                systemLanguage);

            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();
            Assert.Equal(expectedLanguage, saved.Language);
            Assert.Equal(expectedTitle, viewModel.Title);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Theory]
    [MemberData(nameof(EnabledLocales))]
    public async Task NewDraftEntry_UsesLocalizedDefaultsForEveryEnabledLocale(string language)
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, language);
            var viewModel = await CreateViewModelAsync(paths);
            var expected = new InMemoryLocalizationService(language).Resources;

            viewModel.AddEntryCommand.Execute(null);

            Assert.Equal($"{expected.DefaultEntryDisplayNamePrefix} 1", viewModel.Entries[0].DisplayName);
            Assert.Equal(expected.DefaultFolderName, viewModel.Entries[0].DefaultFolderName);
            Assert.Equal(expected.CreateFolderMenuTitle, viewModel.Title);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Theory]
    [MemberData(nameof(EnabledLocales))]
    public async Task ExistingUserData_IsNotTranslatedWhenSwitchingToEnabledLocale(string language)
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(
                paths,
                "ru",
                "Мои папки",
                titleIsCustom: true,
                CreateEntry("entry-view-1", "Вид 1", "Новая папка"));
            var viewModel = await CreateViewModelAsync(
                paths,
                new RecordingSettingsDialogService(language));

            await viewModel.OpenSettingsAsync();

            Assert.Equal("Мои папки", viewModel.Title);
            Assert.Equal("Вид 1", viewModel.Entries[0].DisplayName);
            Assert.Equal("Новая папка", viewModel.Entries[0].DefaultFolderName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_ManualLanguageChoiceIsNotOverriddenBySystemLanguage()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "en");

            var viewModel = await CreateViewModelAsync(
                paths,
                new RecordingSettingsDialogService("ru"),
                "ru-RU");

            Assert.Equal("Create folder", viewModel.Title);

            await viewModel.OpenSettingsAsync();
            await viewModel.SaveDraftAsync();

            var reloaded = await CreateViewModelAsync(
                paths,
                new RecordingSettingsDialogService(),
                "en-US");
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.Equal("ru", saved.Language);
            Assert.Equal("Создать папку", reloaded.Title);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ExistingUserData_IsNotTranslatedWhenUiLanguageIsEnglish()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(
                paths,
                "en",
                CreateEntry("entry-view-1", "Вид 1", "Новая папка"));
            var viewModel = await CreateViewModelAsync(paths);

            Assert.Equal("Create folder", viewModel.Title);
            Assert.Equal("Вид 1", viewModel.Entries[0].DisplayName);
            Assert.Equal("Новая папка", viewModel.Entries[0].DefaultFolderName);

            viewModel.AddEntryCommand.Execute(null);

            Assert.Equal("View 1", viewModel.Entries[1].DisplayName);
            Assert.Equal("New folder", viewModel.Entries[1].DefaultFolderName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task CustomTitle_IsNotTranslatedWhenUiLanguageIsEnglish()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(
                paths,
                "en",
                "Мои папки",
                titleIsCustom: true,
                CreateEntry("entry-view-1", "Вид 1", "Новая папка"));

            var viewModel = await CreateViewModelAsync(paths);

            Assert.Equal("Мои папки", viewModel.Title);
            Assert.Equal("Вид 1", viewModel.Entries[0].DisplayName);
            Assert.Equal("Новая папка", viewModel.Entries[0].DefaultFolderName);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task LanguageSwitch_UpdatesDefaultTitleButNotCustomTitle()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var defaultTitleViewModel = await CreateViewModelAsync(
                paths,
                new RecordingSettingsDialogService("en"));

            await defaultTitleViewModel.OpenSettingsAsync();

            Assert.Equal("Create folder", defaultTitleViewModel.Title);

            var customPaths = new FoldoraDataPaths(Path.Combine(root.FullName, "FoldoraCustom"));
            await SaveSettingsAsync(customPaths, "ru", "Мои папки", titleIsCustom: true);
            var customTitleViewModel = await CreateViewModelAsync(
                customPaths,
                new RecordingSettingsDialogService("en"));

            await customTitleViewModel.OpenSettingsAsync();

            Assert.Equal("Мои папки", customTitleViewModel.Title);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAfterLanguageSwitch_PreservesNewLanguageAndLocalizedDefaultTitle()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var viewModel = await CreateViewModelAsync(
                paths,
                new RecordingSettingsDialogService("en"));

            await viewModel.OpenSettingsAsync();
            await viewModel.SaveDraftAsync();
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.Equal("en", saved.Language);
            Assert.Equal("Create folder", saved.CreateFolderMenu.Title);
            Assert.False(saved.CreateFolderMenu.TitleIsCustom);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task EditingMenuTitle_MarksTitleAsCustom()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "en");
            var viewModel = await CreateViewModelAsync(paths);

            viewModel.Title = "Создать папку";
            await viewModel.SaveDraftAsync();
            var saved = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.Equal("Создать папку", saved.CreateFolderMenu.Title);
            Assert.True(saved.CreateFolderMenu.TitleIsCustom);
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
    public async Task ValidationErrors_AreRenderedInEnglishWhenUiLanguageIsEnglish()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "en", CreateEntry("entry-invalid", "Music", "Bad:Name"));
            var viewModel = await CreateViewModelAsync(paths);

            await viewModel.SaveDraftAsync();

            Assert.Contains(
                viewModel.Errors,
                error => error.Contains("Created folder name contains invalid character \":\".", StringComparison.Ordinal));
            Assert.Contains(
                viewModel.Entries[0].InlineErrors,
                error => error == "Created folder name contains invalid character \":\".");
            Assert.DoesNotContain(viewModel.Errors, error => error.Contains("Имя создаваемой папки", StringComparison.Ordinal));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ValidationErrors_AreRenderedInRussianWhenUiLanguageIsRussian()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "ru", CreateEntry("entry-invalid", "Музыка", "Bad:Name"));
            var viewModel = await CreateViewModelAsync(paths);

            await viewModel.SaveDraftAsync();

            Assert.Contains(
                viewModel.Errors,
                error => error.Contains("Имя создаваемой папки содержит недопустимый символ \":\".", StringComparison.Ordinal));
            Assert.Contains(
                viewModel.Entries[0].InlineErrors,
                error => error == "Имя создаваемой папки содержит недопустимый символ \":\".");
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

    [Fact]
    public async Task ChooseIconCommand_UsesPreparedGeneratedIconAndSavePersistsIt()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraVmPresentation-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, "en");
            var generatedIcon = Path.Combine(paths.IconsDirectory, "generated", "source-12345678.ico");
            await IcoTestFile.WriteValidAsync(generatedIcon);
            var storage = new FoldoraSettingsStorage(paths);
            var draftEditor = new FolderMenuDraftEditor(storage, paths);
            var localizationService = new InMemoryLocalizationService();
            var registrationService = new ExplorerMenuRegistrationService(
                storage,
                new ExplorerMenuRegistryPlanBuilder(),
                new ExplorerMenuRegistryWriter(new FakeRegistryAccess()));
            var viewModel = new MainViewModel(
                draftEditor,
                new FixedIconFilePicker(Path.Combine(root.FullName, "source.png")),
                new NoopIconPreviewService(),
                new ExplorerIntegrationController(
                    draftEditor,
                    registrationService,
                    new FixedHostPathResolver(Path.Combine(paths.RootDirectory, "Foldora.MenuHost.exe")),
                    localizationService),
                localizationService: localizationService,
                settingsLanguageInitializer: new NoopSettingsLanguageInitializer(),
                iconAssetPreparationService: new FixedIconAssetPreparationService(generatedIcon));
            await viewModel.LoadAsync();

            viewModel.AddEntryCommand.Execute(null);
            await viewModel.Entries[0].ChooseIconCommand.ExecuteAsync(null);
            await viewModel.SaveDraftAsync();
            var saved = await storage.LoadAsync();

            Assert.Equal(generatedIcon, Assert.Single(saved.CreateFolderMenu.Entries).IconPath);
            Assert.False(viewModel.HasUnsavedChanges);
            Assert.Empty(viewModel.Errors);
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
        ISettingsDialogService settingsDialogService,
        string systemLanguage = "ru-RU")
    {
        var storage = new FoldoraSettingsStorage(paths);
        var draftEditor = new FolderMenuDraftEditor(storage, paths);
        var localizationService = new InMemoryLocalizationService();
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
                new FixedHostPathResolver(Path.Combine(paths.RootDirectory, "Foldora.MenuHost.exe")),
                localizationService),
            settingsDialogService,
            localizationService,
            settingsLanguageInitializer: new SettingsLanguageInitializer(
                storage,
                new FixedSystemLanguageProvider(systemLanguage)));

        await viewModel.LoadAsync();
        return viewModel;
    }

    private static async Task SaveSettingsAsync(FoldoraDataPaths paths, params FolderMenuEntry[] entries)
    {
        await SaveSettingsAsync(paths, "ru", entries);
    }

    private static async Task SaveSettingsAsync(FoldoraDataPaths paths, string language, params FolderMenuEntry[] entries)
    {
        await SaveSettingsAsync(paths, language, FolderMenuDefaultTitles.GetForLanguage(language), titleIsCustom: false, entries);
    }

    private static async Task SaveSettingsAsync(
        FoldoraDataPaths paths,
        string language,
        string title,
        bool titleIsCustom,
        params FolderMenuEntry[] entries)
    {
        var settings = new FoldoraSettings
        {
            Language = language,
            CreateFolderMenu = new FolderMenuSettings
            {
                Title = title,
                TitleIsCustom = titleIsCustom
            }
        };
        foreach (var entry in entries)
        {
            settings.CreateFolderMenu.Entries.Add(entry);
        }

        await new FoldoraSettingsStorage(paths).SaveAsync(settings);
    }

    private static FolderMenuEntry CreateEntry(string id, string displayName)
    {
        return CreateEntry(id, displayName, displayName);
    }

    private static FolderMenuEntry CreateEntry(string id, string displayName, string defaultFolderName)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = displayName,
            DefaultFolderName = defaultFolderName,
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

    private sealed class FixedIconFilePicker : IIconFilePicker
    {
        private readonly string filePath;

        public FixedIconFilePicker(string filePath)
        {
            this.filePath = filePath;
        }

        public IconFilePickerResult PickIcon()
        {
            return IconFilePickerResult.FromPath(filePath);
        }
    }

    private sealed class FixedIconAssetPreparationService : IIconAssetPreparationService
    {
        private readonly string generatedIconPath;

        public FixedIconAssetPreparationService(string generatedIconPath)
        {
            this.generatedIconPath = generatedIconPath;
        }

        public Task<IconAssetPreparationResult> PrepareAsync(string selectedFilePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IconAssetPreparationResult.GeneratedIcon(generatedIconPath));
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
        private readonly string language;

        public RecordingSettingsDialogService(string language = "ru")
        {
            this.language = language;
        }

        public bool WasCalled { get; private set; }

        public Task<SettingsDialogResult> ShowSettingsAsync()
        {
            WasCalled = true;
            return Task.FromResult(new SettingsDialogResult(true, language));
        }
    }

    private sealed class FixedSystemLanguageProvider : ISystemLanguageProvider
    {
        public FixedSystemLanguageProvider(string currentUiCultureName)
        {
            CurrentUiCultureName = currentUiCultureName;
        }

        public string CurrentUiCultureName { get; }
    }

    private sealed class NoopSettingsLanguageInitializer : ISettingsLanguageInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public static TheoryData<string> EnabledLocales()
    {
        var data = new TheoryData<string>();
        foreach (var locale in FoldoraLanguage.SupportedLocales)
        {
            data.Add(locale);
        }

        return data;
    }
}

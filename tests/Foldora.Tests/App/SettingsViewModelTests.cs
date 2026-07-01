using Foldora.App.Services;
using Foldora.App.ViewModels;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Shell.Registry;
using Foldora.Shell.RegistryPlan;
using Foldora.Tests.Shell.Fakes;

namespace Foldora.Tests.App;

public sealed class SettingsViewModelTests
{
    [Fact]
    public void AvailableLanguages_ContainsEnabledCompleteLocales()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var viewModel = new SettingsViewModel(
                new FoldoraSettingsStorage(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"))),
                "ru");

            Assert.Equal(FoldoraLanguage.SupportedLocales.Order(StringComparer.Ordinal), viewModel.AvailableLanguages.Select(language => language.Code).Order(StringComparer.Ordinal).ToArray());
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.SimplifiedChinese && language.DisplayName == "简体中文");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.TraditionalChinese && language.DisplayName == "繁體中文");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.BrazilianPortuguese && language.DisplayName == "Português (Brasil)");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.PortuguesePortugal && language.DisplayName == "Português (Portugal)");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Ukrainian && language.DisplayName == "Українська");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Polish && language.DisplayName == "Polski");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Turkish && language.DisplayName == "Türkçe");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Romanian && language.DisplayName == "Română");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Czech && language.DisplayName == "Čeština");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Hungarian && language.DisplayName == "Magyar");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Bulgarian && language.DisplayName == "Български");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Italian && language.DisplayName == "Italiano");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Dutch && language.DisplayName == "Nederlands");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Indonesian && language.DisplayName == "Bahasa Indonesia");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Vietnamese && language.DisplayName == "Tiếng Việt");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Hindi && language.DisplayName == "हिन्दी");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == FoldoraLanguage.Thai && language.DisplayName == "ไทย");
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void AvailableLanguages_UsesStableEnglishSortOrder()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var viewModel = new SettingsViewModel(
                new FoldoraSettingsStorage(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"))),
                "en");

            Assert.Equal(
                [
                    FoldoraLanguage.Bulgarian,
                    FoldoraLanguage.SimplifiedChinese,
                    FoldoraLanguage.TraditionalChinese,
                    FoldoraLanguage.Czech,
                    FoldoraLanguage.Dutch,
                    FoldoraLanguage.English,
                    FoldoraLanguage.French,
                    FoldoraLanguage.German,
                    FoldoraLanguage.Hindi,
                    FoldoraLanguage.Hungarian,
                    FoldoraLanguage.Indonesian,
                    FoldoraLanguage.Italian,
                    FoldoraLanguage.Japanese,
                    FoldoraLanguage.Korean,
                    FoldoraLanguage.Polish,
                    FoldoraLanguage.BrazilianPortuguese,
                    FoldoraLanguage.PortuguesePortugal,
                    FoldoraLanguage.Romanian,
                    FoldoraLanguage.Russian,
                    FoldoraLanguage.Spanish,
                    FoldoraLanguage.Thai,
                    FoldoraLanguage.Turkish,
                    FoldoraLanguage.Ukrainian,
                    FoldoraLanguage.Vietnamese
                ],
                viewModel.AvailableLanguages.Select(language => language.Code).ToArray());
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void SelectedLanguage_CanChange()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var viewModel = new SettingsViewModel(
                new FoldoraSettingsStorage(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"))),
                "ru");

            viewModel.SelectedLanguage = "en";

            Assert.Equal("en", viewModel.SelectedLanguage);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_PersistsSelectedLanguage()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var storage = new FoldoraSettingsStorage(paths);
            var viewModel = new SettingsViewModel(storage, "ru")
            {
                SelectedLanguage = "en"
            };

            await viewModel.SaveAsync();

            var settings = await storage.LoadAsync();
            Assert.Equal("en", settings.Language);
            Assert.True(viewModel.Saved);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ExplorerRegister_ReportsDirtyDraftBlockFromSettings()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: false, CreateEntry("entry-documents", "Documents"));
            var storage = new FoldoraSettingsStorage(paths);
            var editor = new FolderMenuDraftEditor(storage, paths);
            await editor.LoadAsync();
            editor.Title = "Draft title";
            var controller = CreateController(storage, editor, new FakeRegistryAccess(), await CreateFakeHostAsync(root.FullName));
            var viewModel = new SettingsViewModel(storage, FoldoraLanguage.English, new InMemoryLocalizationService("en"), controller);

            await viewModel.RegisterExplorerIntegrationAsync();

            Assert.False(viewModel.MenuStateChanged);
            Assert.NotEmpty(viewModel.Errors);
            Assert.Equal("Save or discard menu changes before changing Explorer integration.", viewModel.StatusMessage);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ResetFromSettings_ClearsEntriesAndMarksMenuStateChanged()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, CreateEntry("entry-documents", "Documents"));
            var storage = new FoldoraSettingsStorage(paths);
            var editor = new FolderMenuDraftEditor(storage, paths);
            await editor.LoadAsync();
            var registry = new FakeRegistryAccess();
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryBackgroundRoot);
            var controller = CreateController(storage, editor, registry, await CreateFakeHostAsync(root.FullName), new InMemoryLocalizationService("en"));
            var viewModel = new SettingsViewModel(storage, FoldoraLanguage.English, new InMemoryLocalizationService("en"), controller);

            Assert.False(viewModel.ResetMenuCommand.CanExecute(null));
            viewModel.IsResetConfirmed = true;
            Assert.True(viewModel.ResetMenuCommand.CanExecute(null));

            await viewModel.ResetMenuAsync();

            var settings = await storage.LoadAsync();
            Assert.True(viewModel.MenuStateChanged);
            Assert.False(viewModel.ExplorerIntegrationEnabled);
            Assert.False(viewModel.IsResetConfirmed);
            Assert.Equal("Menu entries reset.", viewModel.StatusMessage);
            Assert.Empty(settings.CreateFolderMenu.Entries);
            Assert.Equal("Create folder", settings.CreateFolderMenu.Title);
            Assert.False(settings.CreateFolderMenu.TitleIsCustom);
            Assert.False(settings.ExplorerIntegrationEnabled);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task InstallationInfo_ExposesUserDataAndCommandHostPath()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var storage = new FoldoraSettingsStorage(paths);
            var hostPath = await CreateFakeHostAsync(root.FullName);
            var viewModel = new SettingsViewModel(
                storage,
                FoldoraLanguage.English,
                new InMemoryLocalizationService("en"),
                commandHostPathResolver: new FixedHostPathResolver(hostPath));

            Assert.Equal(paths.RootDirectory, viewModel.UserDataPath);
            Assert.Equal(hostPath, viewModel.CurrentCommandHostPath);
            Assert.False(string.IsNullOrWhiteSpace(viewModel.InstalledAppPath));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void ExplorerStatus_UsesUserFacingMenuWording()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var viewModel = new SettingsViewModel(
                new FoldoraSettingsStorage(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"))),
                FoldoraLanguage.English,
                new InMemoryLocalizationService("en"));

            Assert.Equal("Foldora Explorer menu: Off", viewModel.ExplorerIntegrationStatusLabel);
            Assert.DoesNotContain("True", viewModel.ExplorerIntegrationStatusLabel, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("False", viewModel.ExplorerIntegrationStatusLabel, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Status:", viewModel.ExplorerIntegrationStatusLabel, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void PathCommands_CallPathActionServiceWithExpectedPaths()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var hostPath = Path.Combine(root.FullName, "Foldora.MenuHost.exe");
            var pathActions = new RecordingPathActionService();
            var viewModel = new SettingsViewModel(
                new FoldoraSettingsStorage(paths),
                FoldoraLanguage.English,
                new InMemoryLocalizationService("en"),
                commandHostPathResolver: new FixedHostPathResolver(hostPath),
                pathActionService: pathActions);

            viewModel.OpenInstalledAppPathCommand.Execute(null);
            viewModel.CopyInstalledAppPathCommand.Execute(null);
            viewModel.OpenUserDataPathCommand.Execute(null);
            viewModel.CopyUserDataPathCommand.Execute(null);
            viewModel.OpenCommandHostPathCommand.Execute(null);
            viewModel.CopyCommandHostPathCommand.Execute(null);

            Assert.Equal(viewModel.InstalledAppPath, pathActions.OpenedFolders[0]);
            Assert.Equal(viewModel.InstalledAppPath, pathActions.CopiedPaths[0]);
            Assert.Equal(paths.RootDirectory, pathActions.OpenedFolders[1]);
            Assert.Equal(paths.RootDirectory, pathActions.CopiedPaths[1]);
            Assert.Equal(hostPath, pathActions.OpenedLocations[0]);
            Assert.Equal(hostPath, pathActions.CopiedPaths[2]);
            Assert.Equal($"Path copied: {hostPath}", viewModel.StatusMessage);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void PathCommandFailure_ReportsLocalizedStatusMessage()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var pathActions = new RecordingPathActionService
            {
                CopyException = new IOException("clipboard unavailable")
            };
            var viewModel = new SettingsViewModel(
                new FoldoraSettingsStorage(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"))),
                FoldoraLanguage.English,
                new InMemoryLocalizationService("en"),
                pathActionService: pathActions);

            viewModel.CopyInstalledAppPathCommand.Execute(null);

            Assert.Equal("Could not copy path: clipboard unavailable", viewModel.StatusMessage);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Theory]
    [MemberData(nameof(EnabledLocales))]
    public async Task SaveAsync_PersistsEveryEnabledLanguage(string language)
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var storage = new FoldoraSettingsStorage(paths);
            var viewModel = new SettingsViewModel(storage, FoldoraLanguage.English)
            {
                SelectedLanguage = language
            };

            await viewModel.SaveAsync();

            var settings = await storage.LoadAsync();
            Assert.Equal(language, settings.Language);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData("ru", "ru")]
    [InlineData("RU", "ru")]
    [InlineData("en", "en")]
    [InlineData("EN", "en")]
    [InlineData("zh-Hans", "zh-Hans")]
    [InlineData("ZH-HANS", "zh-Hans")]
    [InlineData("de", "de")]
    [InlineData("es", "es")]
    [InlineData("fr", "fr")]
    [InlineData("ja", "ja")]
    [InlineData("pt-BR", "pt-BR")]
    [InlineData("PT-br", "pt-BR")]
    [InlineData("ko", "ko")]
    [InlineData("uk", "uk")]
    [InlineData("UK", "uk")]
    [InlineData("pl", "pl")]
    [InlineData("tr", "tr")]
    [InlineData("ro", "ro")]
    [InlineData("cs", "cs")]
    [InlineData("hu", "hu")]
    [InlineData("bg", "bg")]
    [InlineData("it", "it")]
    [InlineData("nl", "nl")]
    [InlineData("id", "id")]
    [InlineData("vi", "vi")]
    [InlineData("hi", "hi")]
    [InlineData("th", "th")]
    [InlineData("zh-Hant", "zh-Hant")]
    [InlineData("ZH-HANT", "zh-Hant")]
    [InlineData("pt-PT", "pt-PT")]
    [InlineData("PT-pt", "pt-PT")]
    [InlineData("be", "en")]
    public void FoldoraLanguage_NormalizesCompleteLocales(string input, string expected)
    {
        Assert.Equal(expected, FoldoraLanguage.NormalizeOrDefault(input));
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

    private static ExplorerIntegrationController CreateController(
        FoldoraSettingsStorage storage,
        FolderMenuDraftEditor editor,
        FakeRegistryAccess registry,
        string hostPath,
        ILocalizationService? localizationService = null)
    {
        var service = new ExplorerMenuRegistrationService(
            storage,
            new ExplorerMenuRegistryPlanBuilder(),
            new ExplorerMenuRegistryWriter(registry));

        return new ExplorerIntegrationController(
            editor,
            service,
            new FixedHostPathResolver(hostPath),
            localizationService ?? new InMemoryLocalizationService("en"));
    }

    private static async Task SaveSettingsAsync(
        FoldoraDataPaths paths,
        bool explorerIntegrationEnabled,
        params FolderMenuEntry[] entries)
    {
        await SaveSettingsAsync(paths, FoldoraLanguage.English, explorerIntegrationEnabled, entries);
    }

    private static async Task SaveSettingsAsync(
        FoldoraDataPaths paths,
        string language,
        bool explorerIntegrationEnabled,
        params FolderMenuEntry[] entries)
    {
        var storage = new FoldoraSettingsStorage(paths);
        var defaultMenu = FolderMenuSettings.CreateDefault(language);
        var menu = new FolderMenuSettings
        {
            Title = defaultMenu.Title,
            TitleIsCustom = defaultMenu.TitleIsCustom,
            Entries = entries.ToList()
        };
        await storage.SaveAsync(new FoldoraSettings
        {
            Language = language,
            ExplorerIntegrationEnabled = explorerIntegrationEnabled,
            CreateFolderMenu = menu
        });
    }

    private static FolderMenuEntry CreateEntry(string id, string displayName)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = displayName,
            DefaultFolderName = displayName,
            IconPath = Path.Combine(Path.GetTempPath(), $"{id}.ico")
        };
    }

    private static async Task<string> CreateFakeHostAsync(string directory)
    {
        var path = Path.Combine(directory, "Foldora.MenuHost.exe");
        await File.WriteAllTextAsync(path, "fake");
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

    private sealed class RecordingPathActionService : IPathActionService
    {
        public List<string> OpenedFolders { get; } = [];

        public List<string> OpenedLocations { get; } = [];

        public List<string> CopiedPaths { get; } = [];

        public Exception? OpenException { get; set; }

        public Exception? CopyException { get; set; }

        public void OpenFolder(string path)
        {
            if (OpenException is not null)
            {
                throw OpenException;
            }

            OpenedFolders.Add(path);
        }

        public void OpenLocation(string path)
        {
            if (OpenException is not null)
            {
                throw OpenException;
            }

            OpenedLocations.Add(path);
        }

        public void CopyPath(string path)
        {
            if (CopyException is not null)
            {
                throw CopyException;
            }

            CopiedPaths.Add(path);
        }
    }
}

using Foldora.App.ViewModels;
using Foldora.Core.Settings;
using Foldora.Core.Storage;

namespace Foldora.Tests.App;

public sealed class SettingsViewModelTests
{
    [Fact]
    public void AvailableLanguages_ContainsRussianAndEnglish()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraSettingsVm-");

        try
        {
            var viewModel = new SettingsViewModel(
                new FoldoraSettingsStorage(new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"))),
                "ru");

            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == "ru");
            Assert.Contains(viewModel.AvailableLanguages, language => language.Code == "en");
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
}

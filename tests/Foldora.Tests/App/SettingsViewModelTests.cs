using Foldora.App.ViewModels;
using Foldora.Core.Settings;
using Foldora.Core.Storage;

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
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Foldora.App.Services;
using Foldora.Core.Settings;

namespace Foldora.App.ViewModels;

/// <summary>
/// ViewModel окна настроек приложения.
/// </summary>
public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private static readonly IReadOnlyDictionary<string, LanguageOption> LanguageOptionsByCode =
        new[]
        {
            new LanguageOption(FoldoraLanguage.Bulgarian, "Български", "Bulgarian", 10),
            new LanguageOption(FoldoraLanguage.SimplifiedChinese, "简体中文", "Chinese Simplified", 20),
            new LanguageOption(FoldoraLanguage.TraditionalChinese, "繁體中文", "Chinese Traditional", 30),
            new LanguageOption(FoldoraLanguage.Czech, "Čeština", "Czech", 40),
            new LanguageOption(FoldoraLanguage.Dutch, "Nederlands", "Dutch", 50),
            new LanguageOption(FoldoraLanguage.English, "English", "English", 60),
            new LanguageOption(FoldoraLanguage.French, "Français", "French", 70),
            new LanguageOption(FoldoraLanguage.German, "Deutsch", "German", 80),
            new LanguageOption(FoldoraLanguage.Hindi, "हिन्दी", "Hindi", 90),
            new LanguageOption(FoldoraLanguage.Hungarian, "Magyar", "Hungarian", 100),
            new LanguageOption(FoldoraLanguage.Indonesian, "Bahasa Indonesia", "Indonesian", 110),
            new LanguageOption(FoldoraLanguage.Italian, "Italiano", "Italian", 120),
            new LanguageOption(FoldoraLanguage.Japanese, "日本語", "Japanese", 130),
            new LanguageOption(FoldoraLanguage.Korean, "한국어", "Korean", 140),
            new LanguageOption(FoldoraLanguage.Polish, "Polski", "Polish", 150),
            new LanguageOption(FoldoraLanguage.BrazilianPortuguese, "Português (Brasil)", "Portuguese Brazil", 160),
            new LanguageOption(FoldoraLanguage.PortuguesePortugal, "Português (Portugal)", "Portuguese Portugal", 170),
            new LanguageOption(FoldoraLanguage.Romanian, "Română", "Romanian", 180),
            new LanguageOption(FoldoraLanguage.Russian, "Русский", "Russian", 190),
            new LanguageOption(FoldoraLanguage.Spanish, "Español", "Spanish", 200),
            new LanguageOption(FoldoraLanguage.Thai, "ไทย", "Thai", 210),
            new LanguageOption(FoldoraLanguage.Turkish, "Türkçe", "Turkish", 220),
            new LanguageOption(FoldoraLanguage.Ukrainian, "Українська", "Ukrainian", 230),
            new LanguageOption(FoldoraLanguage.Vietnamese, "Tiếng Việt", "Vietnamese", 240)
        }.ToDictionary(option => option.Code, StringComparer.Ordinal);

    private readonly FoldoraSettingsStorage storage;
    private readonly ILocalizationService localizationService;
    private string selectedLanguage;
    private string statusMessage = string.Empty;

    public SettingsViewModel(
        FoldoraSettingsStorage storage,
        string currentLanguage,
        ILocalizationService? localizationService = null)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.localizationService = localizationService ?? new InMemoryLocalizationService(currentLanguage);
        selectedLanguage = FoldoraLanguage.NormalizeOrDefault(currentLanguage);
        AvailableLanguages = new ObservableCollection<LanguageOption>(
            FoldoraLanguage.SupportedLocales
                .Select(locale => LanguageOptionsByCode[locale])
                .OrderBy(option => option.SortOrder));
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<LanguageOption> AvailableLanguages { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public string SelectedLanguage
    {
        get => selectedLanguage;
        set
        {
            var normalized = FoldoraLanguage.NormalizeOrDefault(value);
            if (selectedLanguage == normalized)
            {
                return;
            }

            selectedLanguage = normalized;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set
        {
            if (statusMessage == value)
            {
                return;
            }

            statusMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasStatusMessage));
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public bool Saved { get; private set; }

    public LocalizationResources L => localizationService.Resources;

    public async Task SaveAsync()
    {
        var settings = await storage.LoadAsync();
        await storage.SaveAsync(settings with { Language = SelectedLanguage });
        Saved = true;
        localizationService.SetLanguage(SelectedLanguage);
        OnPropertyChanged(nameof(L));
        StatusMessage = L.LanguageSavedRestartNotice;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

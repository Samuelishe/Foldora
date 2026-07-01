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
    private static readonly IReadOnlyDictionary<string, string> NativeLanguageNames = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [FoldoraLanguage.Russian] = "Русский",
        [FoldoraLanguage.English] = "English",
        [FoldoraLanguage.SimplifiedChinese] = "简体中文",
        [FoldoraLanguage.German] = "Deutsch",
        [FoldoraLanguage.Spanish] = "Español",
        [FoldoraLanguage.French] = "Français",
        [FoldoraLanguage.Japanese] = "日本語",
        [FoldoraLanguage.BrazilianPortuguese] = "Português (Brasil)",
        [FoldoraLanguage.Korean] = "한국어"
    };

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
            FoldoraLanguage.SupportedLocales.Select(locale =>
                new LanguageOption(locale, NativeLanguageNames[locale])));
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

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
    private readonly FoldoraSettingsStorage storage;
    private string selectedLanguage;
    private string statusMessage = string.Empty;

    public SettingsViewModel(FoldoraSettingsStorage storage, string currentLanguage)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        selectedLanguage = FoldoraLanguage.NormalizeOrDefault(currentLanguage);
        AvailableLanguages =
        [
            new LanguageOption(FoldoraLanguage.Russian, "Русский"),
            new LanguageOption(FoldoraLanguage.English, "English")
        ];
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

    public async Task SaveAsync()
    {
        var settings = await storage.LoadAsync();
        await storage.SaveAsync(settings with { Language = SelectedLanguage });
        Saved = true;
        StatusMessage = "Настройки сохранены. Некоторые изменения языка могут применяться после перезапуска.";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

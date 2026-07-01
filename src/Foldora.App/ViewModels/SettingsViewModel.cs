using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
    private readonly ExplorerIntegrationController? explorerIntegrationController;
    private readonly IExplorerCommandHostPathResolver? commandHostPathResolver;
    private readonly AsyncRelayCommand dryRunCommand;
    private readonly AsyncRelayCommand registerExplorerCommand;
    private readonly AsyncRelayCommand unregisterExplorerCommand;
    private readonly AsyncRelayCommand resetMenuCommand;
    private string selectedLanguage;
    private string statusMessage = string.Empty;
    private bool explorerIntegrationEnabled;
    private bool isResetConfirmed;
    private bool showTechnicalDetails;

    public SettingsViewModel(
        FoldoraSettingsStorage storage,
        string currentLanguage,
        ILocalizationService? localizationService = null,
        ExplorerIntegrationController? explorerIntegrationController = null,
        IExplorerCommandHostPathResolver? commandHostPathResolver = null)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.localizationService = localizationService ?? new InMemoryLocalizationService(currentLanguage);
        this.explorerIntegrationController = explorerIntegrationController;
        this.commandHostPathResolver = commandHostPathResolver;
        selectedLanguage = FoldoraLanguage.NormalizeOrDefault(currentLanguage);
        explorerIntegrationEnabled = explorerIntegrationController?.ExplorerIntegrationEnabled ?? false;
        AvailableLanguages = new ObservableCollection<LanguageOption>(
            FoldoraLanguage.SupportedLocales
                .Select(locale => LanguageOptionsByCode[locale])
                .OrderBy(option => option.SortOrder));
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        dryRunCommand = new AsyncRelayCommand(DryRunExplorerIntegrationAsync, () => HasExplorerIntegrationController);
        registerExplorerCommand = new AsyncRelayCommand(RegisterExplorerIntegrationAsync, () => HasExplorerIntegrationController);
        unregisterExplorerCommand = new AsyncRelayCommand(UnregisterExplorerIntegrationAsync, () => HasExplorerIntegrationController);
        resetMenuCommand = new AsyncRelayCommand(ResetMenuAsync, () => HasExplorerIntegrationController && IsResetConfirmed);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<LanguageOption> AvailableLanguages { get; }

    public ObservableCollection<string> TechnicalDetails { get; } = [];

    public ObservableCollection<string> Errors { get; } = [];

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand DryRunCommand => dryRunCommand;

    public AsyncRelayCommand RegisterExplorerCommand => registerExplorerCommand;

    public AsyncRelayCommand UnregisterExplorerCommand => unregisterExplorerCommand;

    public AsyncRelayCommand ResetMenuCommand => resetMenuCommand;

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

    public bool MenuStateChanged { get; private set; }

    public bool HasExplorerIntegrationController => explorerIntegrationController is not null;

    public bool ExplorerIntegrationEnabled
    {
        get => explorerIntegrationEnabled;
        private set
        {
            if (explorerIntegrationEnabled == value)
            {
                return;
            }

            explorerIntegrationEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExplorerIntegrationStatusText));
            OnPropertyChanged(nameof(ExplorerIntegrationStatusLabel));
        }
    }

    public string ExplorerIntegrationStatusText => ExplorerIntegrationEnabled ? L.ExplorerEnabled : L.ExplorerDisabled;

    public string ExplorerIntegrationStatusLabel => string.Format(L.StatusLabelFormat, ExplorerIntegrationStatusText);

    public string InstalledAppPath => TrimTrailingDirectorySeparator(AppContext.BaseDirectory);

    public string UserDataPath => storage.Paths.RootDirectory;

    public string CurrentCommandHostPath => ResolveCommandHostPathForDisplay();

    public bool IsResetConfirmed
    {
        get => isResetConfirmed;
        set
        {
            if (isResetConfirmed == value)
            {
                return;
            }

            isResetConfirmed = value;
            OnPropertyChanged();
            resetMenuCommand.RaiseCanExecuteChanged();
        }
    }

    public bool HasTechnicalDetails => TechnicalDetails.Count > 0;

    public bool HasErrors => Errors.Count > 0;

    public bool ShowTechnicalDetails
    {
        get => showTechnicalDetails;
        set
        {
            if (showTechnicalDetails == value)
            {
                return;
            }

            showTechnicalDetails = value;
            OnPropertyChanged();
        }
    }

    public LocalizationResources L => localizationService.Resources;

    public async Task SaveAsync()
    {
        var settings = await storage.LoadAsync();
        await storage.SaveAsync(settings with { Language = SelectedLanguage });
        Saved = true;
        localizationService.SetLanguage(SelectedLanguage);
        OnPropertyChanged(nameof(L));
        OnPropertyChanged(nameof(ExplorerIntegrationStatusText));
        OnPropertyChanged(nameof(ExplorerIntegrationStatusLabel));
        StatusMessage = L.LanguageSavedRestartNotice;
    }

    public async Task DryRunExplorerIntegrationAsync()
    {
        if (explorerIntegrationController is null)
        {
            return;
        }

        ApplyIntegrationResult(await explorerIntegrationController.DryRunAsync());
    }

    public async Task RegisterExplorerIntegrationAsync()
    {
        if (explorerIntegrationController is null)
        {
            return;
        }

        ApplyIntegrationResult(await explorerIntegrationController.RegisterAsync());
    }

    public async Task UnregisterExplorerIntegrationAsync()
    {
        if (explorerIntegrationController is null)
        {
            return;
        }

        ApplyIntegrationResult(await explorerIntegrationController.UnregisterAsync());
    }

    public async Task ResetMenuAsync()
    {
        if (explorerIntegrationController is null)
        {
            return;
        }

        ApplyIntegrationResult(await explorerIntegrationController.ResetMenuAsync());
        IsResetConfirmed = false;
    }

    private void ApplyIntegrationResult(ExplorerIntegrationOperationResult result)
    {
        Errors.Clear();
        TechnicalDetails.Clear();

        foreach (var detail in result.Details)
        {
            TechnicalDetails.Add(detail);
        }

        if (!result.Success)
        {
            Errors.Add(result.Message);
        }

        ExplorerIntegrationEnabled = result.ExplorerIntegrationEnabled;
        StatusMessage = result.Message;
        if (result.Success)
        {
            MenuStateChanged = true;
        }

        OnPropertyChanged(nameof(HasErrors));
        OnPropertyChanged(nameof(HasTechnicalDetails));
    }

    private string ResolveCommandHostPathForDisplay()
    {
        if (commandHostPathResolver is null)
        {
            return string.Empty;
        }

        try
        {
            return commandHostPathResolver.ResolveCommandHostPath();
        }
        catch (Exception exception) when (exception is InvalidOperationException
            or FileNotFoundException
            or DirectoryNotFoundException
            or UnauthorizedAccessException
            or IOException)
        {
            return exception.Message;
        }
    }

    private static string TrimTrailingDirectorySeparator(string path)
    {
        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

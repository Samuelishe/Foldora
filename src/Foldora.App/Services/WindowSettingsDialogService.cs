using System.Windows;
using Foldora.App.ViewModels;
using Foldora.Core.Settings;

namespace Foldora.App.Services;

/// <summary>
/// Открывает WPF-окно настроек.
/// </summary>
public sealed class WindowSettingsDialogService : ISettingsDialogService
{
    private readonly FoldoraSettingsStorage storage;
    private readonly ILocalizationService? localizationService;

    public WindowSettingsDialogService(FoldoraSettingsStorage storage, ILocalizationService? localizationService = null)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.localizationService = localizationService;
    }

    public async Task<SettingsDialogResult> ShowSettingsAsync()
    {
        var currentSettings = await storage.LoadAsync();
        var viewModel = new SettingsViewModel(storage, currentSettings.Language, localizationService);
        var window = new SettingsWindow
        {
            DataContext = viewModel,
            Owner = Application.Current?.MainWindow
        };

        window.ShowDialog();
        return new SettingsDialogResult(viewModel.Saved, viewModel.SelectedLanguage);
    }
}

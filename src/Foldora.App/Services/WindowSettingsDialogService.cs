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
    private readonly ExplorerIntegrationController? explorerIntegrationController;
    private readonly IExplorerCommandHostPathResolver? commandHostPathResolver;

    public WindowSettingsDialogService(
        FoldoraSettingsStorage storage,
        ILocalizationService? localizationService = null,
        ExplorerIntegrationController? explorerIntegrationController = null,
        IExplorerCommandHostPathResolver? commandHostPathResolver = null)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.localizationService = localizationService;
        this.explorerIntegrationController = explorerIntegrationController;
        this.commandHostPathResolver = commandHostPathResolver;
    }

    public async Task<SettingsDialogResult> ShowSettingsAsync()
    {
        var currentSettings = await storage.LoadAsync();
        var viewModel = new SettingsViewModel(
            storage,
            currentSettings.Language,
            localizationService,
            explorerIntegrationController,
            commandHostPathResolver);
        var window = new SettingsWindow
        {
            DataContext = viewModel,
            Owner = Application.Current?.MainWindow
        };

        window.ShowDialog();
        return new SettingsDialogResult(viewModel.Saved, viewModel.SelectedLanguage, viewModel.MenuStateChanged);
    }
}

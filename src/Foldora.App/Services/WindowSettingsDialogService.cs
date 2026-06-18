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

    public WindowSettingsDialogService(FoldoraSettingsStorage storage)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public async Task<SettingsDialogResult> ShowSettingsAsync()
    {
        var currentSettings = await storage.LoadAsync();
        var viewModel = new SettingsViewModel(storage, currentSettings.Language);
        var window = new SettingsWindow
        {
            DataContext = viewModel,
            Owner = Application.Current?.MainWindow
        };

        window.ShowDialog();
        return new SettingsDialogResult(viewModel.Saved, viewModel.SelectedLanguage);
    }
}

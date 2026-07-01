using System.Windows;
using Foldora.App;
using Foldora.App.ViewModels;

namespace Foldora.App.Services;

/// <summary>
/// WPF-реализация диалога справки.
/// </summary>
public sealed class WindowHelpDialogService : IHelpDialogService
{
    private readonly ILocalizationService localizationService;

    public WindowHelpDialogService(ILocalizationService localizationService)
    {
        this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public void ShowHelp()
    {
        var window = new HelpWindow
        {
            DataContext = new HelpWindowViewModel(localizationService)
        };

        var owner = GetOwnerWindow();
        if (owner is not null)
        {
            window.Owner = owner;
        }

        window.ShowDialog();
    }

    private static Window? GetOwnerWindow()
    {
        var application = Application.Current;
        if (application is null)
        {
            return null;
        }

        return application.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
            ?? application.MainWindow;
    }
}

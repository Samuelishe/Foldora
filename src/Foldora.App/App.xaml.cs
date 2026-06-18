using System.Windows;
using System.Windows.Threading;
using Foldora.App.Services;

namespace Foldora.App;

/// <summary>
/// Точка входа WPF-приложения.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        try
        {
            base.OnStartup(e);

            MainWindow = new MainWindow();
            MainWindow.Show();
        }
        catch (Exception exception)
        {
            HandleStartupException(exception);
            Shutdown(1);
        }
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleStartupException(e.Exception);
        e.Handled = true;
        Current.Shutdown(1);
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            StartupDiagnosticsService.WriteStartupError(exception);
        }
    }

    private static void HandleStartupException(Exception exception)
    {
        var logPath = StartupDiagnosticsService.WriteStartupError(exception);
        MessageBox.Show(
            $"Foldora не удалось запустить. Подробности записаны в:{Environment.NewLine}{logPath}",
            "Ошибка запуска Foldora",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}

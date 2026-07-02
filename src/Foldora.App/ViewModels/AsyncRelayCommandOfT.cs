using System.Windows.Input;

namespace Foldora.App.ViewModels;

/// <summary>
/// Простая WPF-команда для асинхронных действий с параметром.
/// </summary>
public sealed class AsyncRelayCommand<T> : ICommand
{
    private readonly Func<T?, Task> executeAsync;
    private readonly Func<T?, bool>? canExecute;
    private bool isRunning;

    public AsyncRelayCommand(Func<T?, Task> executeAsync, Func<T?, bool>? canExecute = null)
    {
        this.executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return !isRunning && (canExecute?.Invoke(ConvertParameter(parameter)) ?? true);
    }

    public async void Execute(object? parameter)
    {
        await ExecuteAsync(ConvertParameter(parameter));
    }

    public async Task ExecuteAsync(T? parameter = default)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        try
        {
            isRunning = true;
            RaiseCanExecuteChanged();
            await executeAsync(parameter);
        }
        finally
        {
            isRunning = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private static T? ConvertParameter(object? parameter)
    {
        return parameter is T typedParameter ? typedParameter : default;
    }
}

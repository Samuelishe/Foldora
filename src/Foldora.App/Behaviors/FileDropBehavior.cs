using System.Windows;
using System.Windows.Input;

namespace Foldora.App.Behaviors;

/// <summary>
/// Минимальный bridge для drop одного или нескольких файлов в команду ViewModel.
/// </summary>
public static class FileDropBehavior
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(FileDropBehavior),
        new PropertyMetadata(null, OnCommandChanged));

    public static ICommand? GetCommand(DependencyObject element)
    {
        return (ICommand?)element.GetValue(CommandProperty);
    }

    public static void SetCommand(DependencyObject element, ICommand? value)
    {
        element.SetValue(CommandProperty, value);
    }

    private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not UIElement element)
        {
            return;
        }

        element.PreviewDragOver -= OnPreviewDragOver;
        element.Drop -= OnDrop;
        element.AllowDrop = args.NewValue is not null;

        if (args.NewValue is null)
        {
            return;
        }

        element.PreviewDragOver += OnPreviewDragOver;
        element.Drop += OnDrop;
    }

    private static void OnPreviewDragOver(object sender, DragEventArgs args)
    {
        args.Effects = args.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        args.Handled = true;
    }

    private static void OnDrop(object sender, DragEventArgs args)
    {
        if (sender is not DependencyObject dependencyObject)
        {
            return;
        }

        var command = GetCommand(dependencyObject);
        if (command is null)
        {
            return;
        }

        var filePaths = args.Data.GetData(DataFormats.FileDrop) as string[] ?? [];
        if (command.CanExecute(filePaths))
        {
            command.Execute(filePaths);
        }

        args.Handled = true;
    }
}

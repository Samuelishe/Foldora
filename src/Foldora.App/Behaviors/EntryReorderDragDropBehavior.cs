using System.Windows;
using System.Windows.Input;

namespace Foldora.App.Behaviors;

/// <summary>
/// Минимальный bridge для внутреннего drag-and-drop переупорядочивания пунктов меню.
/// </summary>
public static class EntryReorderDragDropBehavior
{
    private const string DataFormat = "Foldora.EntryReorder.EntryId";
    private static Point? dragStartPosition;

    public static readonly DependencyProperty DragEntryIdProperty = DependencyProperty.RegisterAttached(
        "DragEntryId",
        typeof(string),
        typeof(EntryReorderDragDropBehavior),
        new PropertyMetadata(null, OnDragEntryIdChanged));

    public static readonly DependencyProperty DropEntryIdProperty = DependencyProperty.RegisterAttached(
        "DropEntryId",
        typeof(string),
        typeof(EntryReorderDragDropBehavior),
        new PropertyMetadata(null, OnDropEntryChanged));

    public static readonly DependencyProperty ReorderCommandProperty = DependencyProperty.RegisterAttached(
        "ReorderCommand",
        typeof(ICommand),
        typeof(EntryReorderDragDropBehavior),
        new PropertyMetadata(null, OnDropEntryChanged));

    public static string? GetDragEntryId(DependencyObject element)
    {
        return (string?)element.GetValue(DragEntryIdProperty);
    }

    public static void SetDragEntryId(DependencyObject element, string? value)
    {
        element.SetValue(DragEntryIdProperty, value);
    }

    public static string? GetDropEntryId(DependencyObject element)
    {
        return (string?)element.GetValue(DropEntryIdProperty);
    }

    public static void SetDropEntryId(DependencyObject element, string? value)
    {
        element.SetValue(DropEntryIdProperty, value);
    }

    public static ICommand? GetReorderCommand(DependencyObject element)
    {
        return (ICommand?)element.GetValue(ReorderCommandProperty);
    }

    public static void SetReorderCommand(DependencyObject element, ICommand? value)
    {
        element.SetValue(ReorderCommandProperty, value);
    }

    private static void OnDragEntryIdChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not UIElement element)
        {
            return;
        }

        element.PreviewMouseMove -= OnPreviewMouseMove;
        element.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        element.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
        if (args.NewValue is not null)
        {
            element.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            element.PreviewMouseMove += OnPreviewMouseMove;
            element.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        }
    }

    private static void OnDropEntryChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not UIElement element)
        {
            return;
        }

        element.PreviewDragOver -= OnPreviewDragOver;
        element.Drop -= OnDrop;

        var hasDropEntry = !string.IsNullOrWhiteSpace(GetDropEntryId(element));
        var hasCommand = GetReorderCommand(element) is not null;
        element.AllowDrop = hasDropEntry && hasCommand;
        if (!element.AllowDrop)
        {
            return;
        }

        element.PreviewDragOver += OnPreviewDragOver;
        element.Drop += OnDrop;
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
        if (sender is IInputElement inputElement)
        {
            dragStartPosition = args.GetPosition(inputElement);
        }
    }

    private static void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
    {
        dragStartPosition = null;
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs args)
    {
        if (sender is not DependencyObject dependencyObject || args.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (sender is not IInputElement inputElement || !HasExceededDragThreshold(args.GetPosition(inputElement)))
        {
            return;
        }

        var entryId = GetDragEntryId(dependencyObject);
        if (string.IsNullOrWhiteSpace(entryId))
        {
            return;
        }

        dragStartPosition = null;
        DragDrop.DoDragDrop((DependencyObject)sender, new DataObject(DataFormat, entryId), DragDropEffects.Move);
        args.Handled = true;
    }

    private static bool HasExceededDragThreshold(Point currentPosition)
    {
        if (!dragStartPosition.HasValue)
        {
            return false;
        }

        var delta = currentPosition - dragStartPosition.Value;
        return Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance
               || Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance;
    }

    private static void OnPreviewDragOver(object sender, DragEventArgs args)
    {
        args.Effects = args.Data.GetDataPresent(DataFormat)
            ? DragDropEffects.Move
            : DragDropEffects.None;
        args.Handled = args.Effects == DragDropEffects.Move;
    }

    private static void OnDrop(object sender, DragEventArgs args)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (args.Data.GetData(DataFormat) is not string sourceEntryId || string.IsNullOrWhiteSpace(sourceEntryId))
        {
            return;
        }

        var targetEntryId = GetDropEntryId(element);
        var command = GetReorderCommand(element);
        if (string.IsNullOrWhiteSpace(targetEntryId) || command is null)
        {
            return;
        }

        var dropPosition = GetDropPosition(element, args);
        var request = new EntryReorderRequest(sourceEntryId, targetEntryId, dropPosition);
        if (command.CanExecute(request))
        {
            command.Execute(request);
        }

        args.Handled = true;
    }

    private static EntryReorderDropPosition GetDropPosition(FrameworkElement element, DragEventArgs args)
    {
        var relativePosition = args.GetPosition(element);
        return relativePosition.Y > element.ActualHeight / 2
            ? EntryReorderDropPosition.After
            : EntryReorderDropPosition.Before;
    }
}

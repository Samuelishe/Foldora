using Microsoft.Win32;

namespace Foldora.App.Services;

/// <summary>
/// WPF file picker для выбора ICO-файла.
/// </summary>
public sealed class WindowsIconFilePicker : IIconFilePicker
{
    public IconFilePickerResult PickIcon()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выбрать .ico",
            Filter = "ICO files (*.ico)|*.ico",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog() == true
            ? IconFilePickerResult.FromPath(dialog.FileName)
            : IconFilePickerResult.Cancelled;
    }
}

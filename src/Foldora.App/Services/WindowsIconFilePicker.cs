using Microsoft.Win32;

namespace Foldora.App.Services;

/// <summary>
/// WPF file picker для выбора ICO-файла или raster image для auto-conversion.
/// </summary>
public sealed class WindowsIconFilePicker : IIconFilePicker
{
    private readonly ILocalizationService localizationService;

    public WindowsIconFilePicker()
        : this(new InMemoryLocalizationService())
    {
    }

    public WindowsIconFilePicker(ILocalizationService localizationService)
    {
        this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public IconFilePickerResult PickIcon()
    {
        var dialog = new OpenFileDialog
        {
            Title = localizationService.Resources.IconPickerTitle,
            Filter = string.Join(
                "|",
                localizationService.Resources.IconPickerFilterIconImages,
                "*.ico;*.png;*.jpg;*.jpeg;*.bmp",
                localizationService.Resources.IconPickerFilterIco,
                "*.ico",
                localizationService.Resources.IconPickerFilterImages,
                "*.png;*.jpg;*.jpeg;*.bmp",
                localizationService.Resources.IconPickerFilterAllFiles,
                "*.*"),
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog() == true
            ? IconFilePickerResult.FromPath(dialog.FileName)
            : IconFilePickerResult.Cancelled;
    }
}

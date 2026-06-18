namespace Foldora.App.Services;

/// <summary>
/// Abstraction над WPF file picker для выбора .ico.
/// </summary>
public interface IIconFilePicker
{
    IconFilePickerResult PickIcon();
}

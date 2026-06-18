namespace Foldora.App.Services;

/// <summary>
/// Результат выбора .ico-файла в WPF.
/// </summary>
public sealed record IconFilePickerResult(bool Selected, string? FilePath)
{
    public static IconFilePickerResult Cancelled { get; } = new(false, null);

    public static IconFilePickerResult FromPath(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        return new IconFilePickerResult(true, filePath);
    }
}

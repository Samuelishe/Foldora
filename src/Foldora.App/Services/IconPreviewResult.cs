using System.Windows.Media;

namespace Foldora.App.Services;

/// <summary>
/// Результат загрузки preview для .ico.
/// </summary>
public sealed record IconPreviewResult(bool HasPreview, ImageSource? Image, string StatusText, string? ErrorMessage)
{
    public static IconPreviewResult NoIcon { get; } = new(false, null, "Иконка не выбрана", null);

    public static IconPreviewResult MissingFile(string iconPath)
    {
        return new IconPreviewResult(false, null, "Иконка не найдена", $"Icon file was not found: {iconPath}");
    }

    public static IconPreviewResult Loaded(ImageSource image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return new IconPreviewResult(true, image, "Иконка: есть", null);
    }

    public static IconPreviewResult Error(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new IconPreviewResult(false, null, "Preview не загружен", message);
    }
}

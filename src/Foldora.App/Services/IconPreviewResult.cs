using System.Windows.Media;

namespace Foldora.App.Services;

/// <summary>
/// Результат загрузки preview для .ico.
/// </summary>
public sealed record IconPreviewResult(bool HasPreview, ImageSource? Image, string StatusText, string? ErrorMessage)
{
    public static IconPreviewResult NoIcon { get; } = new(false, null, new InMemoryLocalizationService().Resources.PreviewNoIcon, null);

    public static IconPreviewResult MissingFile(string iconPath)
    {
        return new IconPreviewResult(false, null, new InMemoryLocalizationService().Resources.PreviewIconMissing, $"Icon file was not found: {iconPath}");
    }

    public static IconPreviewResult Loaded(ImageSource image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return new IconPreviewResult(true, image, new InMemoryLocalizationService().Resources.PreviewIconLoaded, null);
    }

    public static IconPreviewResult Error(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new IconPreviewResult(false, null, new InMemoryLocalizationService().Resources.PreviewNotLoaded, message);
    }
}

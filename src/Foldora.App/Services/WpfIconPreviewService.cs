using System.IO;
using System.Windows.Media.Imaging;

namespace Foldora.App.Services;

/// <summary>
/// WPF decoder для прямого preview .ico.
/// </summary>
public sealed class WpfIconPreviewService : IIconPreviewService
{
    private const int TargetPreviewSize = 50;

    public IconPreviewResult LoadPreview(string? iconPath)
    {
        if (string.IsNullOrWhiteSpace(iconPath))
        {
            return IconPreviewResult.NoIcon;
        }

        if (!File.Exists(iconPath))
        {
            return IconPreviewResult.MissingFile(iconPath);
        }

        try
        {
            using var stream = File.Open(iconPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var decoder = new IconBitmapDecoder(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            var frame = SelectBestFrame(decoder.Frames);
            frame.Freeze();
            return IconPreviewResult.Loaded(frame);
        }
        catch (Exception exception) when (exception is IOException
                                          or UnauthorizedAccessException
                                          or NotSupportedException
                                          or InvalidOperationException)
        {
            return IconPreviewResult.Error($"Icon preview could not be loaded: {exception.Message}");
        }
    }

    private static BitmapFrame SelectBestFrame(IReadOnlyList<BitmapFrame> frames)
    {
        if (frames.Count == 0)
        {
            throw new InvalidOperationException("Icon file does not contain image frames.");
        }

        return frames
            .OrderBy(frame => Math.Abs(frame.PixelWidth - TargetPreviewSize) + Math.Abs(frame.PixelHeight - TargetPreviewSize))
            .ThenBy(frame => Math.Abs(frame.PixelWidth - TargetPreviewSize))
            .First();
    }
}

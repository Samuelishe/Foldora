using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Foldora.Imaging.Windows;

/// <summary>
/// Кодирует project RGBA buffer в PNG payload для будущих ICO frames.
/// </summary>
public sealed class WindowsPngFrameEncoder
{
    public byte[] Encode(RgbaImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        using var output = new MemoryStream();
        Encode(image, output);
        return output.ToArray();
    }

    public void Encode(RgbaImage image, Stream output)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(output);

        if (!output.CanWrite)
        {
            throw new ArgumentException("Output stream must be writable.", nameof(output));
        }

        var bgraPixels = ConvertRgbaToBgra(image.Pixels.Span);
        var bitmap = BitmapSource.Create(
            image.Width,
            image.Height,
            96,
            96,
            PixelFormats.Bgra32,
            palette: null,
            bgraPixels,
            image.Stride);
        bitmap.Freeze();

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(output);
    }

    private static byte[] ConvertRgbaToBgra(ReadOnlySpan<byte> rgbaPixels)
    {
        var bgraPixels = new byte[rgbaPixels.Length];

        for (var offset = 0; offset < rgbaPixels.Length; offset += 4)
        {
            bgraPixels[offset] = rgbaPixels[offset + 2];
            bgraPixels[offset + 1] = rgbaPixels[offset + 1];
            bgraPixels[offset + 2] = rgbaPixels[offset];
            bgraPixels[offset + 3] = rgbaPixels[offset + 3];
        }

        return bgraPixels;
    }
}

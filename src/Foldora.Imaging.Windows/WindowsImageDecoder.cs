using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Foldora.Imaging.Windows;

/// <summary>
/// Декодирует поддерживаемые Windows image streams в project-owned RGBA buffer.
/// </summary>
public sealed class WindowsImageDecoder
{
    public RgbaImage Decode(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!input.CanRead)
        {
            throw new ArgumentException("Input stream must be readable.", nameof(input));
        }

        if (input.CanSeek && input.Length == 0)
        {
            throw new ArgumentException("Input stream must not be empty.", nameof(input));
        }

        try
        {
            var decoder = BitmapDecoder.Create(
                input,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            if (decoder.Frames.Count == 0)
            {
                throw new ArgumentException("Input image does not contain decodable frames.", nameof(input));
            }

            var bitmap = ConvertToBgra32(decoder.Frames[0]);
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var stride = checked(width * 4);
            var bgraPixels = new byte[checked(stride * height)];
            bitmap.CopyPixels(bgraPixels, stride, 0);

            return new RgbaImage(width, height, ConvertBgraToRgba(bgraPixels));
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception exception) when (exception is NotSupportedException or FileFormatException or InvalidOperationException or IOException)
        {
            throw new ArgumentException(
                "Input stream could not be decoded as a supported PNG, JPEG or BMP image.",
                nameof(input),
                exception);
        }
    }

    private static BitmapSource ConvertToBgra32(BitmapSource source)
    {
        if (source.Format == PixelFormats.Bgra32)
        {
            return source;
        }

        var converted = new FormatConvertedBitmap();
        converted.BeginInit();
        converted.Source = source;
        converted.DestinationFormat = PixelFormats.Bgra32;
        converted.EndInit();
        converted.Freeze();

        return converted;
    }

    private static byte[] ConvertBgraToRgba(byte[] bgraPixels)
    {
        var rgbaPixels = new byte[bgraPixels.Length];

        for (var offset = 0; offset < bgraPixels.Length; offset += 4)
        {
            rgbaPixels[offset] = bgraPixels[offset + 2];
            rgbaPixels[offset + 1] = bgraPixels[offset + 1];
            rgbaPixels[offset + 2] = bgraPixels[offset];
            rgbaPixels[offset + 3] = bgraPixels[offset + 3];
        }

        return rgbaPixels;
    }
}

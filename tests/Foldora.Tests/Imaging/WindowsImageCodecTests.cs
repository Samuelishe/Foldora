using System.Windows.Media;
using System.Windows.Media.Imaging;
using Foldora.Imaging;
using Foldora.Imaging.Windows;

namespace Foldora.Tests.Imaging;

public sealed class WindowsImageCodecTests
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    [Fact]
    public void Decode_PngStream_ReturnsRgbaImageAndPreservesAlpha()
    {
        var source = CreateSampleRgbaImage();
        var png = EncodeWithWpf(source, new PngBitmapEncoder());
        var decoder = new WindowsImageDecoder();

        var decoded = decoder.Decode(new MemoryStream(png));

        Assert.Equal(source.Width, decoded.Width);
        Assert.Equal(source.Height, decoded.Height);
        Assert.Equal(source.Pixels.ToArray(), decoded.Pixels.ToArray());
    }

    [Fact]
    public void Decode_JpegStream_ReturnsOpaqueRgbaImage()
    {
        var source = CreateSampleRgbaImage();
        var jpeg = EncodeWithWpf(source, new JpegBitmapEncoder { QualityLevel = 100 }, useAlpha: false);
        var decoder = new WindowsImageDecoder();

        var decoded = decoder.Decode(new MemoryStream(jpeg));

        Assert.Equal(source.Width, decoded.Width);
        Assert.Equal(source.Height, decoded.Height);
        Assert.Equal(source.Width * source.Height * 4, decoded.Pixels.Length);
        Assert.All(ReadAlphaBytes(decoded), alpha => Assert.Equal(0xFF, alpha));
    }

    [Fact]
    public void Decode_BmpStream_ReturnsOpaqueRgbaImage()
    {
        var source = CreateSampleRgbaImage();
        var bmp = EncodeWithWpf(source, new BmpBitmapEncoder(), useAlpha: false);
        var decoder = new WindowsImageDecoder();

        var decoded = decoder.Decode(new MemoryStream(bmp));

        Assert.Equal(source.Width, decoded.Width);
        Assert.Equal(source.Height, decoded.Height);
        Assert.Equal(source.Width * source.Height * 4, decoded.Pixels.Length);
        Assert.All(ReadAlphaBytes(decoded), alpha => Assert.Equal(0xFF, alpha));
    }

    [Fact]
    public void Decode_UnsupportedStream_ThrowsArgumentException()
    {
        var decoder = new WindowsImageDecoder();

        var exception = Assert.Throws<ArgumentException>(
            () => decoder.Decode(new MemoryStream([0x01, 0x02, 0x03, 0x04])));

        Assert.Equal("input", exception.ParamName);
    }

    [Fact]
    public void Decode_EmptyStream_ThrowsArgumentException()
    {
        var decoder = new WindowsImageDecoder();

        var exception = Assert.Throws<ArgumentException>(() => decoder.Decode(new MemoryStream()));

        Assert.Equal("input", exception.ParamName);
    }

    [Fact]
    public void Decode_RejectsNonReadableStream()
    {
        var decoder = new WindowsImageDecoder();
        using var stream = new WriteOnlyStream();

        var exception = Assert.Throws<ArgumentException>(() => decoder.Decode(stream));

        Assert.Equal("input", exception.ParamName);
    }

    [Fact]
    public void Encode_WritesPngSignature()
    {
        var encoder = new WindowsPngFrameEncoder();

        var png = encoder.Encode(CreateSampleRgbaImage());

        Assert.Equal(PngSignature, png.Take(PngSignature.Length).ToArray());
    }

    [Fact]
    public void Encode_PngRoundTripThroughDecoder_PreservesSizeAndAlpha()
    {
        var source = CreateSampleRgbaImage();
        var encoder = new WindowsPngFrameEncoder();
        var decoder = new WindowsImageDecoder();

        var decoded = decoder.Decode(new MemoryStream(encoder.Encode(source)));

        Assert.Equal(source.Width, decoded.Width);
        Assert.Equal(source.Height, decoded.Height);
        Assert.Equal(source.Pixels.ToArray(), decoded.Pixels.ToArray());
    }

    [Fact]
    public void Encode_DoesNotCloseCallerOwnedStream()
    {
        var encoder = new WindowsPngFrameEncoder();
        using var output = new MemoryStream();

        encoder.Encode(CreateSampleRgbaImage(), output);
        output.WriteByte(0xFF);

        Assert.True(output.CanWrite);
        Assert.Equal(0xFF, output.ToArray()[^1]);
    }

    [Fact]
    public void Encode_RejectsNonWritableStream()
    {
        var encoder = new WindowsPngFrameEncoder();
        using var output = new MemoryStream(new byte[128], writable: false);

        var exception = Assert.Throws<ArgumentException>(
            () => encoder.Encode(CreateSampleRgbaImage(), output));

        Assert.Equal("output", exception.ParamName);
    }

    [Fact]
    public void EncodedPngPayload_CanBeUsedByIcoWriter()
    {
        var image = CreateSolidRgbaImage(width: 16, height: 16);
        var pngPayload = new WindowsPngFrameEncoder().Encode(image);
        var ico = new IcoWriter().Write([new IconFrame(new IconFrameSize(16), pngPayload)]);

        Assert.Equal(0, ReadUInt16(ico, 0));
        Assert.Equal(1, ReadUInt16(ico, 2));
        Assert.Equal(1, ReadUInt16(ico, 4));
        Assert.Equal(16, ico[6]);
        Assert.Equal(16, ico[7]);
        Assert.Equal(pngPayload.Length, ReadUInt32(ico, 14));
        Assert.Equal(22, ReadUInt32(ico, 18));
    }

    private static RgbaImage CreateSampleRgbaImage()
    {
        byte[] pixels =
        [
            0xFF, 0x00, 0x00, 0xFF,
            0x00, 0xFF, 0x00, 0x80,
            0x00, 0x00, 0xFF, 0x40,
            0x22, 0x44, 0x66, 0x00
        ];

        return new RgbaImage(width: 2, height: 2, pixels);
    }

    private static RgbaImage CreateSolidRgbaImage(int width, int height)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var offset = 0; offset < pixels.Length; offset += 4)
        {
            pixels[offset] = 0x20;
            pixels[offset + 1] = 0xA0;
            pixels[offset + 2] = 0xF0;
            pixels[offset + 3] = 0xFF;
        }

        return new RgbaImage(width, height, pixels);
    }

    private static byte[] EncodeWithWpf(RgbaImage image, BitmapEncoder encoder, bool useAlpha = true)
    {
        var pixels = image.Pixels.ToArray();
        var pixelFormat = useAlpha ? PixelFormats.Bgra32 : PixelFormats.Bgr24;
        var stride = checked(image.Width * (useAlpha ? 4 : 3));
        var sourcePixels = useAlpha
            ? ConvertRgbaToBgra(pixels)
            : ConvertRgbaToBgr(pixels);
        var bitmap = BitmapSource.Create(
            image.Width,
            image.Height,
            96,
            96,
            pixelFormat,
            palette: null,
            sourcePixels,
            stride);

        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var output = new MemoryStream();
        encoder.Save(output);
        return output.ToArray();
    }

    private static byte[] ConvertRgbaToBgra(byte[] rgbaPixels)
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

    private static byte[] ConvertRgbaToBgr(byte[] rgbaPixels)
    {
        var bgrPixels = new byte[rgbaPixels.Length / 4 * 3];

        for (int sourceOffset = 0, targetOffset = 0; sourceOffset < rgbaPixels.Length; sourceOffset += 4, targetOffset += 3)
        {
            bgrPixels[targetOffset] = rgbaPixels[sourceOffset + 2];
            bgrPixels[targetOffset + 1] = rgbaPixels[sourceOffset + 1];
            bgrPixels[targetOffset + 2] = rgbaPixels[sourceOffset];
        }

        return bgrPixels;
    }

    private static IEnumerable<byte> ReadAlphaBytes(RgbaImage image)
    {
        var pixels = image.Pixels.ToArray();
        for (var offset = 3; offset < pixels.Length; offset += 4)
        {
            yield return pixels[offset];
        }
    }

    private static ushort ReadUInt16(byte[] data, int offset)
    {
        return (ushort)(data[offset] | (data[offset + 1] << 8));
    }

    private static int ReadUInt32(byte[] data, int offset)
    {
        return data[offset]
               | (data[offset + 1] << 8)
               | (data[offset + 2] << 16)
               | (data[offset + 3] << 24);
    }

    private sealed class WriteOnlyStream : MemoryStream
    {
        public override bool CanRead => false;

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override int Read(Span<byte> buffer)
        {
            throw new NotSupportedException();
        }
    }
}

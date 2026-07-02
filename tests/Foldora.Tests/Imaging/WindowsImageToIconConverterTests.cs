using System.Windows.Media;
using System.Windows.Media.Imaging;
using Foldora.Imaging;
using Foldora.Imaging.Windows;

namespace Foldora.Tests.Imaging;

public sealed class WindowsImageToIconConverterTests
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    [Fact]
    public void Convert_PngStream_WritesStandardMultiSizeIco()
    {
        var source = CreateCheckerImage(width: 8, height: 8);
        var input = new MemoryStream(new WindowsPngFrameEncoder().Encode(source));
        using var output = new MemoryStream();
        var converter = new WindowsImageToIconConverter();

        var result = converter.Convert(input, output);

        var ico = output.ToArray();
        Assert.True(result.Succeeded);
        Assert.Equal(8, result.SourceWidth);
        Assert.Equal(8, result.SourceHeight);
        Assert.Equal([16, 24, 32, 48, 64, 128, 256], result.GeneratedFrameSizes.Select(size => size.Size).ToArray());
        AssertIconDir(ico, expectedCount: 7);
        var entries = ReadDirectoryEntries(ico);
        Assert.Equal([16, 24, 32, 48, 64, 128, 256], entries.Select(entry => entry.Size).ToArray());
        AssertPayloadsArePng(ico, entries);
        AssertPayloadOffsetsAreValid(ico, entries);
    }

    [Fact]
    public void Convert_JpegStream_WritesStandardMultiSizeIco()
    {
        var source = CreateCheckerImage(width: 8, height: 8);
        var input = new MemoryStream(EncodeWithWpf(source, new JpegBitmapEncoder { QualityLevel = 100 }, useAlpha: false));
        using var output = new MemoryStream();

        new WindowsImageToIconConverter().Convert(input, output);

        var entries = ReadDirectoryEntries(output.ToArray());
        Assert.Equal([16, 24, 32, 48, 64, 128, 256], entries.Select(entry => entry.Size).ToArray());
    }

    [Fact]
    public void Convert_BmpStream_WritesStandardMultiSizeIco()
    {
        var source = CreateCheckerImage(width: 8, height: 8);
        var input = new MemoryStream(EncodeWithWpf(source, new BmpBitmapEncoder(), useAlpha: false));
        using var output = new MemoryStream();

        new WindowsImageToIconConverter().Convert(input, output);

        var entries = ReadDirectoryEntries(output.ToArray());
        Assert.Equal([16, 24, 32, 48, 64, 128, 256], entries.Select(entry => entry.Size).ToArray());
    }

    [Fact]
    public void Convert_UsesCustomTargetFrameSizesSortedAscending()
    {
        var source = CreateCheckerImage(width: 8, height: 8);
        var input = new MemoryStream(new WindowsPngFrameEncoder().Encode(source));
        using var output = new MemoryStream();
        var options = new IconConversionOptions(
        [
            new IconFrameSize(64),
            new IconFrameSize(16),
            new IconFrameSize(32)
        ]);

        var result = new WindowsImageToIconConverter().Convert(input, output, options);

        Assert.Equal([16, 32, 64], result.GeneratedFrameSizes.Select(size => size.Size).ToArray());
        Assert.Equal([16, 32, 64], ReadDirectoryEntries(output.ToArray()).Select(entry => entry.Size).ToArray());
    }

    [Fact]
    public void Options_RejectEmptyTargetFrameSizes()
    {
        var exception = Assert.Throws<ArgumentException>(() => new IconConversionOptions([]));

        Assert.Equal("frameSizes", exception.ParamName);
    }

    [Fact]
    public void Options_RejectDuplicateTargetFrameSizes()
    {
        var exception = Assert.Throws<ArgumentException>(() => new IconConversionOptions(
        [
            new IconFrameSize(16),
            new IconFrameSize(16)
        ]));

        Assert.Equal("frameSizes", exception.ParamName);
    }

    [Fact]
    public void Convert_RejectsNullInputStream()
    {
        using var output = new MemoryStream();

        Assert.Throws<ArgumentNullException>(() => new WindowsImageToIconConverter().Convert(null!, output));
    }

    [Fact]
    public void Convert_RejectsUnreadableInputStream()
    {
        using var input = new WriteOnlyStream();
        using var output = new MemoryStream();

        var exception = Assert.Throws<ArgumentException>(() => new WindowsImageToIconConverter().Convert(input, output));

        Assert.Equal("input", exception.ParamName);
    }

    [Fact]
    public void Convert_RejectsNullOutputStream()
    {
        using var input = new MemoryStream(new WindowsPngFrameEncoder().Encode(CreateCheckerImage(width: 4, height: 4)));

        Assert.Throws<ArgumentNullException>(() => new WindowsImageToIconConverter().Convert(input, null!));
    }

    [Fact]
    public void Convert_RejectsNonWritableOutputStream()
    {
        using var input = new MemoryStream(new WindowsPngFrameEncoder().Encode(CreateCheckerImage(width: 4, height: 4)));
        using var output = new MemoryStream(new byte[128], writable: false);

        var exception = Assert.Throws<ArgumentException>(() => new WindowsImageToIconConverter().Convert(input, output));

        Assert.Equal("output", exception.ParamName);
    }

    [Fact]
    public void Convert_DoesNotCloseCallerOwnedStreams()
    {
        using var input = new MemoryStream(new WindowsPngFrameEncoder().Encode(CreateCheckerImage(width: 4, height: 4)));
        using var output = new MemoryStream();

        new WindowsImageToIconConverter().Convert(input, output);
        output.WriteByte(0xFF);

        Assert.True(input.CanRead);
        Assert.True(output.CanWrite);
        Assert.Equal(0xFF, output.ToArray()[^1]);
    }

    [Fact]
    public void Convert_ResultReportsSourceDimensionsAndGeneratedFrameSizes()
    {
        var source = CreateCheckerImage(width: 9, height: 5);
        using var input = new MemoryStream(new WindowsPngFrameEncoder().Encode(source));
        using var output = new MemoryStream();
        var options = new IconConversionOptions([new IconFrameSize(16), new IconFrameSize(32)]);

        var result = new WindowsImageToIconConverter().Convert(input, output, options);

        Assert.True(result.Succeeded);
        Assert.Equal(9, result.SourceWidth);
        Assert.Equal(5, result.SourceHeight);
        Assert.Equal([16, 32], result.GeneratedFrameSizes.Select(size => size.Size).ToArray());
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Convert_NonSquareSource_UsesContainFitTransparentSquarePolicy()
    {
        var source = CreateSolidImage(width: 4, height: 2, red: 0, green: 220, blue: 40, alpha: 255);
        using var input = new MemoryStream(new WindowsPngFrameEncoder().Encode(source));
        using var output = new MemoryStream();
        var options = new IconConversionOptions([new IconFrameSize(32)]);

        new WindowsImageToIconConverter().Convert(input, output, options);

        var entry = Assert.Single(ReadDirectoryEntries(output.ToArray()));
        var framePayload = ExtractPayload(output.ToArray(), entry);
        var decodedFrame = new WindowsImageDecoder().Decode(new MemoryStream(framePayload));
        var pixels = decodedFrame.Pixels.ToArray();

        Assert.Equal(32, decodedFrame.Width);
        Assert.Equal(32, decodedFrame.Height);
        Assert.Equal(0, ReadAlpha(pixels, decodedFrame.Width, x: 16, y: 0));
        Assert.Equal(0, ReadAlpha(pixels, decodedFrame.Width, x: 16, y: 31));
        Assert.Equal(255, ReadAlpha(pixels, decodedFrame.Width, x: 16, y: 16));
        Assert.True(ReadGreen(pixels, decodedFrame.Width, x: 16, y: 16) > 180);
    }

    [Fact]
    public void Convert_TinySource_UpscalesToStandardSizes()
    {
        using var input = new MemoryStream(new WindowsPngFrameEncoder().Encode(CreateSolidImage(width: 1, height: 1, red: 20, green: 40, blue: 60, alpha: 255)));
        using var output = new MemoryStream();

        new WindowsImageToIconConverter().Convert(input, output);

        Assert.Equal([16, 24, 32, 48, 64, 128, 256], ReadDirectoryEntries(output.ToArray()).Select(entry => entry.Size).ToArray());
    }

    [Fact]
    public void Convert_PngAlpha_SurvivesGeneratedFrame()
    {
        byte[] pixels =
        [
            255, 0, 0, 0,
            0, 0, 255, 255,
            0, 255, 0, 128,
            255, 255, 255, 255
        ];
        using var input = new MemoryStream(new WindowsPngFrameEncoder().Encode(new RgbaImage(width: 2, height: 2, pixels)));
        using var output = new MemoryStream();
        var options = new IconConversionOptions([new IconFrameSize(16)]);

        new WindowsImageToIconConverter().Convert(input, output, options);

        var entry = Assert.Single(ReadDirectoryEntries(output.ToArray()));
        var decodedFrame = new WindowsImageDecoder().Decode(new MemoryStream(ExtractPayload(output.ToArray(), entry)));
        var alphaValues = ReadAlphaBytes(decodedFrame).Distinct().ToArray();

        Assert.Contains(alphaValues, alpha => alpha == 0);
        Assert.Contains(alphaValues, alpha => alpha is > 0 and < 255);
        Assert.Contains(alphaValues, alpha => alpha == 255);
    }

    [Fact]
    public void Convert_CorruptStream_ThrowsArgumentException()
    {
        using var input = new MemoryStream([0x01, 0x02, 0x03, 0x04]);
        using var output = new MemoryStream();

        var exception = Assert.Throws<ArgumentException>(() => new WindowsImageToIconConverter().Convert(input, output));

        Assert.Equal("input", exception.ParamName);
    }

    private static RgbaImage CreateCheckerImage(int width, int height)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                var bright = (x + y) % 2 == 0;
                pixels[offset] = bright ? (byte)240 : (byte)20;
                pixels[offset + 1] = bright ? (byte)240 : (byte)80;
                pixels[offset + 2] = bright ? (byte)255 : (byte)160;
                pixels[offset + 3] = 255;
            }
        }

        return new RgbaImage(width, height, pixels);
    }

    private static RgbaImage CreateSolidImage(int width, int height, byte red, byte green, byte blue, byte alpha)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var offset = 0; offset < pixels.Length; offset += 4)
        {
            pixels[offset] = red;
            pixels[offset + 1] = green;
            pixels[offset + 2] = blue;
            pixels[offset + 3] = alpha;
        }

        return new RgbaImage(width, height, pixels);
    }

    private static byte[] EncodeWithWpf(RgbaImage image, BitmapEncoder encoder, bool useAlpha)
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

    private static void AssertIconDir(byte[] ico, int expectedCount)
    {
        Assert.Equal(0, ReadUInt16(ico, 0));
        Assert.Equal(1, ReadUInt16(ico, 2));
        Assert.Equal(expectedCount, ReadUInt16(ico, 4));
    }

    private static IcoDirectoryEntry[] ReadDirectoryEntries(byte[] ico)
    {
        var count = ReadUInt16(ico, 4);
        var entries = new IcoDirectoryEntry[count];

        for (var index = 0; index < count; index++)
        {
            var offset = 6 + index * 16;
            var size = ico[offset] == 0 ? 256 : ico[offset];
            entries[index] = new IcoDirectoryEntry(
                size,
                ReadUInt32(ico, offset + 8),
                ReadUInt32(ico, offset + 12));
        }

        return entries;
    }

    private static void AssertPayloadsArePng(byte[] ico, IReadOnlyList<IcoDirectoryEntry> entries)
    {
        foreach (var entry in entries)
        {
            var payload = ExtractPayload(ico, entry);
            Assert.Equal(PngSignature, payload.Take(PngSignature.Length).ToArray());
        }
    }

    private static void AssertPayloadOffsetsAreValid(byte[] ico, IReadOnlyList<IcoDirectoryEntry> entries)
    {
        var expectedOffset = 6 + entries.Count * 16;
        foreach (var entry in entries)
        {
            Assert.Equal(expectedOffset, entry.Offset);
            Assert.InRange(entry.BytesInResource, PngSignature.Length, ico.Length - entry.Offset);
            expectedOffset += entry.BytesInResource;
        }

        Assert.Equal(ico.Length, expectedOffset);
    }

    private static byte[] ExtractPayload(byte[] ico, IcoDirectoryEntry entry)
    {
        return ico.Skip(entry.Offset).Take(entry.BytesInResource).ToArray();
    }

    private static IEnumerable<byte> ReadAlphaBytes(RgbaImage image)
    {
        var pixels = image.Pixels.ToArray();
        for (var offset = 3; offset < pixels.Length; offset += 4)
        {
            yield return pixels[offset];
        }
    }

    private static byte ReadAlpha(byte[] pixels, int width, int x, int y)
    {
        return pixels[(y * width + x) * 4 + 3];
    }

    private static byte ReadGreen(byte[] pixels, int width, int x, int y)
    {
        return pixels[(y * width + x) * 4 + 1];
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

    private readonly record struct IcoDirectoryEntry(int Size, int BytesInResource, int Offset);
}

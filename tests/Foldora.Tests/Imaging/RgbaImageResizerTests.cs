using Foldora.Imaging;
using Foldora.Imaging.Windows;

namespace Foldora.Tests.Imaging;

public sealed class RgbaImageResizerTests
{
    [Fact]
    public void Resize_RejectsNullSource()
    {
        var resizer = new RgbaImageResizer();

        Assert.Throws<ArgumentNullException>(() => resizer.Resize(null!, targetWidth: 16, targetHeight: 16));
    }

    [Theory]
    [InlineData(0, 16, "targetWidth")]
    [InlineData(-1, 16, "targetWidth")]
    [InlineData(4097, 16, "targetWidth")]
    [InlineData(16, 0, "targetHeight")]
    [InlineData(16, -1, "targetHeight")]
    [InlineData(16, 4097, "targetHeight")]
    public void Resize_RejectsInvalidTargetDimensions(int targetWidth, int targetHeight, string expectedParameter)
    {
        var resizer = new RgbaImageResizer();
        var source = CreateSolidImage(width: 2, height: 2, red: 10, green: 20, blue: 30, alpha: 255);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => resizer.Resize(source, targetWidth, targetHeight));

        Assert.Equal(expectedParameter, exception.ParamName);
    }

    [Fact]
    public void Resize_Downscale_ReturnsRequestedDimensions()
    {
        var source = CreateGradientImage(width: 4, height: 4);
        var resizer = new RgbaImageResizer();

        var resized = resizer.Resize(source, targetWidth: 2, targetHeight: 2);

        Assert.Equal(2, resized.Width);
        Assert.Equal(2, resized.Height);
        Assert.Equal(2 * 2 * 4, resized.Pixels.Length);
    }

    [Fact]
    public void Resize_Upscale_ReturnsRequestedDimensions()
    {
        var source = CreateGradientImage(width: 2, height: 2);
        var resizer = new RgbaImageResizer();

        var resized = resizer.Resize(source, targetWidth: 4, targetHeight: 4);

        Assert.Equal(4, resized.Width);
        Assert.Equal(4, resized.Height);
        Assert.Equal(4 * 4 * 4, resized.Pixels.Length);
    }

    [Fact]
    public void Resize_OpaqueSourceKeepsOpaqueAlpha()
    {
        var source = CreateGradientImage(width: 5, height: 5, alpha: 255);
        var resizer = new RgbaImageResizer();

        var resized = resizer.Resize(source, targetWidth: 3, targetHeight: 7);

        Assert.All(ReadAlphaBytes(resized), alpha => Assert.Equal(255, alpha));
    }

    [Fact]
    public void Resize_ConstantColorStaysConstantAfterDownscale()
    {
        var source = CreateSolidImage(width: 9, height: 7, red: 24, green: 96, blue: 168, alpha: 255);
        var resizer = new RgbaImageResizer();

        var resized = resizer.Resize(source, targetWidth: 3, targetHeight: 2);

        Assert.All(ReadPixels(resized), pixel =>
        {
            AssertInRange(pixel.Red, 24, tolerance: 1);
            AssertInRange(pixel.Green, 96, tolerance: 1);
            AssertInRange(pixel.Blue, 168, tolerance: 1);
            AssertInRange(pixel.Alpha, 255, tolerance: 0);
        });
    }

    [Fact]
    public void Resize_TransparentRgbDoesNotBleedIntoTransparentOutput()
    {
        byte[] pixels =
        [
            255, 0, 0, 0,
            255, 0, 0, 0,
            255, 0, 0, 0,
            255, 0, 0, 0
        ];
        var source = new RgbaImage(width: 2, height: 2, pixels);
        var resizer = new RgbaImageResizer();

        var resized = resizer.Resize(source, targetWidth: 1, targetHeight: 1);
        var output = resized.Pixels.ToArray();

        Assert.Equal([0, 0, 0, 0], output);
    }

    [Fact]
    public void Resize_TransparentRedPixelsDoNotCreateRedHalo()
    {
        byte[] pixels =
        [
            255, 0, 0, 0,
            255, 0, 0, 0,
            255, 0, 0, 0,
            0, 0, 255, 255
        ];
        var source = new RgbaImage(width: 2, height: 2, pixels);
        var resizer = new RgbaImageResizer();

        var resized = resizer.Resize(source, targetWidth: 1, targetHeight: 1);
        var output = resized.Pixels.ToArray();

        Assert.True(output[3] > 0);
        Assert.True(output[0] < 8, $"Expected red halo to be suppressed, got R={output[0]}.");
        Assert.True(output[2] > 200, $"Expected visible blue contribution, got B={output[2]}.");
    }

    [Fact]
    public void Resize_SemiTransparentAlphaSurvives()
    {
        byte[] pixels =
        [
            20, 40, 60, 64,
            20, 40, 60, 128,
            20, 40, 60, 192,
            20, 40, 60, 255
        ];
        var source = new RgbaImage(width: 2, height: 2, pixels);
        var resizer = new RgbaImageResizer();

        var resized = resizer.Resize(source, targetWidth: 4, targetHeight: 4);
        var alphaValues = ReadAlphaBytes(resized).Distinct().ToArray();

        Assert.Contains(alphaValues, alpha => alpha is > 0 and < 255);
        Assert.True(alphaValues.Length > 2);
    }

    [Fact]
    public void Resize_IsDeterministic()
    {
        var source = CreateGradientImage(width: 7, height: 5);
        var resizer = new RgbaImageResizer();

        var first = resizer.Resize(source, targetWidth: 3, targetHeight: 4);
        var second = resizer.Resize(source, targetWidth: 3, targetHeight: 4);

        Assert.Equal(first.Pixels.ToArray(), second.Pixels.ToArray());
    }

    [Fact]
    public void Resize_OnePixelSourceUpscales()
    {
        var source = CreateSolidImage(width: 1, height: 1, red: 9, green: 18, blue: 27, alpha: 255);
        var resizer = new RgbaImageResizer();

        var resized = resizer.Resize(source, targetWidth: 16, targetHeight: 16);

        Assert.Equal(16, resized.Width);
        Assert.Equal(16, resized.Height);
        Assert.All(ReadPixels(resized), pixel =>
        {
            Assert.Equal(9, pixel.Red);
            Assert.Equal(18, pixel.Green);
            Assert.Equal(27, pixel.Blue);
            Assert.Equal(255, pixel.Alpha);
        });
    }

    [Fact]
    public void Resize_ToOnePixelDownscales()
    {
        var source = CreateGradientImage(width: 16, height: 16);
        var resizer = new RgbaImageResizer();

        var resized = resizer.Resize(source, targetWidth: 1, targetHeight: 1);

        Assert.Equal(1, resized.Width);
        Assert.Equal(1, resized.Height);
        Assert.Single(ReadPixels(resized));
    }

    [Fact]
    public void Resize_NonSquareTargetWorks()
    {
        var source = CreateGradientImage(width: 4, height: 2);
        var resizer = new RgbaImageResizer();

        var resized = resizer.Resize(source, targetWidth: 2, targetHeight: 4);

        Assert.Equal(2, resized.Width);
        Assert.Equal(4, resized.Height);
        Assert.Equal(2 * 4 * 4, resized.Pixels.Length);
    }

    [Fact]
    public void Resize_DoesNotMutateSourcePixels()
    {
        var source = CreateGradientImage(width: 4, height: 4);
        var before = source.Pixels.ToArray();
        var resizer = new RgbaImageResizer();

        _ = resizer.Resize(source, targetWidth: 2, targetHeight: 2);

        Assert.Equal(before, source.Pixels.ToArray());
    }

    [Fact]
    public void ResizedImage_CanBeEncodedAsPngPayloadAndWrittenIntoIco()
    {
        var source = CreateGradientImage(width: 32, height: 32);
        var resized = new RgbaImageResizer().Resize(source, targetWidth: 16, targetHeight: 16);
        var pngPayload = new WindowsPngFrameEncoder().Encode(resized);
        var ico = new IcoWriter().Write([new IconFrame(new IconFrameSize(16), pngPayload)]);

        Assert.Equal(0, ReadUInt16(ico, 0));
        Assert.Equal(1, ReadUInt16(ico, 2));
        Assert.Equal(1, ReadUInt16(ico, 4));
        Assert.Equal(16, ico[6]);
        Assert.Equal(16, ico[7]);
        Assert.Equal(pngPayload.Length, ReadUInt32(ico, 14));
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

    private static RgbaImage CreateGradientImage(int width, int height, byte alpha = 255)
    {
        var pixels = new byte[checked(width * height * 4)];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                pixels[offset] = (byte)(x * 17 + y * 3);
                pixels[offset + 1] = (byte)(x * 5 + y * 19);
                pixels[offset + 2] = (byte)(x * 11 + y * 7);
                pixels[offset + 3] = alpha;
            }
        }

        return new RgbaImage(width, height, pixels);
    }

    private static IEnumerable<RgbaPixel> ReadPixels(RgbaImage image)
    {
        var pixels = image.Pixels.ToArray();
        for (var offset = 0; offset < pixels.Length; offset += 4)
        {
            yield return new RgbaPixel(
                pixels[offset],
                pixels[offset + 1],
                pixels[offset + 2],
                pixels[offset + 3]);
        }
    }

    private static IEnumerable<byte> ReadAlphaBytes(RgbaImage image)
    {
        return ReadPixels(image).Select(pixel => pixel.Alpha);
    }

    private static void AssertInRange(byte actual, byte expected, int tolerance)
    {
        Assert.InRange(actual, expected - tolerance, expected + tolerance);
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

    private readonly record struct RgbaPixel(byte Red, byte Green, byte Blue, byte Alpha);
}

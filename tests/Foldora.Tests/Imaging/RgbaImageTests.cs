using Foldora.Imaging;

namespace Foldora.Tests.Imaging;

public sealed class RgbaImageTests
{
    [Fact]
    public void Constructor_AcceptsTightlyPackedRgbaBuffer()
    {
        byte[] pixels =
        [
            0x01, 0x02, 0x03, 0x04,
            0x05, 0x06, 0x07, 0x08
        ];

        var image = new RgbaImage(width: 2, height: 1, pixels);

        Assert.Equal(2, image.Width);
        Assert.Equal(1, image.Height);
        Assert.Equal(8, image.Stride);
        Assert.Equal(pixels, image.Pixels.ToArray());
    }

    [Fact]
    public void Constructor_CopiesCallerOwnedBuffer()
    {
        byte[] pixels =
        [
            0x11, 0x22, 0x33, 0x44
        ];

        var image = new RgbaImage(width: 1, height: 1, pixels);
        pixels[0] = 0xFF;

        Assert.Equal(0x11, image.Pixels.ToArray()[0]);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    public void Constructor_RejectsInvalidWidth(int width, int height)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new RgbaImage(width, height, [0x00, 0x00, 0x00, 0xFF]));

        Assert.Equal("width", exception.ParamName);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void Constructor_RejectsInvalidHeight(int width, int height)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new RgbaImage(width, height, [0x00, 0x00, 0x00, 0xFF]));

        Assert.Equal("height", exception.ParamName);
    }

    [Fact]
    public void Constructor_RejectsNullPixels()
    {
        Assert.Throws<ArgumentNullException>(() => new RgbaImage(width: 1, height: 1, null!));
    }

    [Fact]
    public void Constructor_RejectsWrongBufferLength()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new RgbaImage(width: 2, height: 2, [0x00, 0x00, 0x00, 0xFF]));

        Assert.Equal("pixels", exception.ParamName);
    }

    [Fact]
    public void Pixels_AreDocumentedAsRgbaOrder()
    {
        var image = new RgbaImage(
            width: 1,
            height: 1,
            pixels: [0x10, 0x20, 0x30, 0x40]);

        var pixel = image.Pixels.ToArray();
        Assert.Equal(0x10, pixel[0]);
        Assert.Equal(0x20, pixel[1]);
        Assert.Equal(0x30, pixel[2]);
        Assert.Equal(0x40, pixel[3]);
    }
}

namespace Foldora.Imaging;

/// <summary>
/// Плотно упакованное RGBA-изображение без привязки к WPF или Windows imaging types.
/// </summary>
public sealed class RgbaImage
{
    private const int BytesPerPixel = 4;
    private readonly byte[] _pixels;

    public RgbaImage(int width, int height, byte[] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels);

        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Image width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Image height must be positive.");
        }

        var expectedLength = checked(width * height * BytesPerPixel);
        if (pixels.Length != expectedLength)
        {
            throw new ArgumentException(
                $"RGBA pixel buffer length must be exactly width * height * 4 bytes ({expectedLength}).",
                nameof(pixels));
        }

        Width = width;
        Height = height;
        _pixels = pixels.ToArray();
    }

    public int Width { get; }

    public int Height { get; }

    public int Stride => Width * BytesPerPixel;

    /// <summary>
    /// Пиксели в порядке RGBA: R offset +0, G offset +1, B offset +2, A offset +3.
    /// </summary>
    public ReadOnlyMemory<byte> Pixels => _pixels;
}

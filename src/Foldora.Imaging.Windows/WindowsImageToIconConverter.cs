using System.IO;

namespace Foldora.Imaging.Windows;

/// <summary>
/// Windows-specific pipeline для конвертации raster image stream в multi-size ICO.
/// </summary>
public sealed class WindowsImageToIconConverter
{
    private const int BytesPerPixel = 4;
    private readonly WindowsImageDecoder _decoder;
    private readonly WindowsPngFrameEncoder _pngFrameEncoder;
    private readonly IcoWriter _icoWriter;

    public WindowsImageToIconConverter()
        : this(new WindowsImageDecoder(), new WindowsPngFrameEncoder(), new IcoWriter())
    {
    }

    public WindowsImageToIconConverter(
        WindowsImageDecoder decoder,
        WindowsPngFrameEncoder pngFrameEncoder,
        IcoWriter icoWriter)
    {
        _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
        _pngFrameEncoder = pngFrameEncoder ?? throw new ArgumentNullException(nameof(pngFrameEncoder));
        _icoWriter = icoWriter ?? throw new ArgumentNullException(nameof(icoWriter));
    }

    public IconConversionResult Convert(Stream input, Stream output, IconConversionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        if (!input.CanRead)
        {
            throw new ArgumentException("Input stream must be readable.", nameof(input));
        }

        if (!output.CanWrite)
        {
            throw new ArgumentException("Output stream must be writable.", nameof(output));
        }

        var conversionOptions = options ?? IconConversionOptions.Default;
        if (conversionOptions.FitMode != IconImageFitMode.Contain)
        {
            throw new ArgumentOutOfRangeException(nameof(options), conversionOptions.FitMode, "Unsupported icon image fit mode.");
        }

        var source = _decoder.Decode(input);
        var resizer = new RgbaImageResizer(conversionOptions.ResizeOptions);
        var frames = new List<IconFrame>(conversionOptions.TargetFrameSizes.Count);

        foreach (var frameSize in conversionOptions.TargetFrameSizes)
        {
            var frameImage = CreateFrameImage(source, frameSize.Size, resizer, conversionOptions.FitMode);
            var payload = _pngFrameEncoder.Encode(frameImage);
            frames.Add(new IconFrame(frameSize, payload));
        }

        _icoWriter.Write(frames, output);

        return IconConversionResult.Success(
            conversionOptions.TargetFrameSizes,
            sourceWidth: source.Width,
            sourceHeight: source.Height);
    }

    private static RgbaImage CreateFrameImage(
        RgbaImage source,
        int size,
        RgbaImageResizer resizer,
        IconImageFitMode fitMode)
    {
        return fitMode switch
        {
            IconImageFitMode.Contain => CreateContainFrameImage(source, size, resizer),
            _ => throw new ArgumentOutOfRangeException(nameof(fitMode), fitMode, "Unsupported icon image fit mode.")
        };
    }

    private static RgbaImage CreateContainFrameImage(RgbaImage source, int size, RgbaImageResizer resizer)
    {
        if (source.Width == source.Height)
        {
            return resizer.Resize(source, size, size);
        }

        var scale = Math.Min((double)size / source.Width, (double)size / source.Height);
        var contentWidth = Math.Clamp((int)Math.Round(source.Width * scale, MidpointRounding.AwayFromZero), 1, size);
        var contentHeight = Math.Clamp((int)Math.Round(source.Height * scale, MidpointRounding.AwayFromZero), 1, size);
        var resizedContent = resizer.Resize(source, contentWidth, contentHeight);
        var canvasPixels = new byte[checked(size * size * BytesPerPixel)];
        var offsetX = (size - contentWidth) / 2;
        var offsetY = (size - contentHeight) / 2;

        CopyIntoCanvas(resizedContent, canvasPixels, size, offsetX, offsetY);

        return new RgbaImage(size, size, canvasPixels);
    }

    private static void CopyIntoCanvas(
        RgbaImage source,
        byte[] canvasPixels,
        int canvasSize,
        int offsetX,
        int offsetY)
    {
        var sourcePixels = source.Pixels.Span;
        var canvasStride = canvasSize * BytesPerPixel;

        for (var y = 0; y < source.Height; y++)
        {
            var sourceRowOffset = y * source.Stride;
            var canvasRowOffset = (y + offsetY) * canvasStride + offsetX * BytesPerPixel;
            sourcePixels.Slice(sourceRowOffset, source.Stride).CopyTo(canvasPixels.AsSpan(canvasRowOffset, source.Stride));
        }
    }
}

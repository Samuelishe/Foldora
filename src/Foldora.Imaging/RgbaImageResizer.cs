namespace Foldora.Imaging;

/// <summary>
/// Выполняет alpha-aware resize для RGBA-изображений без Windows/WPF-зависимостей.
/// </summary>
public sealed class RgbaImageResizer
{
    public const int MaxDimension = 4096;
    private const int BytesPerPixel = 4;
    private const double LanczosRadius = 3.0;
    private readonly ImageResizeOptions _options;

    public RgbaImageResizer()
        : this(new ImageResizeOptions())
    {
    }

    public RgbaImageResizer(ImageResizeOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public RgbaImage Resize(RgbaImage source, int targetWidth, int targetHeight)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateTargetDimension(targetWidth, nameof(targetWidth));
        ValidateTargetDimension(targetHeight, nameof(targetHeight));
        _ = checked(targetWidth * targetHeight * BytesPerPixel);

        if (_options.Filter != ImageResizeFilter.Lanczos3)
        {
            throw new ArgumentOutOfRangeException(nameof(_options), _options.Filter, "Unsupported resize filter.");
        }

        var horizontalContributions = CreateContributions(source.Width, targetWidth);
        var verticalContributions = CreateContributions(source.Height, targetHeight);
        var horizontal = ResizeHorizontal(source, targetWidth, horizontalContributions);
        var targetPixels = ResizeVertical(horizontal, targetWidth, targetHeight, verticalContributions);

        return new RgbaImage(targetWidth, targetHeight, targetPixels);
    }

    private static void ValidateTargetDimension(int dimension, string parameterName)
    {
        if (dimension is < 1 or > MaxDimension)
        {
            throw new ArgumentOutOfRangeException(parameterName, dimension, $"Target dimension must be between 1 and {MaxDimension}.");
        }
    }

    private static ResizeContribution[] CreateContributions(int sourceSize, int targetSize)
    {
        var scale = (double)targetSize / sourceSize;
        var filterScale = scale < 1.0 ? 1.0 / scale : 1.0;
        var radius = LanczosRadius * filterScale;
        var contributions = new ResizeContribution[targetSize];

        for (var targetIndex = 0; targetIndex < targetSize; targetIndex++)
        {
            var sourceCenter = (targetIndex + 0.5) / scale - 0.5;
            var start = (int)Math.Ceiling(sourceCenter - radius);
            var end = (int)Math.Floor(sourceCenter + radius);
            var samples = new List<ResizeSample>(Math.Max(1, end - start + 1));
            var totalWeight = 0.0;

            for (var sourceIndex = start; sourceIndex <= end; sourceIndex++)
            {
                var clampedIndex = Math.Clamp(sourceIndex, 0, sourceSize - 1);
                var weight = Lanczos3((sourceIndex - sourceCenter) / filterScale);

                if (Math.Abs(weight) <= double.Epsilon)
                {
                    continue;
                }

                samples.Add(new ResizeSample(clampedIndex, weight));
                totalWeight += weight;
            }

            if (samples.Count == 0 || Math.Abs(totalWeight) <= double.Epsilon)
            {
                samples.Clear();
                samples.Add(new ResizeSample(Math.Clamp((int)Math.Round(sourceCenter), 0, sourceSize - 1), 1.0));
                totalWeight = 1.0;
            }

            for (var index = 0; index < samples.Count; index++)
            {
                var sample = samples[index];
                samples[index] = sample with { Weight = sample.Weight / totalWeight };
            }

            contributions[targetIndex] = new ResizeContribution(samples.ToArray());
        }

        return contributions;
    }

    private static double[] ResizeHorizontal(
        RgbaImage source,
        int targetWidth,
        IReadOnlyList<ResizeContribution> contributions)
    {
        var sourcePixels = source.Pixels.Span;
        var horizontal = new double[checked(targetWidth * source.Height * BytesPerPixel)];

        for (var y = 0; y < source.Height; y++)
        {
            var sourceRowOffset = y * source.Stride;
            var targetRowOffset = y * targetWidth * BytesPerPixel;

            for (var targetX = 0; targetX < targetWidth; targetX++)
            {
                var targetOffset = targetRowOffset + targetX * BytesPerPixel;
                AccumulateSourceRow(sourcePixels, sourceRowOffset, contributions[targetX], horizontal, targetOffset);
            }
        }

        return horizontal;
    }

    private static void AccumulateSourceRow(
        ReadOnlySpan<byte> sourcePixels,
        int sourceRowOffset,
        ResizeContribution contribution,
        double[] target,
        int targetOffset)
    {
        var premultipliedRed = 0.0;
        var premultipliedGreen = 0.0;
        var premultipliedBlue = 0.0;
        var alpha = 0.0;

        foreach (var sample in contribution.Samples)
        {
            var sourceOffset = sourceRowOffset + sample.SourceIndex * BytesPerPixel;
            var sourceAlpha = sourcePixels[sourceOffset + 3];
            var alphaFactor = sourceAlpha / 255.0;

            premultipliedRed += sourcePixels[sourceOffset] * alphaFactor * sample.Weight;
            premultipliedGreen += sourcePixels[sourceOffset + 1] * alphaFactor * sample.Weight;
            premultipliedBlue += sourcePixels[sourceOffset + 2] * alphaFactor * sample.Weight;
            alpha += sourceAlpha * sample.Weight;
        }

        target[targetOffset] = premultipliedRed;
        target[targetOffset + 1] = premultipliedGreen;
        target[targetOffset + 2] = premultipliedBlue;
        target[targetOffset + 3] = alpha;
    }

    private static byte[] ResizeVertical(
        double[] horizontal,
        int targetWidth,
        int targetHeight,
        IReadOnlyList<ResizeContribution> contributions)
    {
        var targetPixels = new byte[checked(targetWidth * targetHeight * BytesPerPixel)];
        var intermediateStride = targetWidth * BytesPerPixel;

        for (var targetY = 0; targetY < targetHeight; targetY++)
        {
            var targetRowOffset = targetY * intermediateStride;

            for (var x = 0; x < targetWidth; x++)
            {
                var premultipliedRed = 0.0;
                var premultipliedGreen = 0.0;
                var premultipliedBlue = 0.0;
                var alpha = 0.0;

                foreach (var sample in contributions[targetY].Samples)
                {
                    var sourceOffset = sample.SourceIndex * intermediateStride + x * BytesPerPixel;
                    premultipliedRed += horizontal[sourceOffset] * sample.Weight;
                    premultipliedGreen += horizontal[sourceOffset + 1] * sample.Weight;
                    premultipliedBlue += horizontal[sourceOffset + 2] * sample.Weight;
                    alpha += horizontal[sourceOffset + 3] * sample.Weight;
                }

                WriteUnpremultipliedPixel(targetPixels, targetRowOffset + x * BytesPerPixel, premultipliedRed, premultipliedGreen, premultipliedBlue, alpha);
            }
        }

        return targetPixels;
    }

    private static void WriteUnpremultipliedPixel(
        byte[] targetPixels,
        int targetOffset,
        double premultipliedRed,
        double premultipliedGreen,
        double premultipliedBlue,
        double alpha)
    {
        var alphaByte = ClampToByte(alpha);

        if (alphaByte == 0)
        {
            targetPixels[targetOffset] = 0;
            targetPixels[targetOffset + 1] = 0;
            targetPixels[targetOffset + 2] = 0;
            targetPixels[targetOffset + 3] = 0;
            return;
        }

        var unpremultiplyScale = 255.0 / alphaByte;
        targetPixels[targetOffset] = ClampToByte(premultipliedRed * unpremultiplyScale);
        targetPixels[targetOffset + 1] = ClampToByte(premultipliedGreen * unpremultiplyScale);
        targetPixels[targetOffset + 2] = ClampToByte(premultipliedBlue * unpremultiplyScale);
        targetPixels[targetOffset + 3] = alphaByte;
    }

    private static byte ClampToByte(double value)
    {
        if (value <= 0)
        {
            return 0;
        }

        if (value >= 255)
        {
            return 255;
        }

        return (byte)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    private static double Lanczos3(double value)
    {
        var distance = Math.Abs(value);
        if (distance < double.Epsilon)
        {
            return 1.0;
        }

        if (distance >= LanczosRadius)
        {
            return 0.0;
        }

        return Sinc(value) * Sinc(value / LanczosRadius);
    }

    private static double Sinc(double value)
    {
        if (Math.Abs(value) < double.Epsilon)
        {
            return 1.0;
        }

        var angle = Math.PI * value;
        return Math.Sin(angle) / angle;
    }

    private sealed record ResizeContribution(IReadOnlyList<ResizeSample> Samples);

    private readonly record struct ResizeSample(int SourceIndex, double Weight);
}

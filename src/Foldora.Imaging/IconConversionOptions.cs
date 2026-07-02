namespace Foldora.Imaging;

/// <summary>
/// Настройки будущего image-to-ICO conversion pipeline.
/// </summary>
public sealed class IconConversionOptions
{
    public IconConversionOptions(
        IReadOnlyList<IconFrameSize>? targetFrameSizes = null,
        ImageResizeOptions? resizeOptions = null,
        IconImageFitMode fitMode = IconImageFitMode.Contain)
    {
        TargetFrameSizes = ValidateFrameSizes(targetFrameSizes ?? StandardIconFrameSizes.All);
        ResizeOptions = resizeOptions ?? new ImageResizeOptions();
        FitMode = fitMode;
    }

    public IReadOnlyList<IconFrameSize> TargetFrameSizes { get; }

    public ImageResizeOptions ResizeOptions { get; }

    public IconImageFitMode FitMode { get; }

    public static IconConversionOptions Default { get; } = new();

    private static IReadOnlyList<IconFrameSize> ValidateFrameSizes(IReadOnlyList<IconFrameSize> frameSizes)
    {
        ArgumentNullException.ThrowIfNull(frameSizes);

        if (frameSizes.Count == 0)
        {
            throw new ArgumentException("At least one target frame size is required.", nameof(frameSizes));
        }

        var duplicates = frameSizes
            .GroupBy(size => size.Size)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            throw new ArgumentException("Target frame sizes must be unique.", nameof(frameSizes));
        }

        return frameSizes
            .OrderBy(size => size.Size)
            .ToArray();
    }
}

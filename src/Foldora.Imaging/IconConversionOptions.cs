namespace Foldora.Imaging;

/// <summary>
/// Настройки будущего image-to-ICO conversion pipeline.
/// </summary>
public sealed class IconConversionOptions
{
    public IconConversionOptions(IReadOnlyList<IconFrameSize>? targetFrameSizes = null)
    {
        TargetFrameSizes = ValidateFrameSizes(targetFrameSizes ?? StandardIconFrameSizes.All);
    }

    public IReadOnlyList<IconFrameSize> TargetFrameSizes { get; }

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

        return frameSizes.ToArray();
    }
}


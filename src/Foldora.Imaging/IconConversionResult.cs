namespace Foldora.Imaging;

/// <summary>
/// Результат будущего image-to-ICO conversion pipeline.
/// </summary>
public sealed class IconConversionResult
{
    private IconConversionResult(
        bool succeeded,
        string? outputPath,
        int? sourceWidth,
        int? sourceHeight,
        IReadOnlyList<IconFrameSize> generatedFrameSizes,
        IReadOnlyList<IconConversionError> errors,
        IReadOnlyList<IconConversionError> warnings)
    {
        Succeeded = succeeded;
        OutputPath = outputPath;
        SourceWidth = sourceWidth;
        SourceHeight = sourceHeight;
        GeneratedFrameSizes = generatedFrameSizes;
        Errors = errors;
        Warnings = warnings;
    }

    public bool Succeeded { get; }

    public string? OutputPath { get; }

    public int? SourceWidth { get; }

    public int? SourceHeight { get; }

    public IReadOnlyList<IconFrameSize> GeneratedFrameSizes { get; }

    public IReadOnlyList<IconConversionError> Errors { get; }

    public IReadOnlyList<IconConversionError> Warnings { get; }

    public static IconConversionResult Success(
        IReadOnlyList<IconFrameSize> generatedFrameSizes,
        string? outputPath = null,
        IReadOnlyList<IconConversionError>? warnings = null,
        int? sourceWidth = null,
        int? sourceHeight = null)
    {
        ArgumentNullException.ThrowIfNull(generatedFrameSizes);

        return new IconConversionResult(
            true,
            outputPath,
            sourceWidth,
            sourceHeight,
            generatedFrameSizes.ToArray(),
            [],
            warnings?.ToArray() ?? []);
    }

    public static IconConversionResult Failure(
        IReadOnlyList<IconConversionError> errors,
        string? outputPath = null,
        IReadOnlyList<IconConversionError>? warnings = null)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            throw new ArgumentException("Failure result must include at least one error.", nameof(errors));
        }

        return new IconConversionResult(
            false,
            outputPath,
            null,
            null,
            [],
            errors.ToArray(),
            warnings?.ToArray() ?? []);
    }
}

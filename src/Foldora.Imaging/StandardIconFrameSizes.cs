namespace Foldora.Imaging;

/// <summary>
/// Стандартные размеры кадров, которые Foldora планирует генерировать для ICO.
/// </summary>
public static class StandardIconFrameSizes
{
    public static IReadOnlyList<IconFrameSize> All { get; } =
        Array.AsReadOnly(
        [
            new IconFrameSize(16),
            new IconFrameSize(24),
            new IconFrameSize(32),
            new IconFrameSize(48),
            new IconFrameSize(64),
            new IconFrameSize(128),
            new IconFrameSize(256)
        ]);
}


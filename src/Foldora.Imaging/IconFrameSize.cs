namespace Foldora.Imaging;

/// <summary>
/// Квадратный размер icon frame в диапазоне, поддерживаемом ICO directory.
/// </summary>
public readonly record struct IconFrameSize
{
    public IconFrameSize(int size)
    {
        if (size is < 1 or > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "Icon frame size must be between 1 and 256.");
        }

        Size = size;
    }

    public int Size { get; }

    public int Width => Size;

    public int Height => Size;

    internal byte DirectorySizeByte => Size == 256 ? (byte)0 : (byte)Size;

    public override string ToString()
    {
        return $"{Size}x{Size}";
    }
}


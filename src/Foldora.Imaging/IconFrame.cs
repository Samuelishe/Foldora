namespace Foldora.Imaging;

/// <summary>
/// Уже закодированный payload одного icon frame.
/// </summary>
public sealed class IconFrame
{
    public IconFrame(
        IconFrameSize size,
        byte[] encodedPayload,
        IconFrameEncoding encoding = IconFrameEncoding.Png,
        ushort bitDepth = 32)
    {
        ArgumentNullException.ThrowIfNull(encodedPayload);

        if (encodedPayload.Length == 0)
        {
            throw new ArgumentException("Icon frame payload must not be empty.", nameof(encodedPayload));
        }

        if (bitDepth == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bitDepth), bitDepth, "Icon frame bit depth must be positive.");
        }

        Size = size;
        EncodedPayload = encodedPayload.ToArray();
        Encoding = encoding;
        BitDepth = bitDepth;
    }

    public IconFrameSize Size { get; }

    public ReadOnlyMemory<byte> EncodedPayload { get; }

    public IconFrameEncoding Encoding { get; }

    public ushort BitDepth { get; }
}


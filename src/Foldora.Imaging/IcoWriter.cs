namespace Foldora.Imaging;

/// <summary>
/// Записывает ICO container из уже закодированных image frame payloads.
/// </summary>
public sealed class IcoWriter
{
    private const ushort IconType = 1;
    private const ushort DefaultPlanes = 1;
    private const int IconDirSize = 6;
    private const int IconDirEntrySize = 16;

    public byte[] Write(IReadOnlyCollection<IconFrame> frames)
    {
        using var output = new MemoryStream();
        Write(frames, output);
        return output.ToArray();
    }

    public void Write(IReadOnlyCollection<IconFrame> frames, Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (!output.CanWrite)
        {
            throw new ArgumentException("Output stream must be writable.", nameof(output));
        }

        var normalizedFrames = NormalizeFrames(frames);
        var payloadOffset = checked(IconDirSize + IconDirEntrySize * normalizedFrames.Count);
        var offsets = new int[normalizedFrames.Count];

        for (var index = 0; index < normalizedFrames.Count; index++)
        {
            offsets[index] = payloadOffset;
            payloadOffset = checked(payloadOffset + normalizedFrames[index].EncodedPayload.Length);
        }

        Span<byte> header = stackalloc byte[IconDirSize];
        WriteUInt16LittleEndian(header, 0, 0);
        WriteUInt16LittleEndian(header, 2, IconType);
        WriteUInt16LittleEndian(header, 4, checked((ushort)normalizedFrames.Count));
        output.Write(header);

        Span<byte> entry = stackalloc byte[IconDirEntrySize];
        for (var index = 0; index < normalizedFrames.Count; index++)
        {
            var frame = normalizedFrames[index];
            entry.Clear();
            entry[0] = frame.Size.DirectorySizeByte;
            entry[1] = frame.Size.DirectorySizeByte;
            entry[2] = 0;
            entry[3] = 0;
            WriteUInt16LittleEndian(entry, 4, DefaultPlanes);
            WriteUInt16LittleEndian(entry, 6, frame.BitDepth);
            WriteUInt32LittleEndian(entry, 8, checked((uint)frame.EncodedPayload.Length));
            WriteUInt32LittleEndian(entry, 12, checked((uint)offsets[index]));
            output.Write(entry);
        }

        foreach (var frame in normalizedFrames)
        {
            output.Write(frame.EncodedPayload.Span);
        }
    }

    private static List<IconFrame> NormalizeFrames(IReadOnlyCollection<IconFrame> frames)
    {
        ArgumentNullException.ThrowIfNull(frames);

        if (frames.Count == 0)
        {
            throw new ArgumentException("At least one icon frame is required.", nameof(frames));
        }

        if (frames.Count > ushort.MaxValue)
        {
            throw new ArgumentException("ICO frame count cannot exceed UInt16.MaxValue.", nameof(frames));
        }

        var normalizedFrames = new List<IconFrame>(frames.Count);
        foreach (var frame in frames)
        {
            ArgumentNullException.ThrowIfNull(frame);

            if (frame.Encoding != IconFrameEncoding.Png)
            {
                throw new ArgumentException("Only PNG-encoded ICO frame payloads are supported in IC1.", nameof(frames));
            }

            if (frame.EncodedPayload.IsEmpty)
            {
                throw new ArgumentException("Icon frame payload must not be empty.", nameof(frames));
            }

            normalizedFrames.Add(frame);
        }

        var duplicateSize = normalizedFrames
            .GroupBy(frame => frame.Size.Size)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateSize is not null)
        {
            throw new ArgumentException($"Duplicate icon frame size is not allowed: {duplicateSize.Key}x{duplicateSize.Key}.", nameof(frames));
        }

        return normalizedFrames
            .OrderBy(frame => frame.Size.Size)
            .ToList();
    }

    private static void WriteUInt16LittleEndian(Span<byte> buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
    }

    private static void WriteUInt32LittleEndian(Span<byte> buffer, int offset, uint value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
        buffer[offset + 3] = (byte)(value >> 24);
    }
}


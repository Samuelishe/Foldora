using Foldora.Imaging;

namespace Foldora.Tests.Imaging;

public sealed class IcoWriterTests
{
    [Fact]
    public void StandardIconFrameSizes_AreExpectedAscendingSet()
    {
        var sizes = StandardIconFrameSizes.All.Select(size => size.Size).ToArray();

        Assert.Equal([16, 24, 32, 48, 64, 128, 256], sizes);
    }

    [Fact]
    public void IconFrameSize_ToStringUsesSquareSize()
    {
        var size = new IconFrameSize(16);

        Assert.Equal("16x16", size.ToString());
        Assert.Equal(16, size.Width);
        Assert.Equal(16, size.Height);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(257)]
    public void IconFrameSize_RejectsValuesOutsideIcoDirectoryRange(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IconFrameSize(size));
    }

    [Fact]
    public void Write_SingleFrame_WritesIconDirAndDirectoryEntry()
    {
        var payload = CreateFakePngPayload(0x31);
        var writer = new IcoWriter();

        var ico = writer.Write([new IconFrame(new IconFrameSize(16), payload)]);

        Assert.Equal(0, ReadUInt16(ico, 0));
        Assert.Equal(1, ReadUInt16(ico, 2));
        Assert.Equal(1, ReadUInt16(ico, 4));

        Assert.Equal(16, ico[6]);
        Assert.Equal(16, ico[7]);
        Assert.Equal(0, ico[8]);
        Assert.Equal(0, ico[9]);
        Assert.Equal(1, ReadUInt16(ico, 10));
        Assert.Equal(32, ReadUInt16(ico, 12));
        Assert.Equal(payload.Length, ReadUInt32(ico, 14));
        Assert.Equal(22, ReadUInt32(ico, 18));
        AssertPayloadAt(ico, 22, payload);
    }

    [Fact]
    public void Write_FrameSize256_WritesZeroWidthAndHeightBytes()
    {
        var payload = CreateFakePngPayload(0x32);
        var writer = new IcoWriter();

        var ico = writer.Write([new IconFrame(new IconFrameSize(256), payload)]);

        Assert.Equal(0, ico[6]);
        Assert.Equal(0, ico[7]);
        Assert.Equal(payload.Length, ReadUInt32(ico, 14));
        Assert.Equal(22, ReadUInt32(ico, 18));
    }

    [Fact]
    public void Write_MultipleFrames_SortsAscendingAndWritesOffsetsAndPayloads()
    {
        var payload48 = CreateFakePngPayload(0x48, 20);
        var payload16 = CreateFakePngPayload(0x16, 24);
        var payload32 = CreateFakePngPayload(0x32, 28);
        var writer = new IcoWriter();

        var ico = writer.Write(
        [
            new IconFrame(new IconFrameSize(48), payload48),
            new IconFrame(new IconFrameSize(16), payload16),
            new IconFrame(new IconFrameSize(32), payload32)
        ]);

        Assert.Equal(3, ReadUInt16(ico, 4));

        AssertDirectoryEntry(ico, entryIndex: 0, sizeByte: 16, payloadLength: payload16.Length, offset: 54);
        AssertDirectoryEntry(ico, entryIndex: 1, sizeByte: 32, payloadLength: payload32.Length, offset: 54 + payload16.Length);
        AssertDirectoryEntry(ico, entryIndex: 2, sizeByte: 48, payloadLength: payload48.Length, offset: 54 + payload16.Length + payload32.Length);

        AssertPayloadAt(ico, 54, payload16);
        AssertPayloadAt(ico, 54 + payload16.Length, payload32);
        AssertPayloadAt(ico, 54 + payload16.Length + payload32.Length, payload48);
    }

    [Fact]
    public void Write_RejectsNullFrameCollection()
    {
        var writer = new IcoWriter();

        Assert.Throws<ArgumentNullException>(() => writer.Write(null!));
    }

    [Fact]
    public void Write_RejectsEmptyFrameCollection()
    {
        var writer = new IcoWriter();

        var exception = Assert.Throws<ArgumentException>(() => writer.Write([]));
        Assert.Equal("frames", exception.ParamName);
    }

    [Fact]
    public void Write_RejectsDuplicateFrameSizes()
    {
        var writer = new IcoWriter();

        var exception = Assert.Throws<ArgumentException>(() => writer.Write(
        [
            new IconFrame(new IconFrameSize(32), CreateFakePngPayload(0x01)),
            new IconFrame(new IconFrameSize(32), CreateFakePngPayload(0x02))
        ]));

        Assert.Equal("frames", exception.ParamName);
    }

    [Fact]
    public void IconFrame_RejectsNullPayload()
    {
        Assert.Throws<ArgumentNullException>(() => new IconFrame(new IconFrameSize(16), null!));
    }

    [Fact]
    public void IconFrame_RejectsEmptyPayload()
    {
        var exception = Assert.Throws<ArgumentException>(() => new IconFrame(new IconFrameSize(16), []));

        Assert.Equal("encodedPayload", exception.ParamName);
    }

    [Fact]
    public void Write_RejectsNonWritableOutputStream()
    {
        var writer = new IcoWriter();
        using var stream = new MemoryStream(new byte[128], writable: false);

        var exception = Assert.Throws<ArgumentException>(() => writer.Write(
            [new IconFrame(new IconFrameSize(16), CreateFakePngPayload(0x01))],
            stream));

        Assert.Equal("output", exception.ParamName);
    }

    [Fact]
    public void Write_DoesNotCloseCallerOwnedStream()
    {
        var writer = new IcoWriter();
        using var stream = new MemoryStream();

        writer.Write([new IconFrame(new IconFrameSize(16), CreateFakePngPayload(0x01))], stream);
        stream.WriteByte(0xFF);

        Assert.True(stream.CanWrite);
        Assert.Equal(23 + 32, stream.Length);
    }

    private static void AssertDirectoryEntry(byte[] ico, int entryIndex, int sizeByte, int payloadLength, int offset)
    {
        var entryOffset = 6 + entryIndex * 16;
        Assert.Equal(sizeByte, ico[entryOffset]);
        Assert.Equal(sizeByte, ico[entryOffset + 1]);
        Assert.Equal(0, ico[entryOffset + 2]);
        Assert.Equal(0, ico[entryOffset + 3]);
        Assert.Equal(1, ReadUInt16(ico, entryOffset + 4));
        Assert.Equal(32, ReadUInt16(ico, entryOffset + 6));
        Assert.Equal(payloadLength, ReadUInt32(ico, entryOffset + 8));
        Assert.Equal(offset, ReadUInt32(ico, entryOffset + 12));
    }

    private static void AssertPayloadAt(byte[] ico, int offset, byte[] expectedPayload)
    {
        Assert.Equal(expectedPayload, ico.Skip(offset).Take(expectedPayload.Length).ToArray());
    }

    private static byte[] CreateFakePngPayload(byte marker, int length = 32)
    {
        if (length < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        var payload = new byte[length];
        var signature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        signature.CopyTo(payload, 0);

        for (var index = signature.Length; index < payload.Length; index++)
        {
            payload[index] = (byte)(marker + index);
        }

        return payload;
    }

    private static ushort ReadUInt16(byte[] data, int offset)
    {
        return (ushort)(data[offset] | (data[offset + 1] << 8));
    }

    private static int ReadUInt32(byte[] data, int offset)
    {
        return data[offset]
               | (data[offset + 1] << 8)
               | (data[offset + 2] << 16)
               | (data[offset + 3] << 24);
    }
}

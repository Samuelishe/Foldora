namespace Foldora.Tests.Fixtures;

internal static class IcoTestFile
{
    public static async Task WriteValidAsync(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, CreateValidBytes());
    }

    public static async Task WriteValidPreviewAsync(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, CreateValidPreviewBytes());
    }

    public static byte[] CreateValidBytes()
    {
        return
        [
            0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
            0x10, 0x10, 0x00, 0x00, 0x01, 0x00, 0x20, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x16, 0x00, 0x00, 0x00,
            0x89, 0x50, 0x4E, 0x47
        ];
    }

    public static byte[] CreateOutOfBoundsBytes()
    {
        return
        [
            0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
            0x10, 0x10, 0x00, 0x00, 0x01, 0x00, 0x20, 0x00,
            0xFF, 0x00, 0x00, 0x00,
            0x16, 0x00, 0x00, 0x00,
            0x89, 0x50, 0x4E, 0x47
        ];
    }

    public static byte[] CreateValidPreviewBytes()
    {
        const int width = 16;
        const int height = 16;
        const int xorSize = width * height * 4;
        const int andMaskStride = 4;
        const int andMaskSize = andMaskStride * height;
        const int imageSize = 40 + xorSize + andMaskSize;
        const int imageOffset = 22;

        var bytes = new byte[imageOffset + imageSize];

        bytes[2] = 0x01;
        bytes[4] = 0x01;
        bytes[6] = width;
        bytes[7] = height;
        bytes[10] = 0x01;
        bytes[12] = 0x20;
        WriteUInt32(bytes, 14, imageSize);
        WriteUInt32(bytes, 18, imageOffset);

        WriteUInt32(bytes, imageOffset, 40);
        WriteUInt32(bytes, imageOffset + 4, width);
        WriteUInt32(bytes, imageOffset + 8, height * 2);
        bytes[imageOffset + 12] = 0x01;
        bytes[imageOffset + 14] = 0x20;
        WriteUInt32(bytes, imageOffset + 20, xorSize);

        var pixelOffset = imageOffset + 40;
        for (var index = 0; index < width * height; index++)
        {
            var offset = pixelOffset + index * 4;
            bytes[offset] = 0x20;
            bytes[offset + 1] = 0x80;
            bytes[offset + 2] = 0xE0;
            bytes[offset + 3] = 0xFF;
        }

        return bytes;
    }

    private static void WriteUInt32(byte[] bytes, int offset, int value)
    {
        bytes[offset] = (byte)(value & 0xFF);
        bytes[offset + 1] = (byte)((value >> 8) & 0xFF);
        bytes[offset + 2] = (byte)((value >> 16) & 0xFF);
        bytes[offset + 3] = (byte)((value >> 24) & 0xFF);
    }
}

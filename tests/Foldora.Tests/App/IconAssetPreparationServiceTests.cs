using System.Windows.Media;
using System.Windows.Media.Imaging;
using Foldora.App.Services;
using Foldora.Core.Storage;
using Foldora.Imaging;
using Foldora.Imaging.Windows;
using Foldora.Tests.Fixtures;

namespace Foldora.Tests.App;

public sealed class IconAssetPreparationServiceTests
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    [Fact]
    public async Task PrepareAsync_IcoInput_UsesExistingImportPath()
    {
        using var directory = TempDirectory.Create();
        var paths = CreatePaths(directory.Path);
        var sourceIcon = directory.PathFor("source.ico");
        await IcoTestFile.WriteValidAsync(sourceIcon);
        var service = new IconAssetPreparationService(paths);

        var result = await service.PrepareAsync(sourceIcon);

        Assert.False(result.WasConverted);
        Assert.True(result.ImportOnSave);
        Assert.Equal(Path.GetFullPath(sourceIcon), result.IconPath);
        Assert.False(Directory.Exists(Path.Combine(paths.IconsDirectory, "generated")));
    }

    [Fact]
    public async Task PrepareAsync_PngInput_ConvertsToGeneratedIco()
    {
        using var directory = TempDirectory.Create();
        var paths = CreatePaths(directory.Path);
        var sourceImage = directory.WriteFile("My Icon.png", new WindowsPngFrameEncoder().Encode(CreateImage(width: 8, height: 8)));
        var service = new IconAssetPreparationService(paths);

        var result = await service.PrepareAsync(sourceImage);

        Assert.True(result.WasConverted);
        Assert.False(result.ImportOnSave);
        Assert.StartsWith(Path.Combine(paths.IconsDirectory, "generated"), result.IconPath, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(".ico", result.IconPath, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(result.IconPath));
        AssertStandardIco(File.ReadAllBytes(result.IconPath));
    }

    [Fact]
    public async Task PrepareAsync_JpegInput_ConvertsToGeneratedIco()
    {
        using var directory = TempDirectory.Create();
        var paths = CreatePaths(directory.Path);
        var sourceImage = directory.WriteFile("source.jpg", EncodeWithWpf(CreateImage(width: 8, height: 8), new JpegBitmapEncoder { QualityLevel = 100 }));
        var service = new IconAssetPreparationService(paths);

        var result = await service.PrepareAsync(sourceImage);

        Assert.True(result.WasConverted);
        AssertStandardIco(File.ReadAllBytes(result.IconPath));
    }

    [Fact]
    public async Task PrepareAsync_BmpInput_ConvertsToGeneratedIco()
    {
        using var directory = TempDirectory.Create();
        var paths = CreatePaths(directory.Path);
        var sourceImage = directory.WriteFile("source.bmp", EncodeWithWpf(CreateImage(width: 8, height: 8), new BmpBitmapEncoder()));
        var service = new IconAssetPreparationService(paths);

        var result = await service.PrepareAsync(sourceImage);

        Assert.True(result.WasConverted);
        AssertStandardIco(File.ReadAllBytes(result.IconPath));
    }

    [Fact]
    public async Task PrepareAsync_UnsupportedExtension_IsRejected()
    {
        using var directory = TempDirectory.Create();
        var paths = CreatePaths(directory.Path);
        var source = directory.WriteFile("source.txt", [0x01, 0x02]);
        var service = new IconAssetPreparationService(paths);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.PrepareAsync(source));

        Assert.Equal("selectedFilePath", exception.ParamName);
    }

    [Fact]
    public async Task PrepareAsync_CorruptImage_DoesNotLeaveFinalOrTempIco()
    {
        using var directory = TempDirectory.Create();
        var paths = CreatePaths(directory.Path);
        var source = directory.WriteFile("source.png", [0x01, 0x02, 0x03]);
        var service = new IconAssetPreparationService(paths);

        await Assert.ThrowsAsync<ArgumentException>(() => service.PrepareAsync(source));

        var generatedDirectory = Path.Combine(paths.IconsDirectory, "generated");
        Assert.True(Directory.Exists(generatedDirectory));
        Assert.Empty(Directory.GetFiles(generatedDirectory));
    }

    [Fact]
    public async Task PrepareAsync_GeneratedFilename_IsSanitized()
    {
        using var directory = TempDirectory.Create();
        var paths = CreatePaths(directory.Path);
        var source = directory.WriteFile("bad name #1.png", new WindowsPngFrameEncoder().Encode(CreateImage(width: 4, height: 4)));
        var service = new IconAssetPreparationService(paths);

        var result = await service.PrepareAsync(source);
        var fileName = Path.GetFileName(result.IconPath);

        Assert.DoesNotContain(' ', fileName);
        Assert.StartsWith("bad-name-#1-", fileName, StringComparison.Ordinal);
        Assert.EndsWith(".ico", fileName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PrepareAsync_SameBaseDifferentContent_DoesNotOverwrite()
    {
        using var directory = TempDirectory.Create();
        var paths = CreatePaths(directory.Path);
        var first = directory.WriteFile("one\\source.png", new WindowsPngFrameEncoder().Encode(CreateSolidImage(width: 4, height: 4, red: 20)));
        var second = directory.WriteFile("two\\source.png", new WindowsPngFrameEncoder().Encode(CreateSolidImage(width: 4, height: 4, red: 200)));
        var service = new IconAssetPreparationService(paths);

        var firstResult = await service.PrepareAsync(first);
        var secondResult = await service.PrepareAsync(second);

        Assert.NotEqual(firstResult.IconPath, secondResult.IconPath);
        Assert.True(File.Exists(firstResult.IconPath));
        Assert.True(File.Exists(secondResult.IconPath));
    }

    [Fact]
    public async Task PrepareAsync_SameContent_ReusesGeneratedIcon()
    {
        using var directory = TempDirectory.Create();
        var paths = CreatePaths(directory.Path);
        var source = directory.WriteFile("source.png", new WindowsPngFrameEncoder().Encode(CreateImage(width: 4, height: 4)));
        var service = new IconAssetPreparationService(paths);

        var first = await service.PrepareAsync(source);
        var second = await service.PrepareAsync(source);

        Assert.Equal(first.IconPath, second.IconPath);
        Assert.Single(Directory.GetFiles(Path.Combine(paths.IconsDirectory, "generated"), "*.ico"));
    }

    private static FoldoraDataPaths CreatePaths(string root)
    {
        return new FoldoraDataPaths(Path.Combine(root, "Foldora"));
    }

    private static RgbaImage CreateImage(int width, int height)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                pixels[offset] = (byte)(40 + x * 5);
                pixels[offset + 1] = (byte)(80 + y * 5);
                pixels[offset + 2] = 220;
                pixels[offset + 3] = 255;
            }
        }

        return new RgbaImage(width, height, pixels);
    }

    private static RgbaImage CreateSolidImage(int width, int height, byte red)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var offset = 0; offset < pixels.Length; offset += 4)
        {
            pixels[offset] = red;
            pixels[offset + 1] = 80;
            pixels[offset + 2] = 220;
            pixels[offset + 3] = 255;
        }

        return new RgbaImage(width, height, pixels);
    }

    private static byte[] EncodeWithWpf(RgbaImage image, BitmapEncoder encoder)
    {
        var sourcePixels = ConvertRgbaToBgr(image.Pixels.ToArray());
        var bitmap = BitmapSource.Create(
            image.Width,
            image.Height,
            96,
            96,
            PixelFormats.Bgr24,
            palette: null,
            sourcePixels,
            checked(image.Width * 3));

        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var output = new MemoryStream();
        encoder.Save(output);
        return output.ToArray();
    }

    private static byte[] ConvertRgbaToBgr(byte[] rgbaPixels)
    {
        var bgrPixels = new byte[rgbaPixels.Length / 4 * 3];
        for (int sourceOffset = 0, targetOffset = 0; sourceOffset < rgbaPixels.Length; sourceOffset += 4, targetOffset += 3)
        {
            bgrPixels[targetOffset] = rgbaPixels[sourceOffset + 2];
            bgrPixels[targetOffset + 1] = rgbaPixels[sourceOffset + 1];
            bgrPixels[targetOffset + 2] = rgbaPixels[sourceOffset];
        }

        return bgrPixels;
    }

    private static void AssertStandardIco(byte[] ico)
    {
        Assert.Equal(0, ReadUInt16(ico, 0));
        Assert.Equal(1, ReadUInt16(ico, 2));
        Assert.Equal(7, ReadUInt16(ico, 4));

        var entries = ReadDirectoryEntries(ico);
        Assert.Equal([16, 24, 32, 48, 64, 128, 256], entries.Select(entry => entry.Size).ToArray());

        foreach (var entry in entries)
        {
            Assert.Equal(PngSignature, ico.Skip(entry.Offset).Take(PngSignature.Length).ToArray());
        }
    }

    private static IcoDirectoryEntry[] ReadDirectoryEntries(byte[] ico)
    {
        var count = ReadUInt16(ico, 4);
        var entries = new IcoDirectoryEntry[count];

        for (var index = 0; index < count; index++)
        {
            var offset = 6 + index * 16;
            entries[index] = new IcoDirectoryEntry(
                ico[offset] == 0 ? 256 : ico[offset],
                ReadUInt32(ico, offset + 8),
                ReadUInt32(ico, offset + 12));
        }

        return entries;
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

    private sealed class TempDirectory : IDisposable
    {
        private TempDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TempDirectory Create()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"FoldoraIconAssetTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(path);
            return new TempDirectory(path);
        }

        public string PathFor(string relativePath)
        {
            return System.IO.Path.Combine(Path, relativePath);
        }

        public string WriteFile(string relativePath, byte[] bytes)
        {
            var path = PathFor(relativePath);
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(path, bytes);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }

    private readonly record struct IcoDirectoryEntry(int Size, int BytesInResource, int Offset);
}

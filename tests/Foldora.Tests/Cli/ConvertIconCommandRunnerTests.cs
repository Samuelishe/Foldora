using System.Windows.Media;
using System.Windows.Media.Imaging;
using Foldora.Cli;
using Foldora.Imaging;
using Foldora.Imaging.Windows;

namespace Foldora.Tests.Cli;

public sealed class ConvertIconCommandRunnerTests
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    [Fact]
    public void Run_ConvertsPngFileToStandardIco()
    {
        using var directory = TempDirectory.Create();
        var inputPath = directory.WriteFile("source.png", new WindowsPngFrameEncoder().Encode(CreateImage(width: 8, height: 8)));
        var outputPath = directory.PathFor("folder.ico");

        var exitCode = Run(inputPath, outputPath, force: false, out var output, out var error);

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(outputPath));
        Assert.Empty(error);
        Assert.Contains("Converted image to ICO.", output);
        Assert.Contains("Frames: 16, 24, 32, 48, 64, 128, 256", output);
        Assert.Contains("Source: 8x8", output);
        AssertStandardIco(File.ReadAllBytes(outputPath));
    }

    [Fact]
    public void Run_ConvertsJpegFileToStandardIco()
    {
        using var directory = TempDirectory.Create();
        var inputPath = directory.WriteFile("source.jpg", EncodeWithWpf(CreateImage(width: 8, height: 8), new JpegBitmapEncoder { QualityLevel = 100 }));
        var outputPath = directory.PathFor("folder.ico");

        var exitCode = Run(inputPath, outputPath, force: false, out _, out var error);

        Assert.Equal(0, exitCode);
        Assert.Empty(error);
        AssertStandardIco(File.ReadAllBytes(outputPath));
    }

    [Fact]
    public void Run_ConvertsBmpFileToStandardIco()
    {
        using var directory = TempDirectory.Create();
        var inputPath = directory.WriteFile("source.bmp", EncodeWithWpf(CreateImage(width: 8, height: 8), new BmpBitmapEncoder()));
        var outputPath = directory.PathFor("folder.ico");

        var exitCode = Run(inputPath, outputPath, force: false, out _, out var error);

        Assert.Equal(0, exitCode);
        Assert.Empty(error);
        AssertStandardIco(File.ReadAllBytes(outputPath));
    }

    [Fact]
    public void Run_RefusesUnsupportedExtension()
    {
        using var directory = TempDirectory.Create();
        var inputPath = directory.WriteFile("source.txt", [0x01, 0x02]);
        var outputPath = directory.PathFor("folder.ico");

        var exitCode = Run(inputPath, outputPath, force: false, out _, out var error);

        Assert.Equal(1, exitCode);
        Assert.Contains("PNG, JPG, JPEG or BMP", error);
        Assert.False(File.Exists(outputPath));
    }

    [Fact]
    public void Run_RefusesMissingInputFile()
    {
        using var directory = TempDirectory.Create();
        var outputPath = directory.PathFor("folder.ico");

        var exitCode = Run(directory.PathFor("missing.png"), outputPath, force: false, out _, out var error);

        Assert.Equal(1, exitCode);
        Assert.Contains("Input file does not exist", error);
        Assert.False(File.Exists(outputPath));
    }

    [Fact]
    public void Run_RefusesDirectoryInput()
    {
        using var directory = TempDirectory.Create();
        var inputDirectory = Directory.CreateDirectory(directory.PathFor("images")).FullName;
        var outputPath = directory.PathFor("folder.ico");

        var exitCode = Run(inputDirectory, outputPath, force: false, out _, out var error);

        Assert.Equal(1, exitCode);
        Assert.Contains("must be a file", error);
        Assert.False(File.Exists(outputPath));
    }

    [Fact]
    public void Run_RefusesNonIcoOutputExtension()
    {
        using var directory = TempDirectory.Create();
        var inputPath = directory.WriteFile("source.png", new WindowsPngFrameEncoder().Encode(CreateImage(width: 4, height: 4)));

        var exitCode = Run(inputPath, directory.PathFor("folder.png"), force: false, out _, out var error);

        Assert.Equal(1, exitCode);
        Assert.Contains(".ico", error);
    }

    [Fact]
    public void Run_RefusesExistingOutputWithoutForce()
    {
        using var directory = TempDirectory.Create();
        var inputPath = directory.WriteFile("source.png", new WindowsPngFrameEncoder().Encode(CreateImage(width: 4, height: 4)));
        var outputPath = directory.WriteFile("folder.ico", [0xAA, 0xBB, 0xCC]);

        var exitCode = Run(inputPath, outputPath, force: false, out _, out var error);

        Assert.Equal(1, exitCode);
        Assert.Contains("--force", error);
        Assert.Equal([0xAA, 0xBB, 0xCC], File.ReadAllBytes(outputPath));
    }

    [Fact]
    public void Run_OverwritesExistingOutputWithForce()
    {
        using var directory = TempDirectory.Create();
        var inputPath = directory.WriteFile("source.png", new WindowsPngFrameEncoder().Encode(CreateImage(width: 4, height: 4)));
        var outputPath = directory.WriteFile("folder.ico", [0xAA, 0xBB, 0xCC]);

        var exitCode = Run(inputPath, outputPath, force: true, out _, out var error);

        Assert.Equal(0, exitCode);
        Assert.Empty(error);
        AssertStandardIco(File.ReadAllBytes(outputPath));
    }

    [Fact]
    public void Run_RefusesMissingOutputDirectory()
    {
        using var directory = TempDirectory.Create();
        var inputPath = directory.WriteFile("source.png", new WindowsPngFrameEncoder().Encode(CreateImage(width: 4, height: 4)));
        var outputPath = Path.Combine(directory.PathFor("missing"), "folder.ico");

        var exitCode = Run(inputPath, outputPath, force: false, out _, out var error);

        Assert.Equal(1, exitCode);
        Assert.Contains("Output directory does not exist", error);
        Assert.False(File.Exists(outputPath));
    }

    [Fact]
    public void Run_DoesNotLeavePartialOutputAfterCorruptInput()
    {
        using var directory = TempDirectory.Create();
        var inputPath = directory.WriteFile("source.png", [0x01, 0x02, 0x03]);
        var outputPath = directory.PathFor("folder.ico");

        var exitCode = Run(inputPath, outputPath, force: false, out _, out var error);

        Assert.Equal(1, exitCode);
        Assert.Contains("Error:", error);
        Assert.False(File.Exists(outputPath));
        Assert.Empty(Directory.GetFiles(directory.Path, "*.tmp"));
    }

    private static int Run(string inputPath, string outputPath, bool force, out string output, out string error)
    {
        using var outputWriter = new StringWriter();
        using var errorWriter = new StringWriter();

        var exitCode = new ConvertIconCommandRunner().Run(inputPath, outputPath, force, outputWriter, errorWriter);

        output = outputWriter.ToString();
        error = errorWriter.ToString();
        return exitCode;
    }

    private static RgbaImage CreateImage(int width, int height)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                pixels[offset] = (byte)(30 + x * 8);
                pixels[offset + 1] = (byte)(90 + y * 8);
                pixels[offset + 2] = 220;
                pixels[offset + 3] = 255;
            }
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

        var expectedOffset = 6 + entries.Length * 16;
        foreach (var entry in entries)
        {
            Assert.Equal(expectedOffset, entry.Offset);
            Assert.InRange(entry.BytesInResource, PngSignature.Length, ico.Length - entry.Offset);
            Assert.Equal(PngSignature, ico.Skip(entry.Offset).Take(PngSignature.Length).ToArray());
            expectedOffset += entry.BytesInResource;
        }

        Assert.Equal(ico.Length, expectedOffset);
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
            return new TempDirectory(System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"FoldoraCliTests-{Guid.NewGuid():N}"));
        }

        public string PathFor(string relativePath)
        {
            Directory.CreateDirectory(Path);
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

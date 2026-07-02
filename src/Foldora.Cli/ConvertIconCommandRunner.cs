using Foldora.Imaging;
using Foldora.Imaging.Windows;

namespace Foldora.Cli;

public sealed class ConvertIconCommandRunner
{
    private static readonly string[] SupportedInputExtensions = [".png", ".jpg", ".jpeg", ".bmp"];

    public int Run(string inputPath, string outputPath, bool force, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (!TryValidate(inputPath, outputPath, force, out var resolvedInputPath, out var resolvedOutputPath, out var validationError))
        {
            error.WriteLine($"Error: {validationError}");
            return 1;
        }

        var outputDirectory = Path.GetDirectoryName(resolvedOutputPath)!;
        var tempPath = Path.Combine(
            outputDirectory,
            $".{Path.GetFileName(resolvedOutputPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            IconConversionResult result;
            using (var inputStream = File.OpenRead(resolvedInputPath))
            using (var outputStream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                result = new WindowsImageToIconConverter().Convert(inputStream, outputStream);
                outputStream.Flush();
            }

            File.Move(tempPath, resolvedOutputPath, overwrite: force);

            output.WriteLine("Converted image to ICO.");
            output.WriteLine($"Input:  {resolvedInputPath}");
            output.WriteLine($"Output: {resolvedOutputPath}");
            output.WriteLine($"Frames: {string.Join(", ", result.GeneratedFrameSizes.Select(size => size.Size))}");
            output.WriteLine($"Source: {result.SourceWidth}x{result.SourceHeight}");

            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException
                                          or UnauthorizedAccessException
                                          or IOException
                                          or NotSupportedException
                                          or System.Security.SecurityException)
        {
            DeleteTempFileIfExists(tempPath);
            error.WriteLine($"Error: {exception.Message}");
            return 1;
        }
        catch
        {
            DeleteTempFileIfExists(tempPath);
            throw;
        }
    }

    private static bool TryValidate(
        string inputPath,
        string outputPath,
        bool force,
        out string resolvedInputPath,
        out string resolvedOutputPath,
        out string error)
    {
        resolvedInputPath = string.Empty;
        resolvedOutputPath = string.Empty;

        if (string.IsNullOrWhiteSpace(inputPath))
        {
            error = "Missing required option --input.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            error = "Missing required option --output.";
            return false;
        }

        resolvedInputPath = Path.GetFullPath(inputPath);
        resolvedOutputPath = Path.GetFullPath(outputPath);

        if (Directory.Exists(resolvedInputPath))
        {
            error = $"Input path must be a file, not a directory: {resolvedInputPath}";
            return false;
        }

        if (!File.Exists(resolvedInputPath))
        {
            error = $"Input file does not exist: {resolvedInputPath}";
            return false;
        }

        var inputExtension = Path.GetExtension(resolvedInputPath);
        if (!SupportedInputExtensions.Contains(inputExtension, StringComparer.OrdinalIgnoreCase))
        {
            error = "Input file must be a PNG, JPG, JPEG or BMP image.";
            return false;
        }

        if (!string.Equals(Path.GetExtension(resolvedOutputPath), ".ico", StringComparison.OrdinalIgnoreCase))
        {
            error = "Output file extension must be .ico.";
            return false;
        }

        var outputDirectory = Path.GetDirectoryName(resolvedOutputPath);
        if (string.IsNullOrWhiteSpace(outputDirectory) || !Directory.Exists(outputDirectory))
        {
            error = $"Output directory does not exist: {outputDirectory}";
            return false;
        }

        if (string.Equals(resolvedInputPath, resolvedOutputPath, StringComparison.OrdinalIgnoreCase))
        {
            error = "Input and output paths must be different.";
            return false;
        }

        if (File.Exists(resolvedOutputPath) && !force)
        {
            error = $"Output file already exists: {resolvedOutputPath}. Re-run with --force to overwrite.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static void DeleteTempFileIfExists(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch
        {
            // Best-effort cleanup: команда уже сообщает исходную ошибку конвертации или записи.
        }
    }
}

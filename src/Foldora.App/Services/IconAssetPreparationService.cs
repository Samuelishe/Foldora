using System.IO;
using System.Security.Cryptography;
using System.Text;
using Foldora.Core.Storage;
using Foldora.Imaging.Windows;

namespace Foldora.App.Services;

/// <summary>
/// App-level подготовка выбранных иконок: .ico оставляет для staged import, raster images конвертирует в generated .ico.
/// </summary>
public sealed class IconAssetPreparationService : IIconAssetPreparationService
{
    private static readonly string[] SupportedRasterExtensions = [".png", ".jpg", ".jpeg", ".bmp"];
    private readonly FoldoraDataPaths paths;
    private readonly WindowsImageToIconConverter converter;

    public IconAssetPreparationService(FoldoraDataPaths paths, WindowsImageToIconConverter? converter = null)
    {
        this.paths = paths ?? throw new ArgumentNullException(nameof(paths));
        this.converter = converter ?? new WindowsImageToIconConverter();
    }

    public async Task<IconAssetPreparationResult> PrepareAsync(string selectedFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedFilePath);

        var sourcePath = Path.GetFullPath(selectedFilePath);
        if (Directory.Exists(sourcePath))
        {
            throw new ArgumentException("Selected icon path must be a file, not a directory.", nameof(selectedFilePath));
        }

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Selected icon file does not exist.", sourcePath);
        }

        var extension = Path.GetExtension(sourcePath);
        if (string.Equals(extension, ".ico", StringComparison.OrdinalIgnoreCase))
        {
            return IconAssetPreparationResult.ExistingIcon(sourcePath);
        }

        if (!SupportedRasterExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Selected file must be an ICO, PNG, JPG, JPEG or BMP file.", nameof(selectedFilePath));
        }

        return IconAssetPreparationResult.GeneratedIcon(await ConvertRasterToGeneratedIconAsync(sourcePath, cancellationToken));
    }

    private async Task<string> ConvertRasterToGeneratedIconAsync(string sourcePath, CancellationToken cancellationToken)
    {
        var generatedDirectory = Path.Combine(paths.IconsDirectory, "generated");
        Directory.CreateDirectory(generatedDirectory);

        var hash = await ComputeShortContentHashAsync(sourcePath, cancellationToken);
        var baseName = SanitizeFileName(Path.GetFileNameWithoutExtension(sourcePath));
        var finalPath = Path.Combine(generatedDirectory, $"{baseName}-{hash}.ico");
        if (File.Exists(finalPath))
        {
            return finalPath;
        }

        var tempPath = Path.Combine(generatedDirectory, $".{Path.GetFileName(finalPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var input = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            await using (var output = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                converter.Convert(input, output);
                await output.FlushAsync(cancellationToken);
            }

            File.Move(tempPath, finalPath, overwrite: false);
            return finalPath;
        }
        catch
        {
            DeleteTempFileIfExists(tempPath);
            throw;
        }
    }

    private static async Task<string> ComputeShortContentHashAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash.AsSpan(0, 4));
    }

    private static string SanitizeFileName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars().ToHashSet();
        var builder = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var character in value)
        {
            var replacement = invalidCharacters.Contains(character) || char.IsControl(character)
                ? '-'
                : character;

            if (char.IsWhiteSpace(replacement))
            {
                replacement = '-';
            }

            if (replacement == '-')
            {
                if (previousWasSeparator)
                {
                    continue;
                }

                previousWasSeparator = true;
            }
            else
            {
                previousWasSeparator = false;
            }

            builder.Append(replacement);
            if (builder.Length >= 64)
            {
                break;
            }
        }

        var sanitized = builder.ToString().Trim(' ', '.', '-');
        return string.IsNullOrWhiteSpace(sanitized) ? "icon" : sanitized;
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
            // Best-effort cleanup: пользователь увидит исходную ошибку чтения, конвертации или записи.
        }
    }
}

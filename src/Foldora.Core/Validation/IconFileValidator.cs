namespace Foldora.Core.Validation;

/// <summary>
/// Проверяет, что файл является структурно безопасным ICO без декодирования изображений.
/// </summary>
public sealed class IconFileValidator
{
    public const long MaxIconFileSizeBytes = 10 * 1024 * 1024;
    public const ushort MaxImageCount = 64;

    public void EnsureValid(string iconPath)
    {
        var result = Validate(iconPath);
        if (result.IsValid)
        {
            return;
        }

        throw new InvalidOperationException(result.Issues.First().Message);
    }

    public FolderMenuValidationResult Validate(string iconPath)
    {
        if (string.IsNullOrWhiteSpace(iconPath))
        {
            return Error("Icon path must not be empty.", FolderMenuValidationIssueCodes.IconPathEmpty);
        }

        var file = new FileInfo(iconPath);
        if (!file.Exists)
        {
            return Error(
                $"Icon file was not found: {file.FullName}",
                FolderMenuValidationIssueCodes.IconMissing,
                new Dictionary<string, string>
                {
                    ["filePath"] = file.FullName
                });
        }

        if (!string.Equals(file.Extension, ".ico", StringComparison.OrdinalIgnoreCase))
        {
            return Error(
                "Foldora supports only .ico files.",
                FolderMenuValidationIssueCodes.IconExtension,
                new Dictionary<string, string>
                {
                    ["extension"] = string.IsNullOrEmpty(file.Extension) ? "<none>" : file.Extension
                });
        }

        if (file.Length == 0)
        {
            return Error(
                "Icon file must not be empty.",
                FolderMenuValidationIssueCodes.IconEmpty,
                new Dictionary<string, string>
                {
                    ["filePath"] = file.FullName
                });
        }

        if (file.Length > MaxIconFileSizeBytes)
        {
            return Error(
                "Icon file must be 10 MB or smaller.",
                FolderMenuValidationIssueCodes.IconTooLarge,
                new Dictionary<string, string>
                {
                    ["maxBytes"] = MaxIconFileSizeBytes.ToString(),
                    ["actualBytes"] = file.Length.ToString()
                });
        }

        try
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            return ValidateIcoStructure(stream, file.Length);
        }
        catch (UnauthorizedAccessException)
        {
            return Error(
                "Icon file is not readable.",
                FolderMenuValidationIssueCodes.IconNotReadable,
                new Dictionary<string, string>
                {
                    ["filePath"] = file.FullName
                });
        }
        catch (IOException exception)
        {
            return Error(
                $"Icon file could not be read: {exception.Message}",
                FolderMenuValidationIssueCodes.IconReadFailed,
                new Dictionary<string, string>
                {
                    ["filePath"] = file.FullName,
                    ["error"] = exception.Message
                });
        }
    }

    private static FolderMenuValidationResult ValidateIcoStructure(Stream stream, long fileLength)
    {
        Span<byte> header = stackalloc byte[6];
        if (!ReadExactly(stream, header))
        {
            return Error("Icon file is too small for an ICO header.", FolderMenuValidationIssueCodes.IconHeaderTooSmall);
        }

        var reserved = ReadUInt16LittleEndian(header[..2]);
        var type = ReadUInt16LittleEndian(header[2..4]);
        var imageCount = ReadUInt16LittleEndian(header[4..6]);

        if (reserved != 0 || type != 1)
        {
            return Error("Icon file has an invalid ICO header.", FolderMenuValidationIssueCodes.IconHeaderInvalid);
        }

        if (imageCount == 0 || imageCount > MaxImageCount)
        {
            return Error(
                "Icon file has an invalid image count.",
                FolderMenuValidationIssueCodes.IconImageCountInvalid,
                new Dictionary<string, string>
                {
                    ["count"] = imageCount.ToString(),
                    ["limit"] = MaxImageCount.ToString()
                });
        }

        var directoryLength = 6L + imageCount * 16L;
        if (directoryLength > fileLength)
        {
            return Error("Icon directory entries do not fit inside the file.", FolderMenuValidationIssueCodes.IconDirectoryOutOfBounds);
        }

        Span<byte> entryBuffer = stackalloc byte[16];
        for (var index = 0; index < imageCount; index++)
        {
            if (!ReadExactly(stream, entryBuffer))
            {
                return Error(
                    "Icon directory entry is incomplete.",
                    FolderMenuValidationIssueCodes.IconDirectoryEntryIncomplete,
                    new Dictionary<string, string>
                    {
                        ["index"] = index.ToString()
                    });
            }

            var imageSize = ReadUInt32LittleEndian(entryBuffer[8..12]);
            var imageOffset = ReadUInt32LittleEndian(entryBuffer[12..16]);

            if (imageSize == 0)
            {
                return Error(
                    "Icon image data size must not be zero.",
                    FolderMenuValidationIssueCodes.IconImageEmpty,
                    new Dictionary<string, string>
                    {
                        ["index"] = index.ToString()
                    });
            }

            if (imageOffset < directoryLength || imageOffset >= fileLength)
            {
                return Error(
                    "Icon image data offset is outside the file.",
                    FolderMenuValidationIssueCodes.IconImageOffsetInvalid,
                    new Dictionary<string, string>
                    {
                        ["index"] = index.ToString()
                    });
            }

            if (imageOffset + imageSize > fileLength)
            {
                return Error(
                    "Icon image data extends past the end of the file.",
                    FolderMenuValidationIssueCodes.IconImageDataOutOfBounds,
                    new Dictionary<string, string>
                    {
                        ["index"] = index.ToString()
                    });
            }
        }

        return FolderMenuValidationResult.Success;
    }

    private static bool ReadExactly(Stream stream, Span<byte> buffer)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = stream.Read(buffer[totalRead..]);
            if (read == 0)
            {
                return false;
            }

            totalRead += read;
        }

        return true;
    }

    private static ushort ReadUInt16LittleEndian(ReadOnlySpan<byte> bytes)
    {
        return (ushort)(bytes[0] | (bytes[1] << 8));
    }

    private static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> bytes)
    {
        return (uint)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24));
    }

    private static FolderMenuValidationResult Error(
        string message,
        string code,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        return new FolderMenuValidationResult(
            [new FolderMenuValidationIssue(FolderMenuValidationSeverity.Error, message, code, parameters: parameters)]);
    }
}

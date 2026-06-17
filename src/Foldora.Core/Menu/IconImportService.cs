using Foldora.Core.Storage;

namespace Foldora.Core.Menu;

/// <summary>
/// Копирует выбранные пользователем .ico-файлы в AppData Foldora.
/// </summary>
public sealed class IconImportService
{
    public async Task<IconImportResult> ImportAsync(
        string sourceIconPath,
        FoldoraDataPaths paths,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceIconPath);
        ArgumentNullException.ThrowIfNull(paths);

        var sourceIcon = new FileInfo(sourceIconPath);
        if (!sourceIcon.Exists)
        {
            throw new FileNotFoundException($"Icon file was not found: {sourceIcon.FullName}", sourceIcon.FullName);
        }

        if (!string.Equals(sourceIcon.Extension, ".ico", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Foldora supports only .ico files for menu entries.");
        }

        Directory.CreateDirectory(paths.IconsDirectory);

        var entryId = CreateEntryId();
        var importedIconPath = Path.Combine(paths.IconsDirectory, $"{entryId}.ico");

        await using var sourceStream = sourceIcon.OpenRead();
        await using var destinationStream = new FileStream(
            importedIconPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);

        await sourceStream.CopyToAsync(destinationStream, cancellationToken);

        return new IconImportResult(entryId, importedIconPath);
    }

    private static string CreateEntryId()
    {
        return $"entry-{Guid.NewGuid():N}";
    }
}

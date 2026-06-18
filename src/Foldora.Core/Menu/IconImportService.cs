using Foldora.Core.Storage;
using Foldora.Core.Validation;

namespace Foldora.Core.Menu;

/// <summary>
/// Копирует выбранные пользователем .ico-файлы в AppData Foldora.
/// </summary>
public sealed class IconImportService
{
    private readonly IconFileValidator iconFileValidator;

    public IconImportService(IconFileValidator? iconFileValidator = null)
    {
        this.iconFileValidator = iconFileValidator ?? new IconFileValidator();
    }

    public async Task<IconImportResult> ImportAsync(
        string sourceIconPath,
        FoldoraDataPaths paths,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceIconPath);
        ArgumentNullException.ThrowIfNull(paths);

        iconFileValidator.EnsureValid(sourceIconPath);

        var sourceIcon = new FileInfo(sourceIconPath);
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

    public async Task<string> ImportForEntryAsync(
        string sourceIconPath,
        FoldoraDataPaths paths,
        string entryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceIconPath);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        iconFileValidator.EnsureValid(sourceIconPath);

        var sourceIcon = new FileInfo(sourceIconPath);
        Directory.CreateDirectory(paths.IconsDirectory);

        var importedIconPath = Path.Combine(paths.IconsDirectory, $"{entryId}.ico");

        await using var sourceStream = sourceIcon.OpenRead();
        await using var destinationStream = new FileStream(
            importedIconPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await sourceStream.CopyToAsync(destinationStream, cancellationToken);

        return importedIconPath;
    }

    private static string CreateEntryId()
    {
        return $"entry-{Guid.NewGuid():N}";
    }
}

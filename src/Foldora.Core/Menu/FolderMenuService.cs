using Foldora.Core.Settings;
using Foldora.Core.Storage;

namespace Foldora.Core.Menu;

/// <summary>
/// Управляет пользовательскими пунктами меню создания папки.
/// </summary>
public sealed class FolderMenuService
{
    private readonly FoldoraSettingsStorage storage;
    private readonly FoldoraDataPaths paths;
    private readonly IconImportService iconImportService;

    public FolderMenuService(
        FoldoraSettingsStorage storage,
        FoldoraDataPaths paths,
        IconImportService? iconImportService = null)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.paths = paths ?? throw new ArgumentNullException(nameof(paths));
        this.iconImportService = iconImportService ?? new IconImportService();
    }

    public async Task<IReadOnlyList<FolderMenuEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        var settings = await storage.LoadAsync(cancellationToken);
        return settings.CreateFolderMenu.Entries
            .OrderBy(entry => entry.SortOrder)
            .ThenBy(entry => entry.DisplayName, StringComparer.CurrentCulture)
            .ToArray();
    }

    public async Task<FolderMenuEntry> AddAsync(
        string sourceIconPath,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        var settings = await storage.LoadAsync(cancellationToken);
        var importedIcon = await iconImportService.ImportAsync(sourceIconPath, paths, cancellationToken);
        var resolvedDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? FolderMenuNameGenerator.GetNextName(settings.CreateFolderMenu.Entries)
            : displayName.Trim();

        var entry = new FolderMenuEntry
        {
            Id = importedIcon.EntryId,
            DisplayName = resolvedDisplayName,
            IconPath = importedIcon.ImportedIconPath,
            SortOrder = GetNextSortOrder(settings.CreateFolderMenu.Entries),
            IsEnabled = true
        };

        settings.CreateFolderMenu.Entries.Add(entry);
        await storage.SaveAsync(settings, cancellationToken);
        return entry;
    }

    public async Task<FolderMenuEntry> RemoveAsync(
        string entryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        var settings = await storage.LoadAsync(cancellationToken);
        var entry = settings.CreateFolderMenu.Entries.FirstOrDefault(
            item => string.Equals(item.Id, entryId, StringComparison.OrdinalIgnoreCase));

        if (entry is null)
        {
            throw new InvalidOperationException($"Menu entry was not found: {entryId}");
        }

        settings.CreateFolderMenu.Entries.Remove(entry);
        DeleteImportedIconIfOwned(entry);
        await storage.SaveAsync(settings, cancellationToken);
        return entry;
    }

    private void DeleteImportedIconIfOwned(FolderMenuEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.IconPath) || !File.Exists(entry.IconPath))
        {
            return;
        }

        var iconsRoot = Path.GetFullPath(paths.IconsDirectory);
        var iconPath = Path.GetFullPath(entry.IconPath);
        var expectedPath = Path.GetFullPath(Path.Combine(paths.IconsDirectory, $"{entry.Id}.ico"));

        if (!string.Equals(iconPath, expectedPath, StringComparison.OrdinalIgnoreCase)
            || !iconPath.StartsWith(iconsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        File.Delete(iconPath);
    }

    private static int GetNextSortOrder(IEnumerable<FolderMenuEntry> entries)
    {
        return entries.Any() ? entries.Max(entry => entry.SortOrder) + 1 : 0;
    }
}

using Foldora.Core.Settings;

namespace Foldora.Core.Menu;

/// <summary>
/// Находит сохранённый пользовательский пункт меню и проверяет его готовность к действию.
/// </summary>
public sealed class FolderMenuEntryResolver
{
    private readonly FoldoraSettingsStorage storage;

    public FolderMenuEntryResolver(FoldoraSettingsStorage storage)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public async Task<FolderMenuEntry> ResolveEnabledAsync(
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

        if (!entry.IsEnabled)
        {
            throw new InvalidOperationException($"Menu entry is disabled: {entryId}");
        }

        if (string.IsNullOrWhiteSpace(entry.IconPath))
        {
            throw new InvalidOperationException($"Menu entry has no icon path: {entryId}");
        }

        if (!File.Exists(entry.IconPath))
        {
            throw new FileNotFoundException($"Menu entry icon file was not found: {entry.IconPath}", entry.IconPath);
        }

        return entry;
    }
}

using Foldora.Core.DesktopIni;
using Foldora.Core.Settings;

namespace Foldora.Core.Menu;

/// <summary>
/// Выполняет действия по сохранённым пользовательским пунктам меню.
/// </summary>
public sealed class FolderMenuEntryActionService
{
    private readonly FolderMenuEntryResolver resolver;
    private readonly DesktopIniService desktopIniService;
    private readonly UniqueFolderNameService uniqueFolderNameService;

    public FolderMenuEntryActionService(
        FoldoraSettingsStorage storage,
        DesktopIniService? desktopIniService = null,
        UniqueFolderNameService? uniqueFolderNameService = null)
    {
        ArgumentNullException.ThrowIfNull(storage);

        resolver = new FolderMenuEntryResolver(storage);
        this.desktopIniService = desktopIniService ?? new DesktopIniService();
        this.uniqueFolderNameService = uniqueFolderNameService ?? new UniqueFolderNameService();
    }

    public async Task ApplyAsync(
        string folderPath,
        string entryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

        var entry = await resolver.ResolveEnabledAsync(entryId, cancellationToken);
        await desktopIniService.ApplyIconAsync(
            new DesktopIniOptions(folderPath, entry.IconPath),
            cancellationToken);
    }

    public async Task<string> CreateAsync(
        string targetDirectory,
        string entryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDirectory);

        var entry = await resolver.ResolveEnabledAsync(entryId, cancellationToken);
        var folderPath = uniqueFolderNameService.GetAvailableDirectoryPath(
            targetDirectory,
            entry.DefaultFolderName);

        Directory.CreateDirectory(folderPath);

        try
        {
            await desktopIniService.ApplyIconAsync(
                new DesktopIniOptions(folderPath, entry.IconPath),
                cancellationToken);
        }
        catch
        {
            if (!Directory.EnumerateFileSystemEntries(folderPath).Any())
            {
                Directory.Delete(folderPath);
            }

            throw;
        }

        return folderPath;
    }
}

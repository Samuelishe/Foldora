using Foldora.Core.Menu;

namespace Foldora.MenuHost;

internal interface IMenuHostFolderActionService
{
    Task<string> CreateAsync(string targetDirectory, string entryId, CancellationToken cancellationToken = default);

    Task ApplyAsync(string folderPath, string entryId, CancellationToken cancellationToken = default);
}

internal sealed class MenuHostFolderActionService : IMenuHostFolderActionService
{
    private readonly FolderMenuEntryActionService inner;

    public MenuHostFolderActionService(FolderMenuEntryActionService inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        this.inner = inner;
    }

    public async Task<string> CreateAsync(
        string targetDirectory,
        string entryId,
        CancellationToken cancellationToken = default)
    {
        return await inner.CreateAsync(targetDirectory, entryId, cancellationToken);
    }

    public async Task ApplyAsync(
        string folderPath,
        string entryId,
        CancellationToken cancellationToken = default)
    {
        await inner.ApplyAsync(folderPath, entryId, cancellationToken);
    }
}

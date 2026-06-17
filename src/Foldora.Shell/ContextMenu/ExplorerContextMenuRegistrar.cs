namespace Foldora.Shell.ContextMenu;

/// <summary>
/// Skeleton-регистратор legacy context menu. Реальные HKCU-записи будут добавлены отдельным этапом.
/// </summary>
public sealed class ExplorerContextMenuRegistrar
{
    public const string DirectoryShellPath = @"Software\Classes\Directory\shell";
    public const string DirectoryBackgroundShellPath = @"Software\Classes\Directory\Background\shell";

    public string BuildApplyCommand(string cliExecutablePath, string folderPlaceholder, string styleId)
    {
        return CommandLineQuoter.Join(cliExecutablePath, "apply", "--folder", folderPlaceholder, "--style", styleId);
    }

    public Task RegisterAsync(
        ExplorerContextMenuRegistrationOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotImplementedException("Legacy HKCU context menu registration is planned after bootstrap.");
    }

    public Task UnregisterAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotImplementedException("Legacy HKCU context menu unregistration is planned after bootstrap.");
    }
}

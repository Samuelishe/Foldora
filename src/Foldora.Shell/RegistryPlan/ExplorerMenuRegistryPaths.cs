namespace Foldora.Shell.RegistryPlan;

/// <summary>
/// Foldora-owned registry paths для legacy context menu.
/// </summary>
public static class ExplorerMenuRegistryPaths
{
    public const string DirectoryRoot = @"Software\Classes\Directory\shell\Foldora";
    public const string DirectoryBackgroundRoot = @"Software\Classes\Directory\Background\shell\Foldora";

    public static string GetOwnedRoot(ExplorerMenuTargetKind targetKind)
    {
        return targetKind switch
        {
            ExplorerMenuTargetKind.Directory => DirectoryRoot,
            ExplorerMenuTargetKind.DirectoryBackground => DirectoryBackgroundRoot,
            _ => throw new ArgumentOutOfRangeException(nameof(targetKind), targetKind, null)
        };
    }

    public static bool IsInsideOwnedRoot(string keyPath)
    {
        return IsInsideRoot(keyPath, DirectoryRoot) || IsInsideRoot(keyPath, DirectoryBackgroundRoot);
    }

    private static bool IsInsideRoot(string keyPath, string root)
    {
        return string.Equals(keyPath, root, StringComparison.OrdinalIgnoreCase)
               || keyPath.StartsWith(root + "\\", StringComparison.OrdinalIgnoreCase);
    }
}

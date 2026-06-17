namespace Foldora.Shell.RegistryPlan;

/// <summary>
/// Placeholder policy для Explorer target path. Требует ручной проверки перед registry writer.
/// </summary>
public static class ExplorerMenuShellTargetPlaceholder
{
    public static string GetPlaceholder(ExplorerMenuTargetKind targetKind)
    {
        return targetKind switch
        {
            ExplorerMenuTargetKind.Directory => "%1",
            ExplorerMenuTargetKind.DirectoryBackground => "%V",
            _ => throw new ArgumentOutOfRangeException(nameof(targetKind), targetKind, null)
        };
    }
}

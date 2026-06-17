namespace Foldora.Shell.RegistryPlan;

/// <summary>
/// Testable plan будущих HKCU registry-изменений для legacy context menu.
/// </summary>
public sealed class ExplorerMenuRegistryPlan
{
    public ExplorerMenuRegistryPlan(
        ExplorerMenuTargetKind targetKind,
        IEnumerable<ExplorerMenuRegistryDeleteOperation> deleteOperations,
        IEnumerable<ExplorerMenuRegistryKeyOperation> keyOperations,
        IEnumerable<ExplorerMenuRegistryValueOperation> valueOperations)
    {
        TargetKind = targetKind;
        DeleteOperations = deleteOperations.ToArray();
        KeyOperations = keyOperations.ToArray();
        ValueOperations = valueOperations.ToArray();
    }

    public ExplorerMenuTargetKind TargetKind { get; }

    public IReadOnlyList<ExplorerMenuRegistryDeleteOperation> DeleteOperations { get; }

    public IReadOnlyList<ExplorerMenuRegistryKeyOperation> KeyOperations { get; }

    public IReadOnlyList<ExplorerMenuRegistryValueOperation> ValueOperations { get; }
}

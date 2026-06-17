namespace Foldora.Shell.RegistryPlan;

/// <summary>
/// Планируемое удаление Foldora-owned registry key.
/// </summary>
public sealed record ExplorerMenuRegistryDeleteOperation(
    ExplorerMenuRegistryHive Hive,
    string KeyPath);

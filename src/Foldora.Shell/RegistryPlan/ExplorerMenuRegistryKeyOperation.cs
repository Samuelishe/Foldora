namespace Foldora.Shell.RegistryPlan;

/// <summary>
/// Планируемое создание registry key.
/// </summary>
public sealed record ExplorerMenuRegistryKeyOperation(
    ExplorerMenuRegistryHive Hive,
    string KeyPath);

namespace Foldora.Shell.RegistryPlan;

/// <summary>
/// Планируемая запись registry value.
/// </summary>
public sealed record ExplorerMenuRegistryValueOperation(
    ExplorerMenuRegistryHive Hive,
    string KeyPath,
    string ValueName,
    string ValueData);

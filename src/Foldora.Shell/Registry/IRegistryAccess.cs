using Foldora.Shell.RegistryPlan;

namespace Foldora.Shell.Registry;

/// <summary>
/// Минимальный доступ к registry для применения validated plan.
/// </summary>
public interface IRegistryAccess
{
    void DeleteTreeIfExists(ExplorerMenuRegistryHive hive, string keyPath);

    void CreateKey(ExplorerMenuRegistryHive hive, string keyPath);

    void SetStringValue(ExplorerMenuRegistryHive hive, string keyPath, string valueName, string valueData);
}

using Foldora.Shell.RegistryPlan;
using Microsoft.Win32;
using System.Runtime.Versioning;
using Win32Registry = Microsoft.Win32.Registry;

namespace Foldora.Shell.Registry;

/// <summary>
/// Реальный доступ к Windows Registry. Использовать только через writer.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsRegistryAccess : IRegistryAccess
{
    public void DeleteTreeIfExists(ExplorerMenuRegistryHive hive, string keyPath)
    {
        var root = GetRootKey(hive);
        root.DeleteSubKeyTree(keyPath, throwOnMissingSubKey: false);
    }

    public void CreateKey(ExplorerMenuRegistryHive hive, string keyPath)
    {
        var root = GetRootKey(hive);
        using var key = root.CreateSubKey(keyPath, writable: true);
        if (key is null)
        {
            throw new InvalidOperationException($"Registry key could not be created: {hive}\\{keyPath}");
        }
    }

    public void SetStringValue(ExplorerMenuRegistryHive hive, string keyPath, string valueName, string valueData)
    {
        var root = GetRootKey(hive);
        using var key = root.CreateSubKey(keyPath, writable: true);
        if (key is null)
        {
            throw new InvalidOperationException($"Registry key could not be opened for writing: {hive}\\{keyPath}");
        }

        key.SetValue(valueName, valueData, RegistryValueKind.String);
    }

    private static RegistryKey GetRootKey(ExplorerMenuRegistryHive hive)
    {
        return hive switch
        {
            ExplorerMenuRegistryHive.CurrentUser => Win32Registry.CurrentUser,
            _ => throw new InvalidOperationException($"Unsupported registry hive for Foldora shell integration: {hive}")
        };
    }
}

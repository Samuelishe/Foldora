using Foldora.Shell.Registry;
using Foldora.Shell.RegistryPlan;

namespace Foldora.Tests.Shell.Fakes;

internal sealed class FakeRegistryAccess : IRegistryAccess
{
    public HashSet<string> Keys { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<(ExplorerMenuRegistryHive Hive, string KeyPath, string ValueName), string> Values { get; } = [];

    public List<string> Calls { get; } = [];

    public void DeleteTreeIfExists(ExplorerMenuRegistryHive hive, string keyPath)
    {
        Calls.Add($"delete:{hive}\\{keyPath}");
        var prefix = $"{hive}\\{keyPath}";

        Keys.RemoveWhere(key => string.Equals(key, prefix, StringComparison.OrdinalIgnoreCase)
                                || key.StartsWith(prefix + "\\", StringComparison.OrdinalIgnoreCase));

        foreach (var key in Values.Keys.ToArray())
        {
            var fullKey = $"{key.Hive}\\{key.KeyPath}";
            if (string.Equals(fullKey, prefix, StringComparison.OrdinalIgnoreCase)
                || fullKey.StartsWith(prefix + "\\", StringComparison.OrdinalIgnoreCase))
            {
                Values.Remove(key);
            }
        }
    }

    public void CreateKey(ExplorerMenuRegistryHive hive, string keyPath)
    {
        Calls.Add($"create:{hive}\\{keyPath}");
        Keys.Add($"{hive}\\{keyPath}");
    }

    public void SetStringValue(ExplorerMenuRegistryHive hive, string keyPath, string valueName, string valueData)
    {
        Calls.Add($"set:{hive}\\{keyPath}\\{valueName}");
        Keys.Add($"{hive}\\{keyPath}");
        Values[(hive, keyPath, valueName)] = valueData;
    }

    public bool ContainsKey(string keyPath)
    {
        return Keys.Contains($"{ExplorerMenuRegistryHive.CurrentUser}\\{keyPath}");
    }
}

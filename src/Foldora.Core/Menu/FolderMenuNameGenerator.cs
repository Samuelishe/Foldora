namespace Foldora.Core.Menu;

/// <summary>
/// Генерирует свободные fallback-имена для пользовательских пунктов меню.
/// </summary>
public static class FolderMenuNameGenerator
{
    private const string Prefix = "Вид";

    public static string GetNextName(IEnumerable<FolderMenuEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var usedNames = entries
            .Select(entry => entry.DisplayName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var number = 1; ; number++)
        {
            var candidate = $"{Prefix} {number}";
            if (!usedNames.Contains(candidate))
            {
                return candidate;
            }
        }
    }
}

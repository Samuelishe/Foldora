using Foldora.Core.Menu;

namespace Foldora.Core.Validation;

/// <summary>
/// Проверяет текущую flat-модель пользовательского меню.
/// </summary>
public sealed class FolderMenuSettingsValidator
{
    public const int MaxChildrenPerGroup = 30;
    public const int MaxEnabledEntries = 50;
    public const int MaxGroups = 30;
    public const int MaxTotalEntries = 100;

    private readonly FolderMenuEntryValidator entryValidator = new();

    public FolderMenuValidationResult Validate(FolderMenuSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var issues = new List<FolderMenuValidationIssue>();
        var totalEntries = settings.Entries.Count;
        var enabledEntries = settings.Entries.Count(entry => entry.IsEnabled);

        if (totalEntries > MaxTotalEntries)
        {
            issues.Add(Error($"Menu can contain at most {MaxTotalEntries} entries.", "menu_total_entries_limit"));
        }

        if (enabledEntries > MaxEnabledEntries)
        {
            issues.Add(Error($"Menu can contain at most {MaxEnabledEntries} enabled entries.", "menu_enabled_entries_limit"));
        }

        var enabledGroups = settings.Entries
            .Where(entry => entry.IsEnabled)
            .Select(entry => GroupNameValidator.Normalize(entry.GroupName))
            .Where(groupName => groupName.Length > 0)
            .GroupBy(groupName => groupName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (enabledGroups.Length > MaxGroups)
        {
            issues.Add(Error($"Menu can contain at most {MaxGroups} groups.", "menu_group_limit"));
        }

        foreach (var group in enabledGroups)
        {
            if (group.Count() > MaxChildrenPerGroup)
            {
                issues.Add(Error($"Group '{group.Key}' can contain at most {MaxChildrenPerGroup} enabled entries.", "menu_group_children_limit"));
            }
        }

        foreach (var entry in settings.Entries)
        {
            issues.AddRange(entryValidator.Validate(entry).Issues);
        }

        return issues.Count == 0 ? FolderMenuValidationResult.Success : new FolderMenuValidationResult(issues);
    }

    private static FolderMenuValidationIssue Error(string message, string code)
    {
        return new FolderMenuValidationIssue(FolderMenuValidationSeverity.Error, message, code);
    }
}

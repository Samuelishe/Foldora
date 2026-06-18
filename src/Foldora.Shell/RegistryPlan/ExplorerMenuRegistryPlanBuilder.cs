using Foldora.Core.Menu;
using Foldora.Core.Validation;

namespace Foldora.Shell.RegistryPlan;

/// <summary>
/// Строит testable registry plan для будущего legacy context menu.
/// </summary>
public sealed class ExplorerMenuRegistryPlanBuilder
{
    private const string DefaultCreateFolderMenuTitle = "Создать папку";

    private readonly ExplorerMenuCommandBuilder commandBuilder;
    private readonly ExplorerMenuRegistryPlanValidator validator;

    public ExplorerMenuRegistryPlanBuilder(
        ExplorerMenuCommandBuilder? commandBuilder = null,
        ExplorerMenuRegistryPlanValidator? validator = null)
    {
        this.commandBuilder = commandBuilder ?? new ExplorerMenuCommandBuilder();
        this.validator = validator ?? new ExplorerMenuRegistryPlanValidator();
    }

    public ExplorerMenuRegistryPlan Build(
        string commandHostPath,
        FolderMenuSettings menuSettings,
        ExplorerMenuTargetKind targetKind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandHostPath);
        ArgumentNullException.ThrowIfNull(menuSettings);

        var root = ExplorerMenuRegistryPaths.GetOwnedRoot(targetKind);
        var deletes = new List<ExplorerMenuRegistryDeleteOperation>
        {
            new(ExplorerMenuRegistryHive.CurrentUser, root)
        };

        var enabledEntries = menuSettings.Entries
            .Where(entry => entry.IsEnabled)
            .OrderBy(entry => entry.SortOrder)
            .ThenBy(entry => entry.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (enabledEntries.Length == 0)
        {
            return CreateValidatedPlan(targetKind, deletes, [], []);
        }

        var keys = new List<ExplorerMenuRegistryKeyOperation>();
        var values = new List<ExplorerMenuRegistryValueOperation>();
        var rootShellKey = $@"{root}\shell";

        AddKey(keys, root);
        AddValue(values, root, "MUIVerb", NormalizeRootMenuTitle(menuSettings.Title));
        AddValue(values, root, "SubCommands", string.Empty);

        var rootEntries = enabledEntries
            .Where(entry => GroupNameValidator.Normalize(entry.GroupName).Length == 0)
            .ToArray();
        var groups = enabledEntries
            .Where(entry => GroupNameValidator.Normalize(entry.GroupName).Length > 0)
            .GroupBy(entry => GroupNameValidator.Normalize(entry.GroupName), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Min(entry => entry.SortOrder))
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        for (var index = 0; index < rootEntries.Length; index++)
        {
            var entry = rootEntries[index];
            var entryKeyName = CreateEntryKeyName(index, entry.Id);
            var entryKey = $@"{rootShellKey}\{entryKeyName}";

            AddEntry(keys, values, commandHostPath, targetKind, entry, entryKey);
        }

        for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            var group = groups[groupIndex];
            var groupKey = $@"{rootShellKey}\{CreateGroupKeyName(groupIndex)}";
            var groupShellKey = $@"{groupKey}\shell";
            AddKey(keys, groupKey);
            AddValue(values, groupKey, "MUIVerb", group.Key);
            AddValue(values, groupKey, "SubCommands", string.Empty);

            var entries = group
                .OrderBy(entry => entry.SortOrder)
                .ThenBy(entry => entry.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            for (var entryIndex = 0; entryIndex < entries.Length; entryIndex++)
            {
                var entry = entries[entryIndex];
                var entryKey = $@"{groupShellKey}\{CreateEntryKeyName(entryIndex, entry.Id)}";
                AddEntry(keys, values, commandHostPath, targetKind, entry, entryKey);
            }
        }

        return CreateValidatedPlan(targetKind, deletes, keys, values);
    }

    private void AddEntry(
        ICollection<ExplorerMenuRegistryKeyOperation> keys,
        ICollection<ExplorerMenuRegistryValueOperation> values,
        string commandHostPath,
        ExplorerMenuTargetKind targetKind,
        FolderMenuEntry entry,
        string entryKey)
    {
        var commandKey = $@"{entryKey}\command";
        var command = commandBuilder.BuildCreateFolderCommand(commandHostPath, targetKind, entry.Id);

        AddKey(keys, entryKey);
        AddValue(values, entryKey, "MUIVerb", entry.DisplayName);
        AddIconValueIfAvailable(values, entryKey, entry.IconPath);
        AddKey(keys, commandKey);
        AddValue(values, commandKey, string.Empty, command);
    }

    private static string NormalizeRootMenuTitle(string? title)
    {
        return string.IsNullOrWhiteSpace(title) ? DefaultCreateFolderMenuTitle : title.Trim();
    }

    private ExplorerMenuRegistryPlan CreateValidatedPlan(
        ExplorerMenuTargetKind targetKind,
        IEnumerable<ExplorerMenuRegistryDeleteOperation> deletes,
        IEnumerable<ExplorerMenuRegistryKeyOperation> keys,
        IEnumerable<ExplorerMenuRegistryValueOperation> values)
    {
        var plan = new ExplorerMenuRegistryPlan(targetKind, deletes, keys, values);
        var validation = validator.Validate(plan);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.Errors[0]);
        }

        return plan;
    }

    private static void AddKey(ICollection<ExplorerMenuRegistryKeyOperation> operations, string keyPath)
    {
        operations.Add(new ExplorerMenuRegistryKeyOperation(ExplorerMenuRegistryHive.CurrentUser, keyPath));
    }

    private static void AddValue(
        ICollection<ExplorerMenuRegistryValueOperation> operations,
        string keyPath,
        string valueName,
        string valueData)
    {
        operations.Add(new ExplorerMenuRegistryValueOperation(
            ExplorerMenuRegistryHive.CurrentUser,
            keyPath,
            valueName,
            valueData));
    }

    private static void AddIconValueIfAvailable(
        ICollection<ExplorerMenuRegistryValueOperation> operations,
        string keyPath,
        string? iconPath)
    {
        if (string.IsNullOrWhiteSpace(iconPath) || !File.Exists(iconPath))
        {
            return;
        }

        AddValue(operations, keyPath, "Icon", iconPath);
    }

    private static string CreateEntryKeyName(int index, string entryId)
    {
        return $"entry-{index + 1:000}-{SanitizeRegistryKeySegment(entryId)}";
    }

    private static string CreateGroupKeyName(int index)
    {
        return $"group-{index + 1:000}";
    }

    private static string SanitizeRegistryKeySegment(string value)
    {
        var safeCharacters = value.Select(character =>
            char.IsAsciiLetterOrDigit(character) || character is '-' or '_'
                ? character
                : '-');

        var result = new string(safeCharacters.ToArray()).Trim('-');
        return string.IsNullOrWhiteSpace(result) ? "entry" : result;
    }
}

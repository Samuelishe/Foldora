using Foldora.Core.Menu;

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
        string cliExecutablePath,
        FolderMenuSettings menuSettings,
        ExplorerMenuTargetKind targetKind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cliExecutablePath);
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

        for (var index = 0; index < enabledEntries.Length; index++)
        {
            var entry = enabledEntries[index];
            var entryKeyName = CreateEntryKeyName(index, entry.Id);
            var entryKey = $@"{rootShellKey}\{entryKeyName}";
            var commandKey = $@"{entryKey}\command";
            var command = commandBuilder.BuildCreateFolderCommand(cliExecutablePath, targetKind, entry.Id);

            AddKey(keys, entryKey);
            AddValue(values, entryKey, "MUIVerb", entry.DisplayName);
            AddKey(keys, commandKey);
            AddValue(values, commandKey, string.Empty, command);
        }

        return CreateValidatedPlan(targetKind, deletes, keys, values);
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

    private static string CreateEntryKeyName(int index, string entryId)
    {
        return $"entry-{index + 1:000}-{SanitizeRegistryKeySegment(entryId)}";
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

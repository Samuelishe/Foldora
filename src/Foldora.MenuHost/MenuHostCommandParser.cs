namespace Foldora.MenuHost;

internal static class MenuHostCommandParser
{
    public static MenuHostCommand Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0)
        {
            return MenuHostCommand.Invalid(MenuHostCommandKind.Unknown);
        }

        var command = args[0].ToLowerInvariant();
        var options = ParseOptions(args.Skip(1).ToArray());

        return command switch
        {
            "create" => ParseCreate(options),
            "apply" => ParseApply(options),
            _ => MenuHostCommand.Invalid(MenuHostCommandKind.Unknown)
        };
    }

    private static MenuHostCommand ParseCreate(IReadOnlyDictionary<string, string?> options)
    {
        return !TryGetRequiredOption(options, "--target", out var targetPath)
               || !TryGetRequiredOption(options, "--entry-id", out var entryId)
            ? MenuHostCommand.Invalid(MenuHostCommandKind.Create)
            : new MenuHostCommand(MenuHostCommandKind.Create, TargetPath: targetPath, EntryId: entryId);
    }

    private static MenuHostCommand ParseApply(IReadOnlyDictionary<string, string?> options)
    {
        return !TryGetRequiredOption(options, "--folder", out var folderPath)
               || !TryGetRequiredOption(options, "--entry-id", out var entryId)
            ? MenuHostCommand.Invalid(MenuHostCommandKind.Apply)
            : new MenuHostCommand(MenuHostCommandKind.Apply, FolderPath: folderPath, EntryId: entryId);
    }

    private static Dictionary<string, string?> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < args.Length; index++)
        {
            var option = args[index];
            if (!option.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var value = index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal)
                ? args[++index]
                : null;

            options[option] = value;
        }

        return options;
    }

    private static bool TryGetRequiredOption(
        IReadOnlyDictionary<string, string?> options,
        string name,
        out string value)
    {
        if (options.TryGetValue(name, out var rawValue) && !string.IsNullOrWhiteSpace(rawValue))
        {
            value = rawValue;
            return true;
        }

        value = string.Empty;
        return false;
    }
}

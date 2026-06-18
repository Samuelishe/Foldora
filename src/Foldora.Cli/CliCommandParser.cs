namespace Foldora.Cli;

public static class CliCommandParser
{
    private static readonly HashSet<string> SkeletonCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "import-pack",
        "list-packs",
        "list-styles",
        "settings"
    };

    public static CliCommand Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0 || IsHelp(args[0]))
        {
            return new CliCommand(CliCommandKind.Help, "help");
        }

        var command = args[0].ToLowerInvariant();
        var options = ParseOptions(args.Skip(1).ToArray());

        return command switch
        {
            "menu" => ParseMenu(args.Skip(1).ToArray()),
            "apply" => ParseApply(options),
            "create" => ParseCreate(options),
            "clear" => ParseClear(options),
            "register-menu" => ParseRegisterMenu(options),
            "unregister-menu" => new CliCommand(CliCommandKind.UnregisterMenu, command),
            "quote" => new CliCommand(CliCommandKind.Quote, command, QuoteValue: args.Length > 1 ? args[1] : string.Empty),
            _ when SkeletonCommands.Contains(command) => new CliCommand(CliCommandKind.Skeleton, command),
            _ => new CliCommand(CliCommandKind.Unknown, command, Error: $"Unknown command '{command}'.")
        };
    }

    private static CliCommand ParseMenu(string[] args)
    {
        if (args.Length == 0)
        {
            return new CliCommand(CliCommandKind.Unknown, "menu", Error: "Missing menu subcommand.");
        }

        var subcommand = args[0].ToLowerInvariant();
        var options = ParseOptions(args.Skip(1).ToArray());

        return subcommand switch
        {
            "list" => new CliCommand(CliCommandKind.MenuList, "menu list"),
            "add" => ParseMenuAdd(options),
            "remove" => ParseMenuRemove(options),
            "reset" => ParseMenuReset(options),
            _ => new CliCommand(CliCommandKind.Unknown, $"menu {subcommand}", Error: $"Unknown menu subcommand '{subcommand}'.")
        };
    }

    private static CliCommand ParseApply(IReadOnlyDictionary<string, string?> options)
    {
        if (!TryGetRequiredOption(options, "--folder", out var folderError, out var folderPath))
        {
            return new CliCommand(CliCommandKind.Apply, "apply", Error: folderError);
        }

        var hasIcon = TryGetOptionalOption(options, "--icon", out var iconPath);
        var hasEntryId = TryGetOptionalOption(options, "--entry-id", out var entryId);

        if (hasIcon && hasEntryId)
        {
            return new CliCommand(CliCommandKind.Apply, "apply", Error: "Options --icon and --entry-id are mutually exclusive.");
        }

        if (!hasIcon && !hasEntryId)
        {
            return new CliCommand(CliCommandKind.Apply, "apply", Error: "Either --icon or --entry-id is required.");
        }

        return new CliCommand(CliCommandKind.Apply, "apply", FolderPath: folderPath, IconPath: iconPath, EntryId: entryId);
    }

    private static CliCommand ParseCreate(IReadOnlyDictionary<string, string?> options)
    {
        if (!TryGetRequiredOption(options, "--target", out var targetError, out var targetPath))
        {
            return new CliCommand(CliCommandKind.Create, "create", Error: targetError);
        }

        if (!TryGetRequiredOption(options, "--entry-id", out var entryIdError, out var entryId))
        {
            return new CliCommand(CliCommandKind.Create, "create", Error: entryIdError);
        }

        return new CliCommand(CliCommandKind.Create, "create", TargetPath: targetPath, EntryId: entryId);
    }

    private static CliCommand ParseClear(IReadOnlyDictionary<string, string?> options)
    {
        if (!TryGetRequiredOption(options, "--folder", out var folderError, out var folderPath))
        {
            return new CliCommand(CliCommandKind.Clear, "clear", Error: folderError);
        }

        return new CliCommand(CliCommandKind.Clear, "clear", FolderPath: folderPath);
    }

    private static CliCommand ParseMenuAdd(IReadOnlyDictionary<string, string?> options)
    {
        if (!TryGetRequiredOption(options, "--icon", out var iconError, out var iconPath))
        {
            return new CliCommand(CliCommandKind.MenuAdd, "menu add", Error: iconError);
        }

        options.TryGetValue("--name", out var displayName);
        options.TryGetValue("--folder-name", out var defaultFolderName);
        return new CliCommand(
            CliCommandKind.MenuAdd,
            "menu add",
            IconPath: iconPath,
            DisplayName: displayName,
            DefaultFolderName: defaultFolderName);
    }

    private static CliCommand ParseMenuRemove(IReadOnlyDictionary<string, string?> options)
    {
        if (!TryGetRequiredOption(options, "--entry-id", out var entryIdError, out var entryId))
        {
            return new CliCommand(CliCommandKind.MenuRemove, "menu remove", Error: entryIdError);
        }

        return new CliCommand(CliCommandKind.MenuRemove, "menu remove", EntryId: entryId);
    }

    private static CliCommand ParseMenuReset(IReadOnlyDictionary<string, string?> options)
    {
        if (!options.ContainsKey("--yes"))
        {
            return new CliCommand(
                CliCommandKind.MenuReset,
                "menu reset",
                Error: "Reset requires explicit confirmation. Re-run with --yes to clear menu entries and unregister Explorer integration.");
        }

        return new CliCommand(CliCommandKind.MenuReset, "menu reset", Yes: true);
    }

    private static CliCommand ParseRegisterMenu(IReadOnlyDictionary<string, string?> options)
    {
        options.TryGetValue("--cli-path", out var cliPath);
        return new CliCommand(
            CliCommandKind.RegisterMenu,
            "register-menu",
            CliExecutablePath: cliPath,
            DryRun: options.ContainsKey("--dry-run"));
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
        out string error,
        out string value)
    {
        if (!options.TryGetValue(name, out var rawValue) || string.IsNullOrWhiteSpace(rawValue))
        {
            error = $"Missing required option {name}.";
            value = string.Empty;
            return false;
        }

        error = string.Empty;
        value = rawValue;
        return true;
    }

    private static bool TryGetOptionalOption(
        IReadOnlyDictionary<string, string?> options,
        string name,
        out string? value)
    {
        if (options.TryGetValue(name, out var rawValue) && !string.IsNullOrWhiteSpace(rawValue))
        {
            value = rawValue;
            return true;
        }

        value = null;
        return false;
    }

    private static bool IsHelp(string value)
    {
        return value is "-h" or "--help" or "help";
    }
}

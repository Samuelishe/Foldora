using Foldora.Shell.ContextMenu;

if (args.Length == 0 || IsHelp(args[0]))
{
    PrintHelp();
    return 0;
}

var command = args[0].ToLowerInvariant();

switch (command)
{
    case "create":
    case "apply":
    case "clear":
    case "import-pack":
    case "list-packs":
    case "list-styles":
    case "settings":
        Console.WriteLine($"Command '{command}' is a bootstrap skeleton and is not implemented yet.");
        return 0;

    case "register-menu":
        Console.WriteLine("Command 'register-menu' is available only as an explicit future action. Registry writes are not implemented in bootstrap.");
        return 0;

    case "unregister-menu":
        Console.WriteLine("Command 'unregister-menu' is available only as an explicit future action. Registry writes are not implemented in bootstrap.");
        return 0;

    case "quote":
        Console.WriteLine(CommandLineQuoter.Quote(args.Length > 1 ? args[1] : string.Empty));
        return 0;

    default:
        Console.WriteLine($"Unknown command '{command}'.");
        PrintHelp();
        return 1;
}

static bool IsHelp(string value)
{
    return value is "-h" or "--help" or "help";
}

static void PrintHelp()
{
    Console.WriteLine("""
Foldora CLI bootstrap

Usage:
  foldora create --target "<directory>" --style "<style-id>"
  foldora apply --folder "<folder>" --style "<style-id>"
  foldora clear --folder "<folder>"
  foldora import-pack --path "<pack-path>"
  foldora list-packs
  foldora list-styles
  foldora register-menu
  foldora unregister-menu
  foldora settings

Commands are skeletons in the bootstrap build.
""");
}
